using CountingKs.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using RouteAttribute = System.Web.Http.RouteAttribute;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;

namespace CountingKs.Controllers
{
  [RoutePrefix("api/stats")]
  //[EnableCors("*", "*", "GET,POST")]
  public class StatsController : BaseApiController
  {
    public StatsController(ICountingKsRepository repo) : base(repo)
    {

    }

    [Route("")]
    public IHttpActionResult Get()
    {
      var results = new
      {
        NumFoods = TheRepository.GetAllFoods().Count(),
        NumUsers = TheRepository.GetApiUsers().Count(),
      };

      return Ok(results);
    }

    [Route("~/api/stat/{id:int}")]
    public IHttpActionResult Get(int id)
    {
      if (id == 1)
        return Ok(new { NumFoods = TheRepository.GetAllFoods().Count() });

      if (id == 2)
        return Ok(new { NumUsers = TheRepository.GetApiUsers().Count() });

      return NotFound();
    }

    [Route("~/api/stat/{name:alpha}")]
    public IHttpActionResult Get(string name)
    {
      if (name == "foods")
        return Ok(new { NumFoods = TheRepository.GetAllFoods().Count() });

      if (name == "users")
        return Ok(new { NumUsers = TheRepository.GetApiUsers().Count() });

      return NotFound();
    }
  }
}