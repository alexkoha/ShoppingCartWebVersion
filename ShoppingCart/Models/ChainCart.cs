using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingCart.Models
{
    public class ChainCart
    {
        public string Name { get; set; }
        public double PriceCart { get; set; }
        public IEnumerable<Product> ProductsCart { get; set; }

        public bool Cheapest { get; set; }
    }
}
