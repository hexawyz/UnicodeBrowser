using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;

namespace UnicodeBrowser.Mvc
{
	internal sealed class RangeResult : ObjectResult
    {
		private readonly ContentRangeHeaderValue _range;

        public RangeResult(object value, ContentRangeHeaderValue range)
            : base(value)
        {
			_range = range;

			// Return HTTP 200 if all the items are present in the response; otherwise, return HTTP 206.
            StatusCode = range.From == 0 && range.To != null && range.Length != null && range.To.GetValueOrDefault() + 1 == range.Length.GetValueOrDefault() ?
				StatusCodes.Status200OK :
				StatusCodes.Status206PartialContent;
		}

        public override Task ExecuteResultAsync(ActionContext context)
        {
			context.HttpContext.Response.Headers.Add("Accept-Ranges", "items");
			context.HttpContext.Response.Headers.Add("Content-Range", _range.ToString());

			return base.ExecuteResultAsync(context);
        }
    }
}
