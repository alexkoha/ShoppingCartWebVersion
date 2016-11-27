using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Practices.ObjectBuilder2;
using ShoppingCart.EventsArgs;
using ShoppingCart.Interfaces;
using ShoppingCart.Models;
using ShoppingCart.WebAPI.Models;

namespace ShoppingCart.WebAPI.Controllers
{
    public class ShoppingCartController : ApiController
    {
        private readonly IManager _app;

        public HttpResponseMessage Options()
        {
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
        }


        public ShoppingCartController(IManager app)
        {
            _app = app;
        }

        [Route("api/ShoppingCart/ProductsInCart")]
        [HttpGet]
        public ICollection<ItemKey> GetProductsInCart()
        {
            return _app.ProductsInCart;
        }

        [Route("api/ShoppingCart/ProductsInCart/{uid}")]
        [HttpGet]
        public ICollection<ItemKey> GetProductsInCartById(string uid)
        {
            var list = _app.ProductsInCartUserId(uid);
            return list;
        }

        [Route("api/ShoppingCart/PricesOfChaines/{uid}")]
        [HttpGet]
        public IEnumerable<ChainCart> GetPricesOfChaines(string uid)
        {
            return _app.GetPricesAndCart(uid);
        }


        [Route("api/ShoppingCart/PricesOfChainesDynamic")]
        [HttpPost]
        public IEnumerable<ChainCart> GetPricesOfChaines([FromBody]ICollection<ItemKey> cart)
        {
            return _app.GetPricesAndCart(cart);
        }

        [Route("api/ShoppingCart/PricesOfChainesDynamic/{uid}")]
        [HttpPost]
        public IEnumerable<ChainCart> GetPricesOfChainesUser([FromBody]ICollection<ItemKey> cart,string uid)
        {
             _app.UpdateProductsQuantityInCart(cart , uid);
             return _app.GetPricesAndCart(uid);
        }

        [Route("api/ShoppingCart/Chaines")]
        [HttpGet]
        public IEnumerable<ChainCart> GetChaines()
        {
            return _app.GetPricesAndCart();
        }

        [Route("api/ShoppingCart/add/{uid}/{pid}/{qnt}")]
        [HttpPost]
        public void PostAdd(string uid, int pid, int qnt)
        {

            var product = _app.ListOfProducts.SingleOrDefault(pro => pro.ItemId == pid);
            _app.AddItemToCart(product, qnt, uid);
        }


    }
}
