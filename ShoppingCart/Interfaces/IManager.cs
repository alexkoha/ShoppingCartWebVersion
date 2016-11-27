using System;
using System.Collections.Generic;
using ShoppingCart.EventsArgs;
using ShoppingCart.Models;

namespace ShoppingCart.Interfaces
{
    public interface IManager
    {
        event EventHandler<ProductCartListUpdatedArgs> ProductCartListUpdated;
        event EventHandler<CurrentUserEventArgs> CurrentUserUpdated;
        event EventHandler<UsersListUpdatedArgs> UsersListUpdated;
        event EventHandler<CartPriceUpdatedArgs> CartPriceUpdated;

        IEnumerable<ItemKey> ListOfProducts { get;}
        IEnumerable<User> Users { get;}
        ICollection<ItemKey> ProductsInCart { get;}
        User CurrentUser { get;}
        IEnumerable<Chain> GetChaines { get;}
        IEnumerable<ChainCart> GetPricesAndCart();

        void AddItemToCart(ItemKey itemKey, int quantity);
        bool RemoveItemFromCart(ItemKey itemKey);
        void CalculateCart();
        void SaveShoppingCart(string file);
        void LoadShoppingCart(string file);
        void CreatExcelFileShoppingCart(string fileName); //<=======
        bool SignUpNewUser(string name,string pass);
        void ChooseCurrentUser(User user);
        void ChooseCurrentUserById(string userId);
        bool RemoveUser(User user);


        User GetGuestUser { get; }
        ICollection<ItemKey> ProductsInCartUserId(string userId);

        void AddItemToCart(ItemKey itemKey, int quantity, string userId);
        bool RemoveItemFromCart(ItemKey itemKey, string userId);
        IEnumerable<ChainCart> GetPricesAndCart(string userId);
        IEnumerable<ChainCart> GetPricesAndCart(ICollection<ItemKey> cart);

        void UpdateProductsQuantityInCart(IEnumerable<ItemKey> cart , string userId);
    }
}