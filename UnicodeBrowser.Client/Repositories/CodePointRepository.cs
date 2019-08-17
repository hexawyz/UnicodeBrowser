using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnicodeBrowser.Models;

namespace UnicodeBrowser.Client.Repositories
{
	internal sealed class CodePointRepository : RepositoryBase
	{
		private readonly BlockRepository _blockRepository;
		private readonly BlockCodePointRepository _blockCodePointRepository;

		public CodePointRepository(ApplicationState applicationState, HttpClient httpClient, BlockRepository blockRepository, BlockCodePointRepository blockCodePointRepository)
			: base(applicationState, httpClient)
		{
			_blockRepository = blockRepository;
			_blockCodePointRepository = blockCodePointRepository;
		}

		public Task<CodePoint> GetCodePointAsync(int codePoint)
		{
			// If the code point information has already been cached somewhere, try to access it from there.
			// This will essentially speed up things when navigating from block to code point.
			if (_blockRepository.TryGetBlocks()?.FirstOrDefault(b => b.Range.LastCodePoint >= codePoint && b.Range.FirstCodePoint <= codePoint) is BlockInformation block
				&& _blockCodePointRepository.TryGetBlockCodePointView(block) is PagedCodePointCollectionView codePointView
				&& (codePoint - block.Range.FirstCodePoint) is int index && index < codePointView.LoadedItemCount )
			{
				return Task.FromResult(codePointView.Items[index]);
			}

			return GetCodePointInternalAsync(codePoint);
		}

		public async Task<CodePoint> GetCodePointInternalAsync(int codePoint)
		{
			BeginAsyncOperation();
			try
			{
				return await HttpClient.GetItemAsync<CodePoint>("/api/codepoints/" + codePoint.ToHexadecimal(), CancellationToken.None).ConfigureAwait(false);
			}
			finally
			{
				EndAsyncOperation();
			}
		}
	}
}
