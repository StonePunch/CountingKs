using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CountingKs.Data;
using CountingKs.Data.Entities;
using CountingKs.Models;

namespace CountingKs.Controllers
{
  public class MeasuresV2Controller : BaseApiController
  {
    public MeasuresV2Controller(ICountingKsRepository repo) : base(repo)
    {
    }

    public IEnumerable<MeasureV2Model> Get(int foodid)
    {
      var results = TheRepository.GetMeasuresForFood(foodid)
                         .ToList()
                         .Select(m => TheModelFactory.Create2(m));

      return results;
    }

    public MeasureV2Model Get(int foodid, int id)
    {
      var results = TheRepository.GetMeasure(id);

      if (results.Food.Id == foodid)
      {
        return TheModelFactory.Create2(results);
      }

      return null;
    }
  }
}
