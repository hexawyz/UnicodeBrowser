using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace UnicodeBrowser.Mvc
{
	internal sealed class RangeOptionsResult : OkResult
    {
		public override Task ExecuteResultAsync(ActionContext context)
		{
			context.HttpContext.Response.Headers.Add("Accept-Ranges", "items");

			return base.ExecuteResultAsync(context);
		}
	}
}
