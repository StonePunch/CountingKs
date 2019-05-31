using Newtonsoft.Json;
using System.Collections.Generic;

namespace CountingKs.Models
{
    public class FoodModel
    {
        public string Url { get; set; }
        public string Description { get; set; }
        public IEnumerable<MeasureModel> Measures { get; set; }
    }
}