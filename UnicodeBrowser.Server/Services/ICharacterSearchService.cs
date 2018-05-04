using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnicodeBrowser.Services
{
	internal interface ICharacterSearchService
    {
        Task<IEnumerable<int>> FindCharactersAsync(string text);
    }
}
