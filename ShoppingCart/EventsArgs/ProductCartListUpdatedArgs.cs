using System;
using System.Collections.Generic;
using ShoppingCart.Models;

namespace ShoppingCart.EventsArgs
{
    public class ProductCartListUpdatedArgs : EventArgs
    {
        public ICollection<ItemKey> ListProductsInCart { get; set; }
    }
}