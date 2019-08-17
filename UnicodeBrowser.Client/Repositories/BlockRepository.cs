using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnicodeBrowser.Models;

namespace UnicodeBrowser.Client.Repositories
{
	internal sealed class BlockRepository : RepositoryBase
    {
		private readonly object _syncRoot = new object();

		private volatile Task<BlockInformation[]> _blockRetrievalTask;

		public BlockRepository(ApplicationState applicationState, HttpClient httpClient)
			: base(applicationState, httpClient)
		{
		}

		public Task<BlockInformation[]> GetBlocksAsync()
		{
			var blockRetrievalTask = _blockRetrievalTask;

			if (blockRetrievalTask == null || blockRetrievalTask.IsFaulted)
			{
				lock (_syncRoot)
				{
					if ((blockRetrievalTask = _blockRetrievalTask) == null || blockRetrievalTask.IsFaulted)
					{
						blockRetrievalTask = _blockRetrievalTask = GetBlocksInternalAsync();
					}
				}
			}

			return blockRetrievalTask;
		}

		public BlockInformation[] TryGetBlocks()
		{
			var blockRetrievalTask = _blockRetrievalTask;

			return blockRetrievalTask != null && blockRetrievalTask.Status == TaskStatus.RanToCompletion ?
				blockRetrievalTask.Result :
				null;
		}

		private async Task<BlockInformation[]> GetBlocksInternalAsync()
		{
			BeginAsyncOperation();
			try
			{
				return await HttpClient.GetItemsAsync<BlockInformation>("/api/blocks", CancellationToken.None);
			}
			finally
			{
				EndAsyncOperation();
			}
		}

		public Task<BlockInformation> GetBlockAsync(string blockName)
		{
			var task = _blockRetrievalTask;

			return task.Status == TaskStatus.RanToCompletion ?
				Task.FromResult(GetBlockSync(task.Result, blockName)) :
				GetBlockInternalAsync(blockName);
		}

		private async Task<BlockInformation> GetBlockInternalAsync(string blockName)
			=> GetBlockSync(await GetBlocksAsync().ConfigureAwait(false), blockName);

		private static BlockInformation GetBlockSync(BlockInformation[] blocks, string blockName)
			=> blocks.FirstOrDefault(b => string.Equals(b.Name, blockName, StringComparison.OrdinalIgnoreCase));
	}
}
