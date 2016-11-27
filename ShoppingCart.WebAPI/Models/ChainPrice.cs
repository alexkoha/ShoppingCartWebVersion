using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ShoppingCart.Models;

namespace ShoppingCart.WebAPI.Models
{
    public class ChainPrice
    {
        public string Name { set; get; }
        public double Price { set; get; }
        public IEnumerable<Product> Products { set; get; }
    }
}