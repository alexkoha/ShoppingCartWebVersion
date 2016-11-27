using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingCart.WebAPI.Models
{
    public class ProductKey
    {
        public string CaindId { get; internal set; }
        public string ItemCode { get; internal set; }
        public string ProductName { get; internal set; }
    }
}