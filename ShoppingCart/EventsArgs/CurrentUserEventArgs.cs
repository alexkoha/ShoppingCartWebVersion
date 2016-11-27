using System;

namespace ShoppingCart.EventsArgs
{
    public class CurrentUserEventArgs : EventArgs
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
    }
}