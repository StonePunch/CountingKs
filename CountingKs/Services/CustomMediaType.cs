using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CountingKs.Services
{
  public class CustomMediaType
  {
    // Probably an implementation of questionable quality
    // TODO: Validate this with someone

    public static string Food = "application/vnd.countingks.food.v1+json";
    public static string MeasureV1 = "application/vnd.countingks.measure.v1+json";
    public static string MeasureV2 ="application/vnd.countingks.measure.v2+json";
    public static string Diary = "application/vnd.countingks.diary.v1+json";
    public static string DiaryEntry = "application/vnd.countingks.diaryEntry.v1+json";

    public static List<string> GetAllCustomMediaTypes()
    {
      return typeof(CustomMediaType)
        .GetFields()
        .Select(field => field.GetValue(null).ToString())
        .ToList();
    }
  }
}