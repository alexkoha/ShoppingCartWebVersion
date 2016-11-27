using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using Microsoft.Practices.ObjectBuilder2;
using ShoppingCart.Accessors;
using ShoppingCart.Interfaces;
using ShoppingCart.Models;
using ShoppingCart.Resources;

namespace ShoppingCart.Engines
{
    public class RepositoryEngine : IRepositoryEngine
    {
        private readonly string _xmlPath;
        private readonly object _locker;
        private readonly object _fileAccessLocker;

        private IDictionary<ItemKey, IEnumerable<Product>> _productsArchive;
        public IDictionary<ItemKey, IEnumerable<Product>> ProductsArchive
        {
            get
            {
                IDictionary<ItemKey, IEnumerable<Product>> result;
                lock (_locker)
                {
                    result = _productsArchive;
                }
                return result;
            }
            set
            {
                lock (_locker)
                {
                    _productsArchive = value;
                }
            }
        }

        public IEnumerable<Chain> ChainsArchive => _chainsArchive;
        private IEnumerable<Chain> _chainsArchive;

        private IEnumerable<string> _listChainId ;
        public IEnumerable<string> ListChainId => _listChainId?? 
            (_listChainId=ChainsArchive.Select(chain => chain.ChainId));

        private IDictionary<Chain, ICollection<Product>> _productsCartByChain;

        public RepositoryEngine()
        {
            _xmlPath = PathsInfo.XmlPath;
            _locker = new object();
            _fileAccessLocker = new object();

            _productsCartByChain = new Dictionary<Chain, ICollection<Product>>();
            var chainsListAccessor = new ChainsListAccessor();
            _chainsArchive = chainsListAccessor.GetChainsList();

            CheckAllXmlFilesExist();
            _productsArchive = ConvertItemsToProduct();

            //Task.Run(()=>UpdateXmlFilesAsync()); ##### Disable Update #####
        }

        public void CheckAllXmlFilesExist()
        {
            var accessor = new ItemListAccessor();
            var fileLocation = accessor.GetFilePath() + accessor.GetFileName();
            var isFileItemsListExist = File.Exists(fileLocation);
            if(!isFileItemsListExist)
                throw new Exception($"File {fileLocation} not found");

            foreach (var chain in ChainsArchive)
            {
                var chainId = chain.ChainId;
                var chainXmlFileName = GetCahinXmlFileName(chainId);
                fileLocation = _xmlPath + chainXmlFileName;

                var isFileExist = File.Exists(fileLocation);
                if (!isFileExist)
                    throw new Exception($"File {fileLocation} not found");
            }
        }

        private IDictionary<Chain, ICollection<Product>> GetProductsCartByChains(IEnumerable<ItemKey> selectedProducts)
        {
            var productsInChainDictionary = new Dictionary<Chain, ICollection<Product>>();
            var allSelectedProducts = selectedProducts.SelectMany(itemKey => ProductsArchive[itemKey]);

            foreach (var product in allSelectedProducts)
            {
                var chainModel = ChainsArchive.Single(x => x.ChainId == product.CaindId);
                if (!productsInChainDictionary.ContainsKey(chainModel))
                    productsInChainDictionary[chainModel] = new List<Product>();

               
                productsInChainDictionary[chainModel].Add(product);
            }
            return productsInChainDictionary;
        }

        private IDictionary<ItemKey , IEnumerable<Product>> ConvertItemsToProduct()
        {
            var itemListAccessor = new ItemListAccessor();
            var itemsDictionary = itemListAccessor.GetItemDictionary();

            var dictionaryProducts =  new Dictionary<ItemKey , IEnumerable<Product>>();

            foreach (KeyValuePair<ItemKey, IEnumerable<Item>> pair in itemsDictionary)
            {
                var listOfProducts = GetProductList(pair.Value);
                dictionaryProducts.Add(pair.Key , listOfProducts);
            }
            return dictionaryProducts;
        }

        private IEnumerable<Product> GetProductList(IEnumerable<Item> itemsList)
        {
            var listProducts = new List<Product>();
            var nibitAccessor = new NibitXmlAccessor();

            lock (_fileAccessLocker)
            {
                foreach (var item in itemsList)
                {
                    var chainId = item.ChainId;
                    var chainXmlFileName = GetCahinXmlFileName(chainId);

                    var element = nibitAccessor.GetProductByCode(chainXmlFileName, item);
                    listProducts.Add(element);
                }
            }
            return listProducts;
        }

        public async void UpdateXmlFilesAsync()
        {
            try
            {
                if (!CheckForInternetConnection())
                    return;

                foreach (var chain in _chainsArchive)
                {
                    var nibitWebAccessor = new NibitWebAccessor();

                    var url = nibitWebAccessor.GetNewerXmlFileLink(chain);
                    var client = new WebClient();
                    var uri = new Uri(url);
                    var name = Path.GetFileName(uri.LocalPath);

                    await client.DownloadFileTaskAsync(uri, $"{_xmlPath}{name}");
                    await UnzipFileAsync(name, chain);

                    Debug.WriteLine($"Downloaded {name}"); 
                }
                _productsArchive = ConvertItemsToProduct();
                Debug.WriteLine($"Updated Done");
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Update files failed :"+ exception.Message);
            }
        }
        private bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        private async Task UnzipFileAsync(string name, Chain chain)
        {
            await Task.Run(() =>
            {
                lock (_fileAccessLocker)
                {
                    using (FileStream fileToDecompressAsStream = new FileStream($"{_xmlPath}{name}",
                        FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                    {
                        string decompressedFile = $"{_xmlPath}{chain.ChainName}.xml";

                        using (FileStream decompressedStream = File.Create(decompressedFile))
                        {
                            using (GZipStream decompressionStream = new GZipStream(fileToDecompressAsStream,
                                CompressionMode.Decompress))
                            {
                                decompressionStream.CopyTo(decompressedStream);
                            }
                        }
                    }
                }
                File.Delete($"{_xmlPath}{name}");
            });
        }

        public IDictionary<Chain, double> GetCartPrice(ICollection<ItemKey> selectedProducts)
        {
            var pricesCartByChains = new Dictionary<Chain, double>();
            foreach (var chain in ChainsArchive)
            {
                pricesCartByChains[chain] = 0;
            }
            _productsCartByChain = GetProductsCartByChains(selectedProducts);

            if (selectedProducts == null || !selectedProducts.Any()) return pricesCartByChains;

            foreach (var itemKey in selectedProducts)
            {
                foreach (var product in ProductsArchive[itemKey])
                {
                    var selectedChain = ChainsArchive.Single(chain => chain.ChainId == product.CaindId);
                    if (!pricesCartByChains.ContainsKey(selectedChain))
                    {
                        pricesCartByChains[selectedChain] = 0;
                    }
                    pricesCartByChains[selectedChain] += product.ItemPrice * itemKey.Quantity;
                }
            }
            return pricesCartByChains;
        }

        public IDictionary<Chain ,Tuple<IEnumerable<Product>, IEnumerable<Product>>> GetChipestExpentivestProductsByChains()
        {
            var chipestExpentivestProductsByChains = new Dictionary<Chain, Tuple<IEnumerable<Product>, IEnumerable<Product>>>();
            if (!_productsCartByChain.Any())
            {
                foreach (var chain in ChainsArchive)
                {
                    chipestExpentivestProductsByChains[chain] = new Tuple<IEnumerable<Product>, IEnumerable<Product>>(null,null);
                }
            }
            else
            {
                foreach (var chain in ChainsArchive)
                {
                    var productOfChain = _productsCartByChain[chain];
                    var orderedCartChain = productOfChain.Where(x=>x.ItemPrice>0).OrderBy(x => x.ItemPrice).ToList();

                    if (_productsCartByChain[chain].Count < 4)
                    {
                        chipestExpentivestProductsByChains[chain] =
                            new Tuple<IEnumerable<Product>, IEnumerable<Product>>(
                                orderedCartChain,
                                orderedCartChain);
                    }
                    else
                    {
                        chipestExpentivestProductsByChains[chain] =
                            new Tuple<IEnumerable<Product>, IEnumerable<Product>>(
                                orderedCartChain.GetRange(0, 3),
                                orderedCartChain.GetRange(orderedCartChain.Count - 3, 3)
                                );
                    }
                }
            }
            return chipestExpentivestProductsByChains; 
        }


        public IDictionary<Chain, IEnumerable<Product>> GetProductsByChains() // <=== new added
        {
              var productsByChains = new Dictionary<Chain, IEnumerable<Product>>();

            if (!_productsCartByChain.Any())
            {
                foreach (var chain in ChainsArchive)
                {
                    productsByChains[chain] = new List<Product>();
                }
            }
            else
            {
                foreach (var chain in ChainsArchive)
                {
                    productsByChains[chain] = _productsCartByChain[chain];
                    var productOfChain = _productsCartByChain[chain];

                    var orderedCartChain = productOfChain.Where(x => x.ItemPrice > 0).OrderBy(x => x.ItemPrice).ToList();

                    if (_productsCartByChain[chain].Count > 4)
                    {
                        productsByChains[chain].ForEach(pro =>
                        {
                            orderedCartChain.GetRange(0, 2).ForEach(cheap =>
                            {
                                if(pro.ItemCode==cheap.ItemCode)
                                    pro.PriceLevel = PriceLevel.Cheap;
                            });
                            orderedCartChain.GetRange(orderedCartChain.Count - 2, 2).ForEach(exp =>
                            {
                                if (pro.ItemCode == exp.ItemCode)
                                    pro.PriceLevel = PriceLevel.Expensive;
                            });
                        });
                    }
                }
            }
            return productsByChains;
        }

        public IDictionary<Chain, ICollection<Product>> GetProductsByChains(ICollection<ItemKey> cart) // <=== new added
        {
            var productsInCart = GetProductsCartByChains(cart);

            var productsByChains = new Dictionary<Chain, ICollection<Product>>();

            if (!productsInCart.Any())
            {
                foreach (var chain in ChainsArchive)
                {
                    productsByChains[chain] = new List<Product>();
                }
            }
            else
            {
                foreach (var chain in ChainsArchive)
                {
                    productsByChains[chain] = new List<Product>();
                    productsInCart[chain].ForEach(pro => productsByChains[chain].Add(new Product()
                    {
                        ItemCode = pro.ItemCode,
                        CaindId = pro.CaindId,
                        Quantity = pro.Quantity,
                        PriceLevel = pro.PriceLevel,
                        ProductName = pro.ProductName,
                        ItemPrice = pro.ItemPrice,
                        UnitQty = pro.UnitQty
                    }));

                    var productOfChain = productsInCart[chain];

                    var orderedCartChain = productOfChain.Where(x => x.ItemPrice > 0).OrderBy(x => x.ItemPrice).ToList();

                    if (productsInCart[chain].Count > 3)
                    {
                        productsByChains[chain].ForEach(pro =>
                        {
                            orderedCartChain.GetRange(0, 2).ForEach(cheap =>
                            {
                                if(pro.ItemCode==cheap.ItemCode)
                                    pro.PriceLevel = PriceLevel.Cheap;
                            });
                            orderedCartChain.GetRange(orderedCartChain.Count - 2, 2).ForEach(exp =>
                            {
                                if (pro.ItemCode == exp.ItemCode)
                                    pro.PriceLevel = PriceLevel.Expensive;
                            });
                        });
                    }
                }
            }
            return productsByChains;
        }

        public IDictionary<ItemKey, IEnumerable<Product>> GetPrudcutsCart(ICollection<ItemKey> selectedProducts)
        {
            var pricesCartByChains = new Dictionary<ItemKey, IEnumerable<Product>>();

            foreach (var itemKey in selectedProducts)
            {
                pricesCartByChains[itemKey] = ProductsArchive[itemKey];
            }
            return pricesCartByChains;
        }

        public string GetChainName(string chainId)
        {
            return _chainsArchive.Single(chain => chain.ChainId == chainId).ChainName;
        }
        public string GetCahinXmlFileName(string chainId)
        {
            return _chainsArchive.Single(chain => chain.ChainId == chainId).ChainXmlFileName;
        }
        public async Task CeateAndSaveExcelFileProductCartAsync(string fileName, IDictionary<ItemKey, IEnumerable<Product>> cartProductsByItemKey)
        {
            await Task.Run(() =>
            {
                object misValue = Missing.Value;

                var xlApp = new Application();
                var xlWorkBook = xlApp.Workbooks.Add(misValue);
                var xlWorkSheet = (Worksheet) xlWorkBook.Worksheets.Item[1];

                xlWorkSheet.Cells[1, 1] = "";
                var numberOfProducts = cartProductsByItemKey.Keys.Count;

                foreach (var pair in cartProductsByItemKey)
                {
                    var counter = pair.Value.Count();

                    xlWorkSheet.Cells[2 + numberOfProducts, 1] = pair.Value.First().ProductName;

                    foreach (var item in pair.Value)
                    {
                        var chainName = GetChainName(item.CaindId);
                        xlWorkSheet.Cells[1, 1 + counter] = chainName;
                        xlWorkSheet.Cells[2 + numberOfProducts, 1 + counter] = item.ItemPrice;
                        counter--;
                    }
                    numberOfProducts--;
                }

                numberOfProducts = cartProductsByItemKey.Keys.Count;
                var numberOfChains = cartProductsByItemKey.Values.First().Count();

                ChartObjects xlCharts =
                    (ChartObjects) xlWorkSheet.ChartObjects(Type.Missing);
                ChartObject myChart =
                    xlCharts.Add(10, 80, 300, 250);
                Chart chartPage = myChart.Chart;

                var chartRange =
                    xlWorkSheet.Range[
                        (Range) xlWorkSheet.Cells[1, 1],
                        (Range)
                            xlWorkSheet.Cells[numberOfProducts + 2, numberOfChains + 1]];
                chartPage.SetSourceData(chartRange, misValue);
                chartPage.ChartType = XlChartType.xlColumnClustered;

                xlWorkBook.SaveAs(fileName, XlFileFormat.xlWorkbookNormal, misValue,
                    misValue, misValue, misValue,
                    XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue,
                    misValue, misValue);
                xlWorkBook.Close(true, misValue, misValue);
                xlApp.Quit();
            });
        }

        public IEnumerable<ItemKey> GetItemsList()
        {
            var accessor = new ItemListAccessor();

            return accessor.GetItemDictionary().Select(x => x.Key).ToList();
        }
        public Chain GetChainById(string id)
        {
            return _chainsArchive.Single(x => x.Id == id);
        }
    }
}   