using CountingKs.Data;
using CountingKs.Data.Entities;
using CountingKs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

namespace CountingKs.Controllers
{
  public class TokenController : BaseApiController
  {
    public TokenController(ICountingKsRepository repo) : base(repo) { }

    [HttpPost]
    public HttpResponseMessage Post([FromBody]TokenRequestModel model)
    {
      try
      {
        ApiUser apiUser = TheRepository.GetApiUsers()
                .Where(user => user.AppId == model.ApiKey)
                .FirstOrDefault();

        if (apiUser == null)
          return Request.CreateErrorResponse(HttpStatusCode.NotFound, "The API key was not found");

        // Simplistic implementation, should not be used in enterprise applications
        byte[] key = Convert.FromBase64String(apiUser.Secret);
        HMACSHA256 provider = new HMACSHA256(key);

        byte[] hash = provider.ComputeHash(Encoding.UTF8.GetBytes(apiUser.AppId));
        string signature = Convert.ToBase64String(hash);

        if (signature != model.Signature)
          return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "The signature was not valid");

        // Generating token
        string rawTokenInfo = string.Concat(apiUser.AppId + DateTime.UtcNow.ToString("d"));
        byte[] rawTokenByte = Encoding.UTF8.GetBytes(rawTokenInfo);

        byte[] token = provider.ComputeHash(rawTokenByte);
        AuthToken authToken = new AuthToken()
        {
          Token = Convert.ToBase64String(token),
          Expiration = DateTime.UtcNow.AddDays(7),
          ApiUser = apiUser,
        };

        if (!TheRepository.Insert(authToken) || !TheRepository.SaveAll())
          return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Something went wrong when committing the token to the database");

        return Request.CreateResponse(HttpStatusCode.Created, TheModelFactory.Create(authToken));
      }
      catch (Exception exception)
      {
        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, exception);
      }
    }
  }
}
