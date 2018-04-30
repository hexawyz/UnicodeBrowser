using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Unicode;
using UnicodeBrowser.Data;

namespace UnicodeBrowser.Controllers
{
	[Route("api/text")]
    public class TextController
    {
        [HttpPost("decompose")]
        [ResponseCache(CacheProfileName = "TextDecompositionCacheProfile")]
        public IActionResult Decompose([FromBody] string text)
        {
            if (text == null || text.Length > 1024)
            {
                return new BadRequestResult();
            }

            var characters = new List<Models.CodePoint>();

            foreach (int codePoint in new PermissiveCodePointEnumerable(text))
            {
                characters.Add(CodePointProvider.GetCodePoint(codePoint));
            }

            return new OkObjectResult(characters);
        }
    }
}
