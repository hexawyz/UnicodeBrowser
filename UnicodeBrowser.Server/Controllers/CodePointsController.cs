using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using UnicodeBrowser.Data;
using UnicodeBrowser.Mvc;

namespace UnicodeBrowser.Controllers
{
	[Route("api/codepoints")]
    public class CodePointsController
    {
		[HttpGet]
		[ResponseCache(CacheProfileName = "CodePointRangeCacheProfile")]
		public IActionResult GetRange([FromHeader(Name = "Range")]string rawRange)
		{
			RangeHeaderValue range;

			if (rawRange != null)
			{
				if (!RangeHeaderValue.TryParse(rawRange, out range))
				{
					return new BadRequestResult();
				}
			}
			else
			{
				range = new RangeHeaderValue(0, 128) { Unit = "items" };
			}

			// Reject multipart range requests for now…
			if (!range.Unit.Equals("items", StringComparison.OrdinalIgnoreCase) || range.Ranges.Count != 1)
			{
				return new BadRequestResult();
			}

			var firstRange = range.Ranges.First();

			int offset = checked((int)(firstRange.From ?? 0));
			int count = firstRange.To != null ?
				Math.Min(128, checked((int)firstRange.To.GetValueOrDefault()) - offset + 1) :
				128;

			return new RangeResult(CodePointProvider.GetCodePoints(offset, count), new ContentRangeHeaderValue(offset, offset + count - 1, 0x110000) { Unit = "items" });
		}

		[HttpOptions]
		[ResponseCache(CacheProfileName = "CodePointCacheProfile")]
		public IActionResult Options() => new RangeOptionsResult();

		[HttpGet("{hexCodePoint}")]
		[ResponseCache(CacheProfileName = "CodePointCacheProfile")]
		public IActionResult Get(string hexCodePoint)
			=> !TryParseCodePoint(hexCodePoint, out int codePoint) ?
				new NotFoundResult() as IActionResult :
				new OkObjectResult(CodePointProvider.GetCodePoint(codePoint));

		private static bool TryParseCodePoint(string text, out int value)
        {
            if (text == null || text.Length < 4 || text.Length > 6)
            {
                goto Failure;
            }

            int buffer = 0;

            foreach (var c in text)
            {
                if (c >= '0' && c <= 'f')
                {
                    buffer <<= 4;

                    if (c <= '9')
                    {
                        buffer |= c - '0';
                    }
                    else if (c >= 'A' && c <= 'F')
                    {
                        buffer |= c - 'A' + 10;
                    }
                    else if (c >= 'a')
                    {
                        buffer |= c - 'a' + 10;
                    }
                }
                else
                {
                    goto Failure;
                }
            }

            if (buffer <= 0x10FFFF)
            {
                value = buffer;
                return true;
            }

        Failure:;
            value = 0;
            return false;
        }
    }
}
