using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UnicodeBrowser.Client.Repositories
{
	internal class PagedCollectionView<TItem> : RepositoryBase
    {
		private readonly object _syncRoot = new object();

		/// <summary>Gets the URI used to access the collection.</summary>
		private string Uri { get; }

		/// <summary>Gets the base index of the current view.</summary>
		public int BaseIndex { get; }

		/// <summary>Gets the array spanning all item slots.</summary>
		/// <remarks>
		/// This array is sized with the exact size of the collection.
		/// The length will be provided in the constructor or inferred from the 
		/// </remarks>
		public TItem[] Items { get; }

		/// <summary>Gets the intended number of items to display per page.</summary>
		public int MaxPageSize { get; }

		private int _loadedItemCount;

		/// <summary>Gets the number of items already loaded by the current view.</summary>
		public int LoadedItemCount => Volatile.Read(ref _loadedItemCount);

		/// <summary>Gets a value indicating whether all items have been loaded.</summary>
		public bool IsFullyLoaded => LoadedItemCount == Items.Length;

		private volatile Task _itemLoadingTask;

		// Not implemented yet
		//protected PagedCollectionView(ApplicationState applicationState, HttpClient httpClient, string uri, int maxPageSize) : base(applicationState)
		//{
		//	_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		//	Uri = uri;
		//	MaxPageSize = maxPageSize > 1 ?
		//		maxPageSize :
		//		throw new ArgumentOutOfRangeException(nameof(maxPageSize));
		//}

		protected PagedCollectionView(ApplicationState applicationState, HttpClient httpClient, string uri, int baseIndex, int itemCount, int maxPageSize)
			: base(applicationState, httpClient)
		{
			Uri = uri;
			BaseIndex = baseIndex >= 0 ?
				baseIndex :
				throw new ArgumentOutOfRangeException(nameof(baseIndex));
			Items = itemCount > 0 ?
				new TItem[itemCount] :
				throw new ArgumentOutOfRangeException(nameof(itemCount));
			MaxPageSize = maxPageSize > 1 ?
				maxPageSize :
				throw new ArgumentOutOfRangeException(nameof(maxPageSize));
		}
		
		/// <summary>Loads the next items in the collection if the specified threshold is not reached.</summary>
		/// <remarks>
		/// The threshold specified in <paramref name="itemCountThreshold"/> can not be greater than <see cref="Items"/>.<see cref="Array.Length"/>.
		/// </remarks>
		/// <param name="itemCountThreshold">The threshold that should be reached.</param>
		/// <returns>A task representing the status of the loading operation.</returns>
		public Task TryLoadNextItemsInternalAsync(int itemCountThreshold)
		{
			if (itemCountThreshold > Items.Length) throw new ArgumentOutOfRangeException(nameof(itemCountThreshold));

			var itemLoadingTask = _itemLoadingTask;

			if (itemLoadingTask == null || (itemLoadingTask.IsCompleted && itemCountThreshold > LoadedItemCount))
			{
				lock (_syncRoot)
				{
					itemLoadingTask = _itemLoadingTask;

					if (itemLoadingTask == null || (itemLoadingTask.IsCompleted && itemCountThreshold > LoadedItemCount))
					{
						itemLoadingTask = _itemLoadingTask = LoadNextItemsInternalAsync();
					}
				}
			}
			else if (itemCountThreshold < LoadedItemCount && _loadedItemCount > 0) // Forcefully return a completed task if any items have already been loaded and we were not required to load more.
			{
				itemLoadingTask = Task.CompletedTask;
			}

			return itemLoadingTask;
		}

		private async Task LoadNextItemsInternalAsync()
		{
			BeginAsyncOperation();
			try
			{
				// This can be a non-volatile read because this method is never called concurrently,
				// and it is the only place where _loadedItemCount is updated.
				int loadedItemCount = _loadedItemCount;

				int remainingItemCount = Items != null ? Items.Length - loadedItemCount : MaxPageSize;
				int requestedItemCount = Math.Min(MaxPageSize, remainingItemCount);

				(long count, var items) = await HttpClient.GetItemRangeAsync<TItem>(Uri, BaseIndex + loadedItemCount, requestedItemCount).ConfigureAwait(false);

				if (count > requestedItemCount) throw new InvalidDataException("The API returned more items than requested.");

				AssignItems(items, loadedItemCount, checked((int)count));

				Volatile.Write(ref _loadedItemCount, loadedItemCount + (int)count);
			}
			finally
			{
				EndAsyncOperation();
			}
		}

		private void AssignItems(TItem[] items, int offset, int count)
		{
			// If the numbers of items match perfectly, we assume that items were returned in order, and no specific mapping is required.
			if (items.Length == count)
			{
				Array.Copy(items, 0, Items, offset, items.Length);
			}
			// If the API returned less items than the page size, we consider that the response contains only non null items, which must then be mapped into their slot.
			else if (items.Length < count)
			{
				AssignSparseItems(items, offset, count);
			}
			// However, we can never allow a response with more actual items than the page size. That wouldn't make any sense.
			else
			{
				throw new InvalidDataException("The API response is incoherent.");
			}
		}

		/// <summary>Assign fetched items in the destination array when the number of items in the array is less than the proclaimed count.</summary>
		/// <remarks>
		/// This can legitimately happen when the API chooses to skip some undefined items.
		/// It is, however, the responsibility of the API to provide the metadata necessary to fit each item in its own slot.
		/// The default implementation of this method throws <see cref="NotImplementedException"/>.
		/// </remarks>
		/// <param name="items">The items returned from the API.</param>
		/// <param name="offset">The start offset requested to the API.</param>
		/// <param name="count">The number of items covered by the API response.</param>
		/// <exception cref="NotImplementedException">The method was not implemented for supporting values if type <typeparamref name="TItem"/>.</exception>
		protected virtual void AssignSparseItems(TItem[] items, int offset, int count)
			=> throw new NotImplementedException();
	}
}
