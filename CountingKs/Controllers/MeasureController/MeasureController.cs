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
    public class MeasureController : BaseApiController
    {
        public MeasureController(ICountingKsRepository repo) : base(repo) { }

        // api/nutrition/food/{foodid}/measure
        [HttpGet]
        public IEnumerable<MeasureModel> Get(int foodid)
        {
            IEnumerable<MeasureModel> results = TheRepository.GetMeasuresForFood(foodid)
                .ToList()
                .Select(m => TheModelFactory.Create(m));

            return results;
        }

        // api/nutrition/food/{foodid}/measure/{id}
        [HttpGet]
        public MeasureModel Get(int foodid, int id)
        {
            Measure measure = TheRepository.GetMeasure(id);

            if (measure.Food.Id == foodid)
                return TheModelFactory.Create(measure);

            return null;
        }
    }
}
