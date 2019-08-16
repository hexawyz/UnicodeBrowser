using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace UnicodeBrowser.Client.Repositories
{
	internal sealed class UnicodeVersionRepository : RepositoryBase
	{
		private readonly object _syncRoot = new object();

		private volatile Task<Version> _versionRetrievalTask;

		public UnicodeVersionRepository(ApplicationState applicationState, HttpClient httpClient)
			: base(applicationState, httpClient)
		{
		}

		public Task<Version> GetUnicodeVersionAsync()
		{
			var versionRetrievalTask = _versionRetrievalTask;

			if (versionRetrievalTask == null || versionRetrievalTask.IsFaulted)
			{
				lock (_syncRoot)
				{
					if ((versionRetrievalTask = _versionRetrievalTask) == null || versionRetrievalTask.IsFaulted)
					{
						versionRetrievalTask = _versionRetrievalTask = GetVersionInternalAsync();
					}
				}
			}

			return versionRetrievalTask;
		}

		public Version TryGetVersion()
		{
			var versionRetrievalTask = _versionRetrievalTask;

			return versionRetrievalTask != null && versionRetrievalTask.Status == TaskStatus.RanToCompletion ?
				versionRetrievalTask.Result :
				null;
		}

		private async Task<Version> GetVersionInternalAsync()
		{
			BeginAsyncOperation();
			try
			{
				return Version.Parse(await HttpClient.GetStringAsync("/api/version"));
			}
			finally
			{
				EndAsyncOperation();
			}
		}
	}
}
