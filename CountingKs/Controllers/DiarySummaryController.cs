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
    public class DiarySummaryController : BaseApiController
    {
        private readonly ICountingKsIdentityService _identityService;

        public DiarySummaryController(ICountingKsRepository repo, ICountingKsIdentityService identityService) : base(repo)
        {
            _identityService = identityService;
        }

        // api/user/diary/{date}/summary
        [HttpGet]
        public HttpResponseMessage Get(DateTime date)
        {
            try
            {
                string username = _identityService.CurrentUser;
                Diary diary = TheRepository.GetDiary(username, date);

                if (diary == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No diary found");

                DiarySummaryModel diarySummaryModel = TheModelFactory.CreateSummary(diary);

                return Request.CreateResponse(HttpStatusCode.OK, diarySummaryModel);
            }
            catch (Exception exception)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, exception);
            }
        }
    }
}
