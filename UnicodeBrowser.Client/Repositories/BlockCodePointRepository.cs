using System.Collections.Concurrent;
using System.Net.Http;
using UnicodeBrowser.Models;

namespace UnicodeBrowser.Client.Repositories
{
	internal sealed class BlockCodePointRepository : RepositoryBase
	{
		// TODO: implement cache eviction. (LRU should do the trick)

		private readonly ConcurrentDictionary<(string BlockName, CodePointRange Range), PagedCodePointCollectionView> _cachedViews = new ConcurrentDictionary<(string, CodePointRange), PagedCodePointCollectionView>();

		public BlockCodePointRepository(ApplicationState applicationState, HttpClient httpClient)
			: base(applicationState, httpClient)
		{
		}

		public PagedCodePointCollectionView GetBlockCodePointView(BlockInformation block)
			=> _cachedViews.GetOrAdd((block.Name, block.Range), k => new PagedCodePointCollectionView(ApplicationState, HttpClient, k.Range.FirstCodePoint, k.Range.LastCodePoint - k.Range.FirstCodePoint + 1));

		public PagedCodePointCollectionView TryGetBlockCodePointView(BlockInformation block)
			=> _cachedViews.TryGetValue((block.Name, block.Range), out var value) ? value : value;
	}
}
