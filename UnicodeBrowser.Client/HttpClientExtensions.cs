using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace UnicodeBrowser.Client
{
	public static class HttpClientExtensions
	{
		internal static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true
		};

		private const string ApplicationJsonMediaType = "application/json";

		public static async Task<(long count, T[] items)> GetItemRangeAsync<T>(this HttpClient httpClient, string uri, int index, int count)
		{
			using var response = await httpClient.SendAsync
			(
				new HttpRequestMessage(HttpMethod.Get, uri)
				{
					Headers =
					{
						Accept =
						{
							new MediaTypeWithQualityHeaderValue(ApplicationJsonMediaType)
						},
						Range = new RangeHeaderValue(index, index + count - 1)
						{
							Unit = "items"
						}
					}
				}
			).ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidDataException("The server did not respond successfully.");
			}

			if (!string.Equals(response.Content.Headers.ContentType?.MediaType, ApplicationJsonMediaType, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException("The server responded with an unexpected Content-Type");
			}

			var contentRange = response.Content.Headers.ContentRange;

			if (contentRange == null || !string.Equals(contentRange.Unit, "items")) throw new InvalidDataException();

			long itemCount = (contentRange.To - index + 1) ?? count;

			return (itemCount, JsonSerializer.Deserialize<T[]>(await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false), JsonSerializerOptions));
		}

		public static Task<T[]> GetItemsAsync<T>(this HttpClient httpClient, string uri, CancellationToken cancellationToken)
			=> httpClient.GetItemsAsync<T>(HttpMethod.Get, uri, null, cancellationToken);

		public static Task<T[]> GetItemsAsync<T>(this HttpClient httpClient, string uri, HttpContent content, CancellationToken cancellationToken)
			=> httpClient.GetItemsAsync<T>(HttpMethod.Post, uri, content, cancellationToken);

		private static async Task<T[]> GetItemsAsync<T>(this HttpClient httpClient, HttpMethod method, string uri, HttpContent content, CancellationToken cancellationToken)
		{
			using var response = await httpClient.SendAsync
			(
				new HttpRequestMessage(HttpMethod.Get, uri)
				{
					Method = method,
					Headers =
					{
						Accept =
						{
							new MediaTypeWithQualityHeaderValue(ApplicationJsonMediaType)
						},
					},
					Content = content
				},
				cancellationToken.CanBeCanceled ?
					HttpCompletionOption.ResponseHeadersRead :
					HttpCompletionOption.ResponseContentRead
			).ConfigureAwait(false);

			cancellationToken.ThrowIfCancellationRequested();

			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidDataException("The server did not respond successfully.");
			}

			if (!string.Equals(response.Content.Headers.ContentType?.MediaType, ApplicationJsonMediaType, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException("The server responded with an unexpected Content-Type");
			}

			return JsonSerializer.Deserialize<T[]>(await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false), JsonSerializerOptions);
		}
	}
}
