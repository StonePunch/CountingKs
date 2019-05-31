using Newtonsoft.Json;

namespace CountingKs.Models
{
  public class MeasureV2Model : MeasureModel
  {
    public double TotalFat { get; set; }
    public double SaturatedFat { get; set; }
    public double Protein { get; set; }
    public double Carbohydrates { get; set; }
    public double Fiber { get; set; }
    public double Sugar { get; set; }
    public double Sodium { get; set; }
    public double Iron { get; set; }
    public double Cholestrol { get; set; }
  }
}