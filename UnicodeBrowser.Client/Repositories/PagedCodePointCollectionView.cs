using System.Net.Http;
using UnicodeBrowser.Client.Models;

namespace UnicodeBrowser.Client.Repositories
{
	internal sealed class PagedCodePointCollectionView : PagedCollectionView<CodePoint>
    {
		private const int CodePointPageSize = 256;

		public PagedCodePointCollectionView(ApplicationState applicationState, HttpClient httpClient, int baseIndex, int itemCount) 
			: base(applicationState, httpClient, "/api/codepoints", baseIndex, itemCount, CodePointPageSize)
		{
		}

		protected override void AssignSparseItems(CodePoint[] items, int offset, int count)
		{
			foreach (var codePoint in items)
			{
				Items[codePoint.Index - BaseIndex] = codePoint;
			}
		}
	}
}
