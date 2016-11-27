using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ShoppingCart.Models
{
    [Serializable]
    public class User : IEquatable<User> 
    {
        public User()
        {
            ItemsInCart = new List<ItemKey>();
        }

        public string Name { get; internal set; }
        public string UserId { get; internal set; }
        public string Password { get; internal set; }
        public ICollection<ItemKey> ItemsInCart { get; internal set; }
    
        public bool Equals(User user)
        {
            return (Name == user.Name);
        }

    }
}