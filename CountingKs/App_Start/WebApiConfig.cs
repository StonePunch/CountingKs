using CacheCow.Server;
using CacheCow.Server.EntityTagStore.SqlServer;
using CountingKs.Converters;
using CountingKs.Filters;
using CountingKs.Services;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Dispatcher;
using WebApiContrib.Formatting.Jsonp;

namespace CountingKs
{
  public static class WebApiConfig
  {
    public static void Register(HttpConfiguration config)
    {
      config.MapHttpAttributeRoutes();

      // The food controller is using attribute routing
      //config.Routes.MapHttpRoute(
      //    name: "Food",
      //    routeTemplate: "api/nutrition/food/{id}",
      //    defaults: new
      //    {
      //      controller = "Food",
      //      id = RouteParameter.Optional
      //    }
      //);

      config.Routes.MapHttpRoute(
          name: "Measure",
          routeTemplate: "api/nutrition/food/{foodid}/measure/{id}",
          defaults: new
          {
            controller = "Measure",
            id = RouteParameter.Optional
          }
      );

      //config.Routes.MapHttpRoute(
      //    name: "MeasureV2",
      //    routeTemplate: "api/nutrition/food/{foodid}/measure/{id}",
      //    defaults: new
      //    {
      //      controller = "MeasureV2",
      //      id = RouteParameter.Optional
      //    }
      //);

      config.Routes.MapHttpRoute(
          name: "Diary",
          routeTemplate: "api/user/diary/{date}",
          defaults: new
          {
            controller = "Diary",
            date = RouteParameter.Optional
          }
      );

      config.Routes.MapHttpRoute(
          name: "DiaryEntry",
          routeTemplate: "api/user/diary/{date}/entry/{id}",
          defaults: new
          {
            controller = "DiaryEntry",
            id = RouteParameter.Optional
          }
      );

      config.Routes.MapHttpRoute(
          name: "DiarySummary",
          routeTemplate: "api/user/diary/{date}/summary",
          defaults: new
          {
            controller = "DiarySummary"
          }
      );

      config.Routes.MapHttpRoute(
          name: "Token",
          routeTemplate: "api/token",
          defaults: new
          {
            controller = "Token"
          }
      );

      //config.Routes.MapHttpRoute(
      //    name: "DefaultApi",
      //    routeTemplate: "api/{controller}/{id}",
      //    defaults: new { id = RouteParameter.Optional }
      //);

      JsonMediaTypeFormatter jsonFormatter = config
          .Formatters
          .OfType<JsonMediaTypeFormatter>()
          .FirstOrDefault();

      // Makes it so that the property names of the returned json data is Camel case and not Pascal case
      jsonFormatter
          .SerializerSettings
          .ContractResolver = new CamelCasePropertyNamesContractResolver();

      jsonFormatter.SerializerSettings.Converters.Add(new LinkModelConverter());

      CreateMediaTypes(jsonFormatter);

      // Support for JSONP
      JsonpMediaTypeFormatter formatter = new JsonpMediaTypeFormatter(jsonFormatter, "cb");
      config.Formatters.Insert(0, formatter);

      // Makes it so that only Https calls are accepted
      //config.Filters.Add(new RequireHttpsAttribute());

      // Replace the controller configuration
      config.Services.Replace(
        typeof(IHttpControllerSelector),
        new CountingKsControllerSelector(config));

      // Configure Cashing/Etag Support
      string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
      SqlServerEntityTagStore etagStore = new SqlServerEntityTagStore(connectionString);
      CachingHandler cacheHandler = new CachingHandler(etagStore)
      {
        AddLastModifiedHeader = false
      };
      config.MessageHandlers.Add(cacheHandler);

      // Add support CORS
      EnableCorsAttribute attr = new EnableCorsAttribute("*", "*", "GET");
      config.EnableCors(attr);

      // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
      // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
      // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
      //config.EnableQuerySupport();
    }

    private static void CreateMediaTypes(JsonMediaTypeFormatter jsonFormatter)
    {
      //CustomMediaType customMediaType = new CustomMediaType();

      foreach (string mediaType in CustomMediaType.GetAllCustomMediaTypes())
      {
        jsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
      }
    }
  }
}