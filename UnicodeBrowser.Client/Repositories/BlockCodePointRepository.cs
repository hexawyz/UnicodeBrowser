using System.Collections.Concurrent;
using System.Net.Http;
using UnicodeBrowser.Client.Models;

namespace UnicodeBrowser.Client.Repositories
{
	internal sealed class BlockCodePointRepository : RepositoryBase
	{
		// TODO: implement cache eviction. (LRU should do the trick)

		private readonly ConcurrentDictionary<(string BlockName, CodePointRange Range), PagedCodePointCollectionView> _cachedViews = new ConcurrentDictionary<(string, CodePointRange), PagedCodePointCollectionView>();
		private readonly HttpClient _httpClient;

		public BlockCodePointRepository(ApplicationState applicationState, HttpClient httpClient)
			: base(applicationState)
		{
			_httpClient = httpClient;
		}

		public PagedCodePointCollectionView GetBlockCodePointView(BlockInformation block)
			=> _cachedViews.GetOrAdd((block.Name, block.Range), k => new PagedCodePointCollectionView(ApplicationState, _httpClient, k.Range.FirstCodePoint, k.Range.LastCodePoint - k.Range.FirstCodePoint + 1));
	}
}
