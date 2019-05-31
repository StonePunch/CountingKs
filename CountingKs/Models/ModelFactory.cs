using CountingKs.Data;
using CountingKs.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Routing;

namespace CountingKs.Models
{
  public class ModelFactory
  {
    private readonly UrlHelper _urlHelper;
    private readonly ICountingKsRepository _repo;

    public ModelFactory(HttpRequestMessage request, ICountingKsRepository repo)
    {
      _urlHelper = new UrlHelper(request);
      _repo = repo;
    }

    #region Create

    public FoodModel Create(Food food)
    {
      return new FoodModel
      {
        Url = string.Format(_urlHelper.Link("Food", new
        {
          id = food.Id
        })),
        Description = food.Description,
        Measures = food.Measures.Select(measure => Create(measure)),
      };
    }

    public MeasureModel Create(Measure measure)
    {
      return new MeasureModel
      {
        Url = string.Format(_urlHelper.Link("Measure", new
        {
          foodid = measure.Food.Id,
          id = measure.Id,
          v = 1,
        })),
        Description = measure.Description,
        Calories = Math.Round(measure.Calories),
      };
    }

    public DiaryModel Create(Diary diary)
    {
      return new DiaryModel
      {
        Links= new List<LinkModel>()
        {
          CreateLink(_urlHelper.Link("Diary", new
          {
              date = diary.CurrentDate.ToString("yyyy-MM-dd")
          }), "self"),
          CreateLink(_urlHelper.Link("DiaryEntry", new
          {
              date = diary.CurrentDate.ToString("yyyy-MM-dd")
          }), "newDiaryEntry", "POST"),
        },
        CurrentDate = diary.CurrentDate,
        Entries = diary.Entries.Select(entry => Create(entry)),
      };
    }

    public LinkModel CreateLink(string href, string rel, string  method = "GET", bool isTemplated = false)
    {
      return new LinkModel()
      {
        Href = href,
        Rel = rel,
        Method = method,
        IsTemplated = isTemplated,
      };
    }

    public DiaryEntryModel Create(DiaryEntry entry)
    {
      DateTime diaryDate = entry.Diary.CurrentDate;
      string year = diaryDate.Year.ToString();
      string month = diaryDate.Month.ToString().PadLeft(2, '0');
      string day = diaryDate.Day.ToString().PadLeft(2, '0');

      FoodModel foodModel = Create(entry.FoodItem);
      MeasureModel measureModel = Create(entry.Measure);

      return new DiaryEntryModel
      {
        Url = string.Format(_urlHelper.Link("DiaryEntry", new
        {
          date = string.Format("{0}-{1}-{2}", year, month, day),
          id = entry.Id
        })),
        FoodDescription = foodModel.Description,
        FoodUrl = foodModel.Url,
        MeasureDescription = entry.Measure.Description,
        MeasureUrl = measureModel.Url,
        Quantity = entry.Quantity,
      };
    }

    public AuthTokenModel Create(AuthToken authToken)
    {
      return new AuthTokenModel()
      {
        Token = authToken.Token,
        Expiration = authToken.Expiration,
      };
    }

    #endregion

    #region Create2

    public MeasureV2Model Create2(Measure measure)
    {
      return new MeasureV2Model
      {
        Url = string.Format(_urlHelper.Link("Measure", new
        {
          foodid = measure.Food.Id,
          id = measure.Id,
        })),
        Description = measure.Description,
        Calories = Math.Round(measure.Calories),
        TotalFat = measure.TotalFat,
        SaturatedFat = measure.SaturatedFat,
        Protein = measure.Protein,
        Carbohydrates = measure.Carbohydrates,
        Fiber = measure.Fiber,
        Sugar = measure.Sugar,
        Sodium = measure.Sodium,
        Iron = measure.Iron,
        Cholestrol = measure.Cholestrol,
      };
    }

    #endregion

    #region Parse

    /// <summary>
    /// Parses DiaryEntryModel into DiaryEntry
    /// </summary>
    /// <param name="entryModel">Requires the presence of the either the MeasureUrl, FoodUrl or Quantity values</param>
    /// <returns>Returns null if parse failed</returns>
    public DiaryEntry Parse(DiaryEntryModel entryModel)
    {
      try
      {
        DiaryEntry entry = new DiaryEntry();

        bool hasValue = false;

        if (!string.IsNullOrWhiteSpace(entryModel.MeasureUrl))
        {
          Uri uri = new Uri(entryModel.MeasureUrl);
          int measuredId = int.Parse(uri.Segments.Last());
          Measure measure = _repo.GetMeasure(measuredId);

          if (measure == null)
            return null;

          entry.Measure = measure;
          entry.FoodItem = measure.Food;

          hasValue = true;
        }

        if (!string.IsNullOrWhiteSpace(entryModel.FoodUrl) &&
            string.IsNullOrWhiteSpace(entryModel.MeasureUrl))
        {
          Uri uri = new Uri(entryModel.FoodUrl);
          int foodId = int.Parse(uri.Segments.Last());
          Food food = _repo.GetFood(foodId);

          if (food == null)
            return null;

          entry.FoodItem = food;

          hasValue = true;
        }

        if (entryModel.Quantity != default(double))
        {
          entry.Quantity = entryModel.Quantity;
          hasValue = true;
        }

        // When a DiaryEntryModel was passed without any meaningful values
        if (!hasValue)
          return null;

        return entry;
      }
      catch
      {
        return null;
      }
    }

    public Diary Parse(DiaryModel diaryModel)
    {
      try
      {
        Diary diary = new Diary();

        LinkModel selfLink = diaryModel.Links.Where(link => link.Rel == "self").FirstOrDefault();

        if (selfLink != null && !string.IsNullOrWhiteSpace(selfLink.Href))
        {
          Uri uri = new Uri(selfLink.Href);
          diary.Id = int.Parse(uri.Segments.Last());
        }

        diary.CurrentDate = diaryModel.CurrentDate;

        if (diaryModel.Entries != null)
        {
          // Transfer all the entries from the model to the diary
          diary.Entries.ToList().AddRange(
            diaryModel.Entries.Select(entryModel => Parse(entryModel))
          );
        }

        return diary;
      }
      catch
      {
        return null;
      }
    }

    #endregion

    #region Summary

    public DiarySummaryModel CreateSummary(Diary diary)
    {
      return new DiarySummaryModel()
      {
        Date = diary.CurrentDate,
        TotalCalories = Math.Round(diary.Entries.Sum(entry => entry.Measure.Calories * entry.Quantity)),
      };
    }

    #endregion

  }
}