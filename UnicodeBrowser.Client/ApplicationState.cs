using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Services;
using System.Net.Http;
using System.Threading.Tasks;
using UnicodeBrowser.Client.Models;

namespace UnicodeBrowser.Client
{
	public sealed class ApplicationState : BindableObject
    {
		private readonly object _syncRoot = new object();
		private readonly HttpClient _httpClient;
		private readonly IUriHelper _uriHelper;

		public ApplicationState(IUriHelper uriHelper, HttpClient httpClient)
		{
			_uriHelper = uriHelper;
			_httpClient = httpClient;

			_uriHelper.OnLocationChanged += OnLocationChanged;
		}

		private void OnLocationChanged(object sender, string e)
		{
			// TODO: Find a way to communicate the busy state reliably.
			//IsBusy = true;
		}

		private volatile Task<BlockInformation[]> _blockRetrievalTask;

		public Task<BlockInformation[]> GetBlocksAsync()
		{
			if (_blockRetrievalTask == null || _blockRetrievalTask.IsFaulted)
			{
				lock (_syncRoot)
				{
					if (_blockRetrievalTask == null || _blockRetrievalTask.IsFaulted)
					{
						_blockRetrievalTask = _httpClient.GetJsonAsync<BlockInformation[]>("/api/blocks");
					}
				}
			}

			return _blockRetrievalTask;
		}

		private bool _isBusy;

		public bool IsBusy
		{
			get => _isBusy;
			set => SetValue(ref _isBusy, value);
		}
	}
}
