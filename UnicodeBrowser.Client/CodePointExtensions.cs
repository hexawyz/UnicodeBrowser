using System.Linq;

namespace UnicodeBrowser.Client
{
	public static class CodePointExtensions
    {
		public static string GetDisplayName(this Models.CodePoint codePoint)
			=> codePoint.Name ?? codePoint.OldName ?? codePoint.NameAliases.FirstOrDefault()?.Name;
    }
}
