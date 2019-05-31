using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CountingKs.ActionResults
{
  public class VersionedActionResult<T> : IHttpActionResult where T : class
  {
    private readonly HttpRequestMessage _request;
    private readonly string _version;
    private readonly T _body;

    public VersionedActionResult(HttpRequestMessage request, string version, T body)
    {
      _request = request;
      _version = version;
      _body = body;
    }

    public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
    {
      HttpResponseMessage msg = _request.CreateResponse(_body);
      msg.Headers.Add("XXX-OurVersion", _version);
      return Task.FromResult(msg);
    }
  }
}