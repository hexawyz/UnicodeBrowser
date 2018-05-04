using System.IO;
using System.Text;

namespace UnicodeBrowser.Metadata
{
	internal static class BlockMetadata
    {
		public static string GetDescription(string blockName)
		{
			var stream = typeof(BlockMetadata).Assembly.GetManifestResourceStream(typeof(BlockMetadata), "Blocks.Descriptions." + blockName + ".md");

			if (stream == null) return null;

			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}
    }
}
