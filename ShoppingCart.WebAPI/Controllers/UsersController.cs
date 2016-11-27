using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ShoppingCart.Interfaces;
using ShoppingCart.Models;
using ShoppingCart.WebAPI.Models;
using WebGrease.Css.Extensions;

namespace ShoppingCart.WebAPI.Controllers
{
    public class UsersController : ApiController
    {

        private readonly IManager _app;

        public HttpResponseMessage Options()
        {
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
        }


        public UsersController(IManager app)
        {
            _app = app;
        }


        [Route("api/users/UserLogIn")]
        [HttpPost]
        public UserKey PostLogIn([FromBody] UserLogIn userLogIn)
        {
            var isExist = _app.Users.Any((user) => user.Name == userLogIn.Username && user.Password == userLogIn.Password);
            if (isExist)
            {
                var userLogedIn =
                    _app.Users.Single((user) => user.Name == userLogIn.Username && user.Password == userLogIn.Password);
                return new UserKey()
                {
                    UserId = userLogedIn.UserId,
                    Name = userLogedIn.Name
                };
                
            }
            var currentUser = _app.GetGuestUser;
            return new UserKey()
            {
                UserId = currentUser.UserId,
                Name = currentUser.Name
            };

        }


        [Route("api/users/UserSignUp")]
        [HttpPost]
        public bool PostSignUp([FromBody] UserLogIn userSignUp)
        {
            var isExcistUser = _app.Users.Any((user) => user.Name == userSignUp.Username);
            if (!isExcistUser)
            {
                return _app.SignUpNewUser(userSignUp.Username, userSignUp.Password);      
            }
            return false;
        }


        [Route("api/users/GuestUser")]
        [HttpGet]
        public UserKey GuestUser()
        {
            var currentUser = _app.CurrentUser;
            return new UserKey()
            {
                UserId = currentUser.UserId,
                Name = currentUser.Name
            };
        }

        [Route("api/users/{uname}")]
        [HttpGet]
        public bool IsUserExist(string uname)
        {
            var isExist = _app.Users.Any(user => user.Name == uname);
            return isExist;
        }

    }
}
