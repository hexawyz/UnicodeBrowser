using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UnicodeBrowser
{
	public abstract class BindableObject : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		protected void NotifyPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

		protected void SetValue<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
		{
			if (!EqualityComparer<T>.Default.Equals(value, storage))
			{
				storage = value;
				NotifyPropertyChanged(propertyName);
			}
		}

		protected void SetValue<T>(ref T storage, T value, PropertyChangedEventArgs e)
		{
			if (!EqualityComparer<T>.Default.Equals(value, storage))
			{
				storage = value;
				NotifyPropertyChanged(e);
			}
		}
	}
}
