using System.Globalization;

namespace UnicodeBrowser.Client
{
	public static class CodePointFormatter
    {
		public static string ToCodePointRepresentation(this int codePoint)
			=> "U+" + codePoint.ToHexadecimal();

		public static string ToHexadecimal(this int codePoint)
			=> codePoint.ToString(codePoint < 0x100000 ? codePoint < 0x10000 ? "X4" : "X5" : "X6", CultureInfo.InvariantCulture);
	}
}
