using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnicodeBrowser.Services
{
	public interface ICharacterSearchService
    {
        Task<IEnumerable<int>> FindCharactersAsync(string text);
    }
}
