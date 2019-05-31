using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace CountingKs.Filters
{
  public class RequireHttpsAttribute : AuthorizationFilterAttribute
  {
    public override void OnAuthorization(HttpActionContext actionContext)
    {
      HttpRequestMessage request = actionContext.Request;

      if (request.RequestUri.Scheme != Uri.UriSchemeHttps)
      {
        string response = "<p>Https is required!</p>";
        if (request.Method.Method == "GET")
        {
          actionContext.Response =
            request.CreateResponse(HttpStatusCode.Found);
          actionContext.Response.Content =
            new StringContent(response, Encoding.UTF8, "text/html");

          UriBuilder uriBuilder = new UriBuilder(request.RequestUri)
          {
            Scheme = Uri.UriSchemeHttps,
            Port = 443,
          };

          actionContext.Response.Headers.Location = uriBuilder.Uri;
        }
        else
        {
          actionContext.Response = 
            request.CreateResponse(HttpStatusCode.NotFound);
          actionContext.Response.Content = 
            new StringContent(response, Encoding.UTF8, "text/html");
        }
      }
    }
  }
}