using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ShoppingCart.WebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            var cors = new EnableCorsAttribute("*", "*", "*");
            cors.SupportsCredentials = true;
            config.EnableCors(cors);

            // Web API configuration and services
            ((Newtonsoft.Json.Serialization.DefaultContractResolver)config.Formatters.JsonFormatter.SerializerSettings.ContractResolver).IgnoreSerializableAttribute = true;

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
            name: "DefaultApi2",
            routeTemplate: "api/{controller}/{id}/{id2}/{id3}/{id4}",
            defaults: new { id = RouteParameter.Optional ,id2 = RouteParameter.Optional , id3 = RouteParameter.Optional  , id4=RouteParameter.Optional}
             );
        }
    }
}
