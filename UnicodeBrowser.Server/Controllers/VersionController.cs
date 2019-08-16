using System.Net.Mime;
using System.Unicode;
using Microsoft.AspNetCore.Mvc;

namespace UnicodeBrowser.Controllers
{
	[Route("api/version")]
	internal sealed class VersionController
	{
		[HttpGet]
		[ResponseCache(CacheProfileName = "BlockCacheProfile")]
		public IActionResult Get()
			=> new ContentResult
			{
				StatusCode = 200,
				Content = UnicodeInfo.UnicodeVersion.ToString(2),
				ContentType = MediaTypeNames.Text.Plain,
			};
	}
}
