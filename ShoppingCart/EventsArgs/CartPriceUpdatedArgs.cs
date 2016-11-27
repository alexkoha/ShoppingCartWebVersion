using System;
using System.Collections.Generic;
using ShoppingCart.Models;

namespace ShoppingCart.EventsArgs
{
    public class CartPriceUpdatedArgs : EventArgs
    {
        public IEnumerable<Product> MahsaneyHashookExpensiveProducts { get; set; }
        public IEnumerable<Product> MahsaneyHashookChipestProducts { get; set; }
        public IEnumerable<Product> ShookHaairExpensiveProducts { get; set; }
        public IEnumerable<Product> ShookHaairChipestProducts { get; set; }
        public IEnumerable<Product> ViktoryExpensiveProducts { get; set; }
        public IEnumerable<Product> ViktoryChipestProducts { get; set; }
        public double ViktoryCartPrice { get; set; }
        public double ShookHaairCartPrice { get; set; }
        public double MahsaneyHashookCartPrice { get; set; }
    }
}