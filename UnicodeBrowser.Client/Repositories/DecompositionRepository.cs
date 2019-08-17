using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnicodeBrowser.Models;

namespace UnicodeBrowser.Client.Repositories
{
	internal sealed class DecompositionRepository : RepositoryBase
	{
		public DecompositionRepository(ApplicationState applicationState, HttpClient httpClient)
			: base(applicationState, httpClient)
		{
		}

		public async Task<CodePoint[]> DecomposeTextAsync(string text, CancellationToken cancellationToken)
		{
			BeginAsyncOperation();
			try
			{
				using var content = new StringContent(text, Encoding.UTF8);
				return await HttpClient.GetItemsAsync<CodePoint>("/api/text/decompose", content, cancellationToken);
			}
			finally
			{
				EndAsyncOperation();
			}
		}
	}
}
