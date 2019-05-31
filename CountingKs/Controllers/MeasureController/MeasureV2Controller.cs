using CountingKs.Data;
using CountingKs.Data.Entities;
using CountingKs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CountingKs.Controllers
{
  public class MeasureV2Controller : BaseApiController
  {
    public MeasureV2Controller(ICountingKsRepository repo) : base(repo) { }

    // api/nutrition/food/{foodid}/measure
    [HttpGet]
    public IEnumerable<MeasureV2Model> Get(int foodid)
    {
      IEnumerable<MeasureV2Model> results = TheRepository.GetMeasuresForFood(foodid)
          .ToList()
          .Select(m => TheModelFactory.Create2(m));

      return results;
    }

    // api/nutrition/food/{foodid}/measure/{id}
    [HttpGet]
    public MeasureV2Model Get(int foodid, int id)
    {
      Measure measure = TheRepository.GetMeasure(id);

      if (measure.Food.Id == foodid)
        return TheModelFactory.Create2(measure);

      return null;
    }
  }
}
