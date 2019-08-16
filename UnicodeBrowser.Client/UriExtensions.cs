using System;

namespace UnicodeBrowser.Client
{
	internal static class UriExtensions
	{
		// Quickly hacked method to retrieve the value of a query parameter.
		// It only retrieves the first matched value.
		public static string GetQueryParameter(this Uri uri, string parameterName)
			=> Uri.UnescapeDataString(uri.FindQueryParameter(parameterName).ToString());

		private static ReadOnlyMemory<char> FindQueryParameter(this Uri uri, string parameterName)
		{
			string query = uri.Query;
			var span = uri.Query.AsSpan();
			int currentIndex;

			if (span.Length > 0)
			{
				// Skip the initial ?
				span = span.Slice(1);
				currentIndex = 1;

				while (true)
				{
					if (span.Length == 0) break;

					bool parameterFound = false;

					if (span.StartsWith(parameterName.AsSpan(), StringComparison.OrdinalIgnoreCase))
					{
						span = span.Slice(parameterName.Length);
						currentIndex += parameterName.Length;

						char c = span[0];

						// Abort parsing if the parameter is empty.
						if (c == '&') break;

						if (c == '=')
						{
							span = span.Slice(1);
							currentIndex += 1;
							parameterFound = true;
						}
					}

					int length = span.IndexOf('&');

					if (parameterFound)
					{
						if (length == 0 || span.Length == 0) break;

						return length < 0 ?
							query.AsMemory(currentIndex) :
							query.AsMemory(currentIndex, length);
					}

					if (length < 0) break;

					span.Slice(++length);
					currentIndex += length;
				}
			}

			return default;
		}
	}
}
