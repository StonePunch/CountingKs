using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CountingKs.Models
{
  public class MeasureModel
  {
    public ICollection<LinkModel> Links { get; set; }
    public string Description { get; set; }
    public double Calories { get; set; }
  }
}

