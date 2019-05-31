using CountingKs.Data;
using CountingKs.Data.Entities;
using CountingKs.Models;
using CountingKs.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CountingKs.Controllers
{
  public class DiaryEntryController : BaseApiController
  {
    private readonly ICountingKsIdentityService _identityService;

    public DiaryEntryController(ICountingKsRepository repo, ICountingKsIdentityService identityService) : base(repo)
    {
      _identityService = identityService;
    }

    // api/user/diary/{date}/entry
    [HttpGet]
    public HttpResponseMessage Get(DateTime date)
    {
      string username = _identityService.CurrentUser;
      IEnumerable<DiaryEntryModel> entriesModel = TheRepository.GetDiaryEntries(username, date)
          .Take(10)
          .ToList()
          .Select(diaryEntry => TheModelFactory.Create(diaryEntry));

      return Request.CreateResponse(HttpStatusCode.OK, entriesModel);
    }

    // api/user/diary/{date}/entry/{id}
    [HttpGet]
    public HttpResponseMessage Get(DateTime date, int id)
    {
      string username = _identityService.CurrentUser;
      DiaryEntry diaryEntry = TheRepository.GetDiaryEntry(username, date, id);

      if (diaryEntry == null)
      {
        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No diary entry found");
      }

      DiaryEntryModel entryModel = TheModelFactory.Create(diaryEntry);

      return Request.CreateResponse(HttpStatusCode.OK, entryModel);
    }

    // api/user/diary/{date}/entry
    [HttpPost]
    public HttpResponseMessage Post(DateTime date, [FromBody]DiaryEntryModel model)
    {
      try
      {
        string username = _identityService.CurrentUser;
        DiaryEntry diaryEntry = TheModelFactory.Parse(model);

        if (diaryEntry == null)
          return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not read diary entry in body");

        Diary diary = TheRepository.GetDiary(username, date);

        if (diary == null)
          return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Diary not found");

        if (diary.Entries.Any(entry => entry.Measure.Id == diaryEntry.Measure.Id))
          return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Duplicate measure not allowed");

        diary.Entries.Add(diaryEntry);
        if (TheRepository.SaveAll())
          return Request.CreateResponse(HttpStatusCode.Created, TheModelFactory.Create(diaryEntry));
        else
          return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not save to the database");
      }
      catch (Exception exception)
      {
        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, exception);
      }
    }

    // api/user/diary/{date}/entry/{id}
    [HttpDelete]
    public HttpResponseMessage Delete(DateTime date, int id)
    {
      try
      {
        string username = _identityService.CurrentUser;
        DiaryEntry diaryEntry = TheRepository.GetDiaryEntry(username, date, id);

        if (diaryEntry == null)
          return Request.CreateResponse(HttpStatusCode.NotFound);

        if (TheRepository.DeleteDiaryEntry(id) && TheRepository.SaveAll())
          return Request.CreateResponse(HttpStatusCode.OK);
        else
          return Request.CreateResponse(HttpStatusCode.BadRequest);
      }
      catch (Exception exception)
      {
        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, exception);
      }
    }

    // api/user/diary/{date}/entry/{id}
    [HttpPatch]
    public HttpResponseMessage Patch(DateTime date, int id, [FromBody]DiaryEntryModel model)
    {
      try
      {
        string username = _identityService.CurrentUser;
        DiaryEntry diaryEntry = TheRepository.GetDiaryEntry(username, date, id);

        if (diaryEntry == null)
          return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Can't update a diary entry that does not exist");

        DiaryEntry newDiaryEntry = TheModelFactory.Parse(model);

        if (newDiaryEntry == null)
          return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not read diary entry in body");

        // Nothing to change
        if (diaryEntry.Quantity == newDiaryEntry.Quantity)
          return Request.CreateResponse(HttpStatusCode.OK);

        diaryEntry.Quantity = newDiaryEntry.Quantity;
        if (TheRepository.SaveAll())
          return Request.CreateResponse(HttpStatusCode.OK);
        else
          return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not save to the database");
      }
      catch (Exception exception)
      {
        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, exception);
      }
    }

  }
}
