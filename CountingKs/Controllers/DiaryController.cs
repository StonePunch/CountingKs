using CountingKs.Data;
using CountingKs.Data.Entities;
using CountingKs.Filters;
using CountingKs.Models;
using CountingKs.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;

namespace CountingKs.Controllers
{
  [CountingKsAuthorize]
  public class DiaryController : BaseApiController
  {
    private readonly ICountingKsIdentityService _identityService;

    public DiaryController(ICountingKsRepository repo, ICountingKsIdentityService identityService) : base(repo)
    {
      _identityService = identityService;
    }

    // api/user/diary
    [HttpGet]
    public HttpResponseMessage Get()
    {
      string username = _identityService.CurrentUser;
      IEnumerable<DiaryModel> diaries = TheRepository.GetDiaries(username)
          .OrderByDescending(diary => diary.CurrentDate)
          .Take(10)
          .ToList()
          .Select(diary => TheModelFactory.Create(diary));

      return Request.CreateResponse(HttpStatusCode.OK, diaries);
    }

    // api/user/diary/{id}
    [HttpGet]
    public HttpResponseMessage Get(DateTime date)
    {
      string username = _identityService.CurrentUser;
      Diary diary = TheRepository.GetDiary(username, date);

      if (diary == null)
        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No diary found");

      DiaryModel diaryModel = TheModelFactory.Create(diary);

      return Request.CreateResponse(HttpStatusCode.OK, diaryModel);
    }

    // api/user/diary
    [HttpPost]
    public HttpResponseMessage Post([FromBody] DiaryModel model)
    {
      try
      {
        string username = _identityService.CurrentUser;

        if (TheRepository.GetDiaries(username).Count(d => d.CurrentDate == model.CurrentDate.Date) > 0)
          return Request.CreateErrorResponse(HttpStatusCode.Conflict, "A diary already exists for that date");

        Diary diary = TheModelFactory.Parse(model);
        diary.UserName = username;

        if (TheRepository.Insert(diary) && TheRepository.SaveAll())
          return Request.CreateResponse(HttpStatusCode.Created, TheModelFactory.Create(diary));
      }
      catch
      {
        // TODO Add Logging
      }

      return Request.CreateResponse(HttpStatusCode.BadRequest);
    }

    // api/user/diary/{id}
    [HttpDelete]
    public HttpResponseMessage Delete(DateTime id)
    {
      try
      {
        string username = _identityService.CurrentUser;
        Diary entity = TheRepository.GetDiary(username, id);
        if (entity == null)
        {
          return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        if (TheRepository.DeleteDiary(entity.Id) && TheRepository.SaveAll())
        {
          return Request.CreateResponse(HttpStatusCode.OK);
        }
      }
      catch
      {
        // TODO Add Logging
      }

      return Request.CreateResponse(HttpStatusCode.BadRequest);
    }
  }
}
