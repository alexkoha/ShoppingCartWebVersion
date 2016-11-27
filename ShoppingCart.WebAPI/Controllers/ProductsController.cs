using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ShoppingCart.Interfaces;
using ShoppingCart.Models;

namespace ShoppingCart.WebAPI.Controllers
{
    public class ProductsController : ApiController
    {
        private readonly IManager _app;

        public HttpResponseMessage Options()
        {
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
        }

        public ProductsController(IManager app)
        {
            _app = app;
        }

        [HttpGet]
        public IEnumerable<ItemKey> Get()
        {
            return _app.ListOfProducts;
        }



    }
}
