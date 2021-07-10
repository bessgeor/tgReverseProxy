using Microsoft.AspNetCore.Http;

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace tgMessageReverseProxy
{
	internal class ProxyContent : HttpContent
	{
		private readonly Stream _inner;
		private readonly long? _size;

		private static void UpdateFromReq(HttpContentHeaders toUpdate, HttpRequest req)
		{
			foreach ((string h, Microsoft.Extensions.Primitives.StringValues v) in req.Headers)
				toUpdate.TryAddWithoutValidation(h, (IEnumerable<string>)v);
		}

		public ProxyContent(HttpRequest req)
		{
			_inner = req.Body;
			_size = req.ContentLength;
			UpdateFromReq(Headers, req);
		}

		protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
		{
			return _inner.CopyToAsync(stream);
		}

		protected override bool TryComputeLength(out long length)
		{
			length = _size ?? 0L;
			return _size.HasValue;
		}
	}
}