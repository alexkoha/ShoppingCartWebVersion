using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using ShoppingCart.Engines;
using ShoppingCart.EventsArgs;
using ShoppingCart.Interfaces;
using ShoppingCart.Models;

namespace ShoppingCart.Managers
{
    public sealed class UserManager : IManager
    {
        enum PriceMode
        {
            Cheap,
            Expensive
        }

        public event EventHandler<ProductCartListUpdatedArgs> ProductCartListUpdated;
        public event EventHandler<CurrentUserEventArgs> CurrentUserUpdated;
        public event EventHandler<UsersListUpdatedArgs> UsersListUpdated;
        public event EventHandler<CartPriceUpdatedArgs> CartPriceUpdated;

        public UserManager()
        {
            _cartProductsByItemKey = new Dictionary<ItemKey, IEnumerable<Product>>();
            _priceCartByChains = new Dictionary<Chain, double>();

            _users = UsersEngine.Users;
            _currentUser = UsersEngine.GetGeustUser();
            
            RepositoryEngine.ChainsArchive
                .ToList()
                .ForEach(chainModel => _priceCartByChains[chainModel] = 0);
        }

        private User _currentUser;
        public User CurrentUser =>_currentUser;

        public User GetGuestUser => _userEngine.GetGeustUser();

        public IEnumerable<User> Users => _users;
        private IEnumerable<User> _users;

        private RepositoryEngine _repositoryEngine;
        private RepositoryEngine RepositoryEngine => _repositoryEngine ?? (_repositoryEngine = new RepositoryEngine());

        private UsersEngine _userEngine;
        private UsersEngine UsersEngine => _userEngine ?? (_userEngine = new UsersEngine());

        private IDictionary<Chain, double> _priceCartByChains;

        public IEnumerable<ItemKey> ListOfProducts=> _listOfProducts ?? (_listOfProducts = RepositoryEngine.GetItemsList());
        private IEnumerable<ItemKey> _listOfProducts;


        private IDictionary<ItemKey, IEnumerable<Product>> _cartProductsByItemKey;
        public ICollection<ItemKey> ProductsInCart => _currentUser.ItemsInCart;

        public ICollection<ItemKey> ProductsInCartUserId(string userId)
        {
            var userChoosed = _userEngine.Users.SingleOrDefault(user => user.UserId == userId);
            if (userChoosed != null)
                return userChoosed.ItemsInCart;

            return new List<ItemKey>();
        }

        

        private IDictionary<Chain, Tuple<IEnumerable<Product>, IEnumerable<Product>>>_cheapestAndExpentivestProductsByChains;
        public IDictionary<Chain, Tuple<IEnumerable<Product>, IEnumerable<Product>>> CheapestAndExpentivestProductsByChains
            => _cheapestAndExpentivestProductsByChains;
        
        private double GetPriceCartChain(string id)
        {
            var chain = RepositoryEngine.GetChainById(id);
            var cartPrice = _priceCartByChains[chain];
            return cartPrice;
        }

        private IEnumerable<Product> GetCheapestExpensiveProductsChain(string id , PriceMode mode)
        {
            var chain = RepositoryEngine.GetChainById(id);
            if(mode==PriceMode.Cheap)
                return CheapestAndExpentivestProductsByChains[chain].Item1;
            if (mode==PriceMode.Expensive)
                return CheapestAndExpentivestProductsByChains[chain].Item2;
            return new List<Product>();
        }

        public IEnumerable<Chain> GetChaines => _repositoryEngine.ChainsArchive;

        public IEnumerable<ChainCart> GetPricesAndCart()
        {
            var productsByChains = RepositoryEngine.GetProductsByChains();
            var list = new List<ChainCart>();

            productsByChains.ForEach(pair =>
            {
                list.Add(new ChainCart()
                {
                    Name = pair.Key.ChainName,
                    PriceCart = _priceCartByChains[pair.Key],
                    ProductsCart = pair.Value
                });
            });
            return list;
        }

        public void AddItemToCart(ItemKey itemKey, int quantity)
        {
            UsersEngine.AddItemToCart(_currentUser,itemKey, quantity);
            OnProductCartListUpdates();
        }
        public bool RemoveItemFromCart(ItemKey itemKey)
        {
            var isRemoved =  UsersEngine.RemoveItemFromCart(_currentUser,itemKey);
            if(isRemoved)
                OnProductCartListUpdates();
            return isRemoved;
        }

        public bool RemoveItemFromCart(ItemKey itemKey, string userId) //<=====
        {
            var isRemoved = UsersEngine.RemoveItemFromCart(userId, itemKey);
            if (isRemoved)
                OnProductCartListUpdates();
            return isRemoved;
        }

        public IEnumerable<ChainCart> GetPricesAndCart(string userId) //<====           
        {
            var cart = _userEngine.Users.Single(user => user.UserId == userId).ItemsInCart;
            var productsByChains = RepositoryEngine.GetProductsByChains(cart);
            var list = new List<ChainCart>();
            var priceCartByChains = RepositoryEngine.GetCartPrice(cart);

            productsByChains.ForEach(pair =>
            {
                list.Add(new ChainCart()
                {
                    Name = pair.Key.ChainName,
                    PriceCart = priceCartByChains[pair.Key],
                    ProductsCart = pair.Value
                });
            });
            return list;
        }

        public void CalculateCart()
        {
            _priceCartByChains = RepositoryEngine.GetCartPrice(ProductsInCart);
            _cartProductsByItemKey = RepositoryEngine.GetPrudcutsCart(ProductsInCart);
            _cheapestAndExpentivestProductsByChains = RepositoryEngine.GetChipestExpentivestProductsByChains();
            
            OnCartPriceUpdated();
        }
        public void SaveShoppingCart(string file) 
        {
            UsersEngine.SaveShoppingCart(_currentUser,file);
        }
        public void LoadShoppingCart(string file)
        {
            UsersEngine.LoadShoppingCart(_currentUser,file);
            OnProductCartListUpdates();
        }

        public async void CreatExcelFileShoppingCart(string fileName)
        {
            await RepositoryEngine.CeateAndSaveExcelFileProductCartAsync(fileName, _cartProductsByItemKey);
        }
        
        public bool SignUpNewUser(string name,string pass)
        {
            var userAdded = _userEngine.AddUser(name,pass);
            if (userAdded)
            {
                _users = _userEngine.Users;
                OnUsersListUpdated();
                return true;
            }
            return false;
        }
        public void ChooseCurrentUser(User user)
        {
            _currentUser = _userEngine.IsContainsUser(user) ? 
                _userEngine.Users.Single(x => (x.UserId == user.UserId)) : _userEngine.GetGeustUser();

            OnCurrentUserUpdated();
            OnProductCartListUpdates();
        }

        public void ChooseCurrentUserById(string userId)
        {
            _currentUser = _userEngine.IsContainsUserByID(userId) ?
                _userEngine.Users.Single(x => (x.UserId == userId)) : _userEngine.GetGeustUser();

            OnCurrentUserUpdated();
            OnProductCartListUpdates();
        }

        public bool RemoveUser(User user)
        {
            bool isUserRemoved = false;
            if (_currentUser == user)
            {
                isUserRemoved = _userEngine.RemoveUser(user);
                if (isUserRemoved)
                {
                    _currentUser = _userEngine.GetGeustUser();
                    CalculateCart();

                    OnCurrentUserUpdated();
                    OnProductCartListUpdates();
                    OnCartPriceUpdated();
                    return true;
                }

            }
            else
                isUserRemoved = _userEngine.RemoveUser(user);

            if (isUserRemoved)
            {
                OnUsersListUpdated();
                return true;
            }
            return false;
        }

        public IEnumerable<ChainCart> GetPricesAndCart(ICollection<ItemKey> cart) 
        {
            var productsByChains = RepositoryEngine.GetProductsByChains(cart);
            var list = new List<ChainCart>();
            var priceCartByChains = RepositoryEngine.GetCartPrice(cart);

            productsByChains.ForEach(pair =>
            {
                list.Add(new ChainCart()
                {
                    Name = pair.Key.ChainName,
                    PriceCart = priceCartByChains[pair.Key],
                    ProductsCart = pair.Value
                });
            });
            return list;
        }

        public void UpdateProductsQuantityInCart(IEnumerable<ItemKey> cart, string userId)
        {

            _userEngine.UpdateCartQuantity(cart,userId);
            
        }

        public void AddItemToCart(ItemKey itemKey, int quantity, string userId) 
        {
            var currentUser = _userEngine.Users.SingleOrDefault(user => user.UserId == userId);

            UsersEngine.AddItemToCart(currentUser, itemKey, quantity);
        }

        private void OnProductCartListUpdates()
        {
            var args = new ProductCartListUpdatedArgs()
            {
                ListProductsInCart = _currentUser.ItemsInCart
            };

            ProductCartListUpdated?.Invoke(this, args);
        }
        private void OnCurrentUserUpdated()
        {
            var args = new CurrentUserEventArgs
            {
                UserId = _currentUser.UserId,
                UserName = _currentUser.Name
            };

            CurrentUserUpdated?.Invoke(this, args);
        }
        private void OnUsersListUpdated()
        {
            var args = new UsersListUpdatedArgs()
            {
                UsersList = _users
            };
            UsersListUpdated?.Invoke(this, args);
        }
        private void OnCartPriceUpdated()
        {
            var args = new CartPriceUpdatedArgs()
            {
                ViktoryChipestProducts = GetCheapestExpensiveProductsChain("1", PriceMode.Cheap),
                ViktoryExpensiveProducts = GetCheapestExpensiveProductsChain("1", PriceMode.Expensive),
                ShookHaairChipestProducts = GetCheapestExpensiveProductsChain("2", PriceMode.Cheap),
                ShookHaairExpensiveProducts = GetCheapestExpensiveProductsChain("2", PriceMode.Expensive),
                MahsaneyHashookChipestProducts = GetCheapestExpensiveProductsChain("3", PriceMode.Cheap),
                MahsaneyHashookExpensiveProducts = GetCheapestExpensiveProductsChain("3", PriceMode.Expensive),

                ViktoryCartPrice = GetPriceCartChain("1"),
                ShookHaairCartPrice = GetPriceCartChain("2"),
                MahsaneyHashookCartPrice = GetPriceCartChain("3")
            };

            CartPriceUpdated?.Invoke(this,args);
        }

    }
}
