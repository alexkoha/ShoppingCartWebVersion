using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ShoppingCart.Interfaces;
using ShoppingCart.Models;
using ShoppingCart.Resources;

namespace ShoppingCart.Accessors
{
    public class NibitXmlAccessor : IXmlAccessor
    {        
        private string _xmlPath;
        private string _xmlFileName;

        public NibitXmlAccessor()
        {
            _xmlPath = PathsInfo.XmlPath;
        }

        public string GetFileName()
        {
            return _xmlFileName;
        }
        public string GetFilePath()
        {
            return _xmlPath;
        }

        public Product GetProductByCode(string xmlFileName , Item item)
        {
            _xmlFileName = xmlFileName;
            Product productItem;

            using (var xmlReader = new StreamReader(_xmlPath+xmlFileName))
            {
                var doc = XDocument.Load(xmlReader);

                var nodePrices = doc.Element("Prices");
                if(nodePrices==null)
                    throw new Exception("Cant find Prices element");

                var node = nodePrices.Element("Products");
                if (node == null)
                    throw new Exception("Cant find Products element int Prices element");

                try
                {
                    var element = node.Elements("Product").
                        Single(product => product.Element("ItemCode").Value.
                            Equals(item.ItemCode));

                    double itemPrice;
                    if (!double.TryParse(element.Element("ItemPrice").Value, out itemPrice))
                        itemPrice = 0;

                    productItem = new Product
                    {
                        ProductName = item.ProductName,
                        CaindId = item.ChainId,
                        ItemCode = element.Element("ItemCode").Value,
                        UnitQty = element.Element("UnitQty").Value,
                        ItemPrice = itemPrice
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("cant find product :" + item.ItemCode + " " + item.ProductName + " " + item.ChainId + "in xml file name " + xmlFileName +"\n"+ ex.Message);
                    productItem = new Product
                    {
                        ProductName = "Out of Stock " + item.ProductName,
                        CaindId = item.ChainId,
                        ItemCode = "0",
                        UnitQty = "-",
                        ItemPrice = 0
                    };
                }
            }

            return productItem;
        }
    }
}
