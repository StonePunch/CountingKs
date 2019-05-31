using CountingKs.Data;
using CountingKs.Data.Entities;
using CountingKs.Filters;
using CountingKs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;

namespace CountingKs.Controllers
{
  [CountingKsAuthorize(false)]
  [RoutePrefix("api/nutrition/food")]
  public class FoodController : BaseApiController
  {
    public FoodController(ICountingKsRepository repo) : base(repo) { }

    public const int PAGE_SIZE = 50;

    // api/nutrition/food
    [HttpGet]
    [Route("", Name = "Foods")]
    public HttpResponseMessage Get(bool includeMeasures = true, int page = 0)
    {
      IQueryable<Food> query;

      if (includeMeasures)
        query = TheRepository.GetAllFoodsWithMeasures()
            .OrderBy(food => food.Description);
      else
        query = TheRepository.GetAllFoods()
            .OrderBy(food => food.Description);

      int totalCount = query.Count();
      int totalPages = (int)Math.Ceiling((double)totalCount / PAGE_SIZE);

      UrlHelper urlHelper = new UrlHelper(Request);

      List<LinkModel> links = new List<LinkModel>();
      if (page > 0)
        links.Add(TheModelFactory.CreateLink(urlHelper.Link("Foods", new
        {
          page = page - 1,
        }), "previousPage"));

      if (page < totalPages - 1)
        links.Add(TheModelFactory.CreateLink(urlHelper.Link("Foods", new
        {
          page = page + 1,
        }), "nextPage"));

      IEnumerable<FoodModel> results = query
          .Skip(PAGE_SIZE * page)
          .Take(PAGE_SIZE)
          .ToList()
          .Select(f => TheModelFactory.Create(f));

      return Request.CreateResponse(HttpStatusCode.OK, new
      {
        TotalCount = totalCount,
        TotalPages = totalPages,
        Links = links,
        Results = results,
      });
    }

    // api/nutrition/food/{id}
    [HttpGet]
    [Route("{id}", Name = "Food")]
    public HttpResponseMessage Get(int id)
    {
      return Request.CreateResponse(
          HttpStatusCode.OK,
          TheModelFactory.Create(TheRepository.GetFood(id)));
    }
  }
}
