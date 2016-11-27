using System;
using System.Collections.Generic;
using ShoppingCart.Models;

namespace ShoppingCart.EventsArgs
{
    public class UsersListUpdatedArgs : EventArgs
    {
        public IEnumerable<User> UsersList { get; set; }
    }
}