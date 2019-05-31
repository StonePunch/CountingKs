using CountingKs.ActionResults;
using CountingKs.Data;
using CountingKs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CountingKs.Controllers
{
  public abstract class BaseApiController : ApiController
  {
    private ModelFactory _modelFactory;
    protected ICountingKsRepository TheRepository { get; }

    protected ModelFactory TheModelFactory
    {
      get
      {
        if (_modelFactory == null)
          _modelFactory = new ModelFactory(this.Request, TheRepository);

        return _modelFactory;
      }
    }

    protected IHttpActionResult Versioned<T>(T body, string version = "v1") where T : class
    {
      return new VersionedActionResult<T>(Request, version, body);
    }

    /// <summary>
    /// Class used to initialize global variables
    /// The class is instantiated via dependency injection in "/App_Start/NinjectWebCommon.cs"
    /// </summary>
    /// <param name="repo">Passed via dependency injection</param>
    public BaseApiController(ICountingKsRepository repo)
    {
      TheRepository = repo;
    }

  }
}
