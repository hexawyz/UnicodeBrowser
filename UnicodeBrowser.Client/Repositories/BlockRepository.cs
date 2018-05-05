using Microsoft.AspNetCore.Blazor;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnicodeBrowser.Client.Models;

namespace UnicodeBrowser.Client.Repositories
{
	internal sealed class BlockRepository : RepositoryBase
    {
		private readonly object _syncRoot = new object();
		private readonly HttpClient _httpClient;

		private volatile Task<BlockInformation[]> _blockRetrievalTask;

		public BlockRepository(ApplicationState applicationState, HttpClient httpClient)
			: base(applicationState)
		{
			_httpClient = httpClient;
		}

		public Task<BlockInformation[]> GetBlocksAsync()
		{
			if (_blockRetrievalTask == null || _blockRetrievalTask.IsFaulted)
			{
				lock (_syncRoot)
				{
					if (_blockRetrievalTask == null || _blockRetrievalTask.IsFaulted)
					{
						_blockRetrievalTask = GetBlocksInternalAsync();
					}
				}
			}

			return _blockRetrievalTask;
		}

		private async Task<BlockInformation[]> GetBlocksInternalAsync()
		{
			BeginAsyncOperation();
			try
			{
				return await _httpClient.GetJsonAsync<BlockInformation[]>("/api/blocks");
			}
			finally
			{
				EndAsyncOperation();
			}
		}

		public Task<BlockInformation> GetBlockAsync(string blockName)
		{
			var task = _blockRetrievalTask;

			return _blockRetrievalTask.Status == TaskStatus.RanToCompletion ?
				Task.FromResult(GetBlockSync(_blockRetrievalTask.Result, blockName)) :
				GetBlockInternalAsync(blockName);
		}

		private async Task<BlockInformation> GetBlockInternalAsync(string blockName)
			=> GetBlockSync(await GetBlocksAsync().ConfigureAwait(false), blockName);

		private static BlockInformation GetBlockSync(BlockInformation[] blocks, string blockName)
			=> blocks.FirstOrDefault(b => string.Equals(b.Name, blockName, StringComparison.OrdinalIgnoreCase));
	}
}
