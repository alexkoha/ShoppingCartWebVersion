using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShoppingCart.Models;

namespace ShoppingCart.Interfaces
{
    public interface IRepositoryEngine
    {
        void UpdateXmlFilesAsync();
        string GetChainName(string chainId);
        string GetCahinXmlFileName(string chainId);

        IDictionary<Chain, double> GetCartPrice(ICollection<ItemKey> selectedProducts);
        IDictionary<Chain, Tuple<IEnumerable<Product>, IEnumerable<Product>>> GetChipestExpentivestProductsByChains();
        IDictionary<ItemKey, IEnumerable<Product>> GetPrudcutsCart(ICollection<ItemKey> selectedProducts);
        IEnumerable<ItemKey> GetItemsList();

        Chain GetChainById(string id);
        Task CeateAndSaveExcelFileProductCartAsync(string fileName,IDictionary<ItemKey, IEnumerable<Product>> cartProductsByItemKey);
    }
}