using System;
using System.Net.Http;

namespace UnicodeBrowser.Client.Repositories
{
	internal abstract class RepositoryBase
    {
		protected ApplicationState ApplicationState { get; }
		protected HttpClient HttpClient { get; }

		protected RepositoryBase(ApplicationState applicationState, HttpClient httpClient)
		{
			ApplicationState = applicationState ?? throw new ArgumentNullException(nameof(applicationState));
			HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}

		protected void BeginAsyncOperation() => ApplicationState.NotifyOperationStart();

		protected void EndAsyncOperation() => ApplicationState.NotifyOperationEnd();
	}
}
