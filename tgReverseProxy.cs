using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Primitives;

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace tgMessageReverseProxy
{
	public static class tgReverseProxy
  {
    private const string _tgHost = "https://api.telegram.org";

    [FunctionName("tgReverseProxy")]
    public static async Task Run(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete", Route = "tg/{botToken}/{*path}")]
      Microsoft.AspNetCore.Http.HttpRequest req, string botToken, string path)
    {
      var method =
        req.Method.ToLowerInvariant() switch
        {
          "get" => HttpMethod.Get,
          "post" => HttpMethod.Post,
          "put" => HttpMethod.Put,
          "delete" => HttpMethod.Delete
        };

      var uri = new Uri($"{_tgHost}/{botToken}/{path}{req.QueryString.ToUriComponent()}");

      var message = new HttpRequestMessage(method, uri) { Content = new ProxyContent(req) };

      var client = new HttpClient();
      
      var tgResponse = await client.SendAsync(message).ConfigureAwait(false);

      var resp = req.HttpContext.Response;

      resp.StatusCode = (int)tgResponse.StatusCode;

      foreach (var (k, v) in tgResponse.Headers)
        resp.Headers.Add(k, new StringValues(v.ToArray()));

      await tgResponse.Content.CopyToAsync(resp.Body).ConfigureAwait(false);
      await resp.Body.FlushAsync().ConfigureAwait(false);
    }
  }
}
