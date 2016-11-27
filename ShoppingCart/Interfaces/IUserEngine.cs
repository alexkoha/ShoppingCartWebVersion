using ShoppingCart.Models;

namespace ShoppingCart.Interfaces
{
    public interface IUserEngine
    {
        User GetGeustUser();
        bool AddUser(string name,string password);
        bool RemoveUser(User user);
        bool IsContainsUser(User user);
        bool IsContainsUserByID(string userId);

        void AddItemToCart(User currentUser, ItemKey itemKey, int quantity);
        bool RemoveItemFromCart(User currentUser, ItemKey itemKey);
        void LoadShoppingCart(User currentUser, string fileLocation);
        void SaveShoppingCart(User currentUser, string fileLocation);
    }
}