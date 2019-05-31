#if DEBUG
#define DISABLE_SECURITY
#endif
using CountingKs.Data;
using CountingKs.Data.Entities;
using Ninject;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Routing;
using WebMatrix.WebData;

namespace CountingKs.Filters
{
  /// <summary>
  /// Requires the [CountingKsAuthorize] tag to use
  /// </summary>
  public class CountingKsAuthorizeAttribute : AuthorizationFilterAttribute
  {
    private readonly bool _perUser;

    [Inject]
    public CountingKsRepository TheRepository { get; set; }

    public CountingKsAuthorizeAttribute(bool perUser = true)
    {
      _perUser = perUser;
    }

    public override void OnAuthorization(HttpActionContext actionContext)
    {
#if !DISABLE_SECURITY
   
      if (IsTokenAuthenticated(actionContext))
      {
        if (_perUser)
          if (!IsBasicAuthenticated(actionContext))
            HandleUnauthorized(actionContext);
      }
      else
        HandleUnauthorized(actionContext);

#endif
    }

    /// <summary>
    /// Validates the token passed in request
    /// </summary>
    /// <returns>True = Valid Token || False = Invalid Token</returns>
    private bool IsTokenAuthenticated(HttpActionContext actionContext)
    {
      /* Token Authentication
       * 
       * For the token authentication to be successful, the "actionContext"
       * that is passed must contain a "Request", which "QueryString" 
       * has an "apiKey" and a "token" value
       *
       */

      const string APIKEYNAME = "apiKey";
      const string TOKENNAME = "token";

      string queryString = actionContext.Request.RequestUri.Query;
      NameValueCollection queryCollection = HttpUtility.ParseQueryString(queryString);

      if (!string.IsNullOrWhiteSpace(queryCollection[APIKEYNAME]))
        if (!string.IsNullOrWhiteSpace(queryCollection[TOKENNAME]))
        {
          string apiKey = queryCollection[APIKEYNAME];
          string token = queryCollection[TOKENNAME];

          AuthToken authToken = TheRepository.GetAuthToken(token);

          if (authToken != null)
            if (authToken.ApiUser.AppId == apiKey)
              if (authToken.Expiration > DateTime.UtcNow)
                return true;
        }

      return false;
    }

    /// <summary>
    /// Validates the username/password combination in the request
    /// </summary>
    /// <returns>True = Valid Data || False = Invalid Data</returns>
    private bool IsBasicAuthenticated(HttpActionContext actionContext)
    {
      /* Basic Authentication
       * 
       * Require either that the user logs in or that the request to the api transmit
       * a header "Authorization: Basic credentials", where the credentials should be a
       * base64 encoded string of "username:password" using the ISO-8859-1 charset 
       *
       */

      // In case the user as already been authenticated by some other mean, namely the form login
      if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
        return true;

      AuthenticationHeaderValue authHeader = actionContext.Request.Headers.Authorization;

      if (authHeader != null)
        if (authHeader.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase))
          if (!string.IsNullOrWhiteSpace(authHeader.Parameter))
          {
            byte[] credentials = Convert.FromBase64String(authHeader.Parameter);
            string decodedCredentials = Encoding.GetEncoding("ISO-8859-1").GetString(credentials);
            string[] split = decodedCredentials.Split(':');

            string username = split[0];
            string password = split[1];

            if (!WebSecurity.Initialized)
              WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);

            if (WebSecurity.Login(username, password))
            {
              GenericPrincipal principal = new GenericPrincipal(new GenericIdentity(username), null);
              Thread.CurrentPrincipal = principal;
              return true;
            }
          }

      return false;
    }

    private void HandleUnauthorized(HttpActionContext actionContext)
    {
      actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
      if (_perUser)
      {
        string url = string.Format(new UrlHelper(actionContext.Request).Link("Default", new
        {
          controller = "account",
          action = "login",
        }));

        actionContext.Response.Headers.Add(
                "WWW-Authenticate",
                string.Format("Basic Scheme='CountingKs' location='{0}'", url));
      }
    }
  }
}