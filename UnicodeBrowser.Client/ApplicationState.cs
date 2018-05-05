using System.Threading;

namespace UnicodeBrowser.Client
{
	public sealed class ApplicationState : BindableObject
    {
		private int _busyCounter;

		public bool IsBusy => Volatile.Read(ref _busyCounter) != 0;

		internal void NotifyOperationStart()
		{
			if (Interlocked.Increment(ref _busyCounter) == 1)
			{
				NotifyPropertyChanged(nameof(IsBusy));
			}
		}

		internal void NotifyOperationEnd()
		{
			if (Interlocked.Decrement(ref _busyCounter) == 0)
			{
				NotifyPropertyChanged(nameof(IsBusy));
			}
		}
	}
}
