using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CountingKs.ActionResults;
using CountingKs.Data;
using CountingKs.Models;

namespace CountingKs.Controllers
{
  public abstract class BaseApiController : ApiController
  {
    ICountingKsRepository _repo;
    ModelFactory _modelFactory;

    public BaseApiController(ICountingKsRepository repo)
    {
      _repo = repo;
    }

    protected ICountingKsRepository TheRepository
    {
      get
      {
        return _repo;
      }
    }

    protected ModelFactory TheModelFactory
    {
      get
      {
        if (_modelFactory == null)
        {
          _modelFactory = new ModelFactory(this.Request, TheRepository);
        }
        return _modelFactory;
      }
    }

    protected IHttpActionResult Versioned<T>(T body, string version = "v1") where T : class
    {
      return new VersionedActionResult<T>(Request, version, body);
    }

  }
}
