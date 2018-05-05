using Microsoft.AspNetCore.Blazor;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnicodeBrowser.Client.Models;

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
				var response = await HttpClient.PostAsync("/api/text/decompose", new StringContent(text, Encoding.UTF8));
				cancellationToken.ThrowIfCancellationRequested();
				return JsonUtil.Deserialize<CodePoint[]>(await response.Content.ReadAsStringAsync());
			}
			finally
			{
				EndAsyncOperation();
			}
		}
	}
}
