using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using ShoppingCart.Interfaces;
using ShoppingCart.Models;

namespace ShoppingCart.WebAPI.Controllers
{
    public class CartController : ApiController
    {
        private readonly IManager _app;

        public HttpResponseMessage Options()
        {
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
        }


        public CartController(IManager app)
        {
            _app = app;
        }


        [Route("api/Cart/add/{uid}/{pid}/{qnt}")]
        [HttpPost]
        public void PostAddProduct(string uid,int pid,int qnt)
        {

            var product = _app.ListOfProducts.SingleOrDefault(pro => pro.ItemId == pid);
            _app.AddItemToCart(product,qnt,uid);
        }

        [Route("api/Cart/remove/{uid}/{pid}")]
        [HttpPost]
        public bool PostRemoveProduct(string uid, int pid)
        {
            var product = _app.ListOfProducts.SingleOrDefault(pro => pro.ItemId == pid);
            return _app.RemoveItemFromCart(product, uid);
        }


    }
}
