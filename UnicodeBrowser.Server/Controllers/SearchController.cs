using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UnicodeBrowser.Data;
using UnicodeBrowser.Services;

namespace UnicodeBrowser.Controllers
{
	[Route("api/search")]
    public class SearchController
    {
        public ICharacterSearchService CharacterSearchService { get; }

        public SearchController(ICharacterSearchService characterSearchService)
        {
            CharacterSearchService = characterSearchService;
        }

        [HttpGet]
        [ResponseCache(CacheProfileName = "TextDecompositionCacheProfile")]
        public async Task<IActionResult> Search([FromQuery(Name = "q"), Required] string text, int limit = 10)
        {
			if (string.IsNullOrEmpty(text)) return new BadRequestResult();

            var characters = await CharacterSearchService.FindCharactersAsync(text);

            return new OkObjectResult
            (
                from codePoint in characters.Take(Math.Max(1, Math.Min(100, limit)))
                select CodePointProvider.GetCodePoint(codePoint)
            );
        }
    }
}
