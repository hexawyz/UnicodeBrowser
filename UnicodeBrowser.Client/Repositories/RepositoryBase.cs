namespace UnicodeBrowser.Client.Repositories
{
	internal abstract class RepositoryBase
    {
		public ApplicationState ApplicationState { get; }

		protected RepositoryBase(ApplicationState applicationState)
		{
			ApplicationState = applicationState;
		}

		protected void BeginAsyncOperation() => ApplicationState.NotifyOperationStart();

		protected void EndAsyncOperation() => ApplicationState.NotifyOperationEnd();
	}
}
