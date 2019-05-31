using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;

namespace CountingKs.Services
{
  public class CountingKsControllerSelector : DefaultHttpControllerSelector
  {
    private readonly HttpConfiguration _config;

    // #Placeholder information#
    private static readonly List<ApiVersion> _apiVersions = new List<ApiVersion>() {
      new ApiVersion()
      {
        Version = "1",
        ReleaseDate = new DateTime(2019, 4, 10, 0, 0, 0),
      },
      new ApiVersion()
      {
        Version = "2",
        ReleaseDate = new DateTime(2019, 4, 11, 0, 0, 0),
      },
    };

    public CountingKsControllerSelector(HttpConfiguration config) : base(config)
    {
      _config = config;
    }

    #region Versioning Methods

    public static ApiVersion GetVersionFromAcceptHeaderVersion(HttpRequestMessage request)
    {
      List<string> ACCEPTED_TYPES = new List<string>() { "application/json" };

      HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> acceptHeader = request.Headers.Accept;

      // Check if the Accept header is present
      if (acceptHeader == null)
        return null;

      string acceptHeaderVersion = (from mime in acceptHeader
                                    where ACCEPTED_TYPES.IndexOf(mime.MediaType) != -1
                                    select (from version in mime.Parameters
                                            where version.Name.Equals("version", StringComparison.OrdinalIgnoreCase)
                                            select version.Value).FirstOrDefault()).FirstOrDefault();

      // Checks if the passed version is valid
      ApiVersion apiVersion = _apiVersions
        .Where(version => version.Version == acceptHeaderVersion)
        .FirstOrDefault();

      if (apiVersion == null)
        return null;

      return apiVersion;
    }

    public static ApiVersion GetVersionFromQueryString(HttpRequestMessage request)
    {
      const string QUERY_KEY = "v";

      string queryString = request.RequestUri.Query;
      NameValueCollection queryCollection = HttpUtility.ParseQueryString(queryString);

      // Check to see if a version was specified in the query string
      if (string.IsNullOrWhiteSpace(queryCollection[QUERY_KEY]))
        return null;

      // Checks if the passed version is valid
      ApiVersion apiVersion = _apiVersions
        .Where(version => version.Version == queryCollection[QUERY_KEY])
        .FirstOrDefault();

      if (apiVersion == null)
        return null;

      return apiVersion;
    }

    public static ApiVersion GetVersionFromHeader(HttpRequestMessage request)
    {
      const string HEADER_NAME = "X-CountingKs-Version";

      // Check to see if a version was specified in the request header
      if (!request.Headers.Contains(HEADER_NAME))
        return null;

      string headerVersion = request.Headers.GetValues(HEADER_NAME).FirstOrDefault();

      // Checks if the passed version is valid
      ApiVersion apiVersion = _apiVersions
        .Where(version => version.Version == headerVersion)
        .FirstOrDefault();

      if (apiVersion == null)
        return null;

      return apiVersion;
    }

    public static ApiVersion GetVersionFromMediaType(HttpRequestMessage request, string controllerName)
    {
      HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> acceptHeader = request.Headers.Accept;

      // Check if the Accept header is present
      if (acceptHeader == null)
        return null;

      Regex customMediaTypeRegex = new Regex(@"application\/vnd\.countingks\.([a-z]+)\.v([0-9]+)\+json");

      string mediaTypeVersion = (from mime in acceptHeader
                                 where CustomMediaType.GetAllCustomMediaTypes().IndexOf(mime.MediaType) != -1 && 
                                 customMediaTypeRegex.Match(mime.MediaType).Groups[1].Value.Equals(controllerName, StringComparison.OrdinalIgnoreCase)
                                 select customMediaTypeRegex.Match(mime.MediaType).Groups[2].Value).FirstOrDefault();

      // Checks if the passed version is valid
      ApiVersion apiVersion = _apiVersions
        .Where(version => version.Version == mediaTypeVersion)
        .FirstOrDefault();

      if (apiVersion == null)
        return null;

      return apiVersion;
    }

    public static ApiVersion GetMostRecentVersion()
    {
      return _apiVersions
          .OrderByDescending(version => version.ReleaseDate)
          .First();
    }

    #endregion

    public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
    {
      /* Api Versioning
       *
       * # -- [1]Using Accept Header -- #
       * 
       * In order to request a specific version of the Api using the
       * Accept Header method, a version must be specified 
       * in the accept header of the request, in the style of 
       * "Accept: application/json; version={version number}"
       * 
       * # -- [2]Using Request Header -- #
       * 
       * In order to request a specific version of the Api using the 
       * Header method, a header in the style of "X-CountingKs-Version: {version number}"
       * needs to be added to the request
       * 
       * # -- [3]Using QueryString -- #
       * 
       * In order to request a specific version of the Api using the
       * QueryString method, a value in the style of "v={version number}" 
       * needs to be added to the query string
       * 
       * # -- [4]Using Custom Media Type -- #
       * 
       * In order to request a specificversion of the Api using the
       * Custom Media Type method, a custom media type (CustomMediaType class)
       * must be added to the accept header of the request, in the style of
       * "Accept: application/vnd.countingks.{controller name}.v{version number}+json"
       * 
       */

      // Gets a dictionary of all the exposed controllers
      IDictionary<string, HttpControllerDescriptor> controllers = GetControllerMapping();

      IHttpRouteData routeData = request.GetRouteData();

      // The name of the controller that the request is targetting
      string controllerName = (string)routeData.Values["controller"];
      
      // When attribute routing is being used
      if (string.IsNullOrWhiteSpace(controllerName))
        return base.SelectController(request);

      if (!controllers.TryGetValue(controllerName, out HttpControllerDescriptor descriptor))
        return null;

      ApiVersion apiVersion;

      apiVersion = GetVersionFromAcceptHeaderVersion(request);

      if (apiVersion == null)
        apiVersion = GetVersionFromHeader(request);

      if (apiVersion == null)
        apiVersion = GetVersionFromQueryString(request);

      if (apiVersion == null)
        apiVersion = GetVersionFromMediaType(request, controllerName);

      if (apiVersion == null)
        apiVersion = GetMostRecentVersion();

      string versionName = string.Concat(controllerName, "V", apiVersion.Version); // example: MeasureV2

      // Check if the controller for the selected version exists
      if (controllers.TryGetValue(versionName, out HttpControllerDescriptor versionDescriptor))
        return versionDescriptor;

      return descriptor;
    }
  }
}