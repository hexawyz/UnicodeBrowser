using System.Unicode;
using UnicodeBrowser.Metadata;
using UnicodeBrowser.Models;

namespace UnicodeBrowser.Data
{
	internal static class BlockInformationProvider
    {
		// Blocks will never change dynamically, so caching the instances is a good idea.
		private static readonly BlockInformation[] _blocks = GetBlocksInternal();

		private static BlockInformation[] GetBlocksInternal()
		{
			var blocks = UnicodeInfo.GetBlocks();

			var blockViewModel = new BlockInformation[blocks.Length];

			for (int i = 0; i < blocks.Length; i++)
			{
				var block = blocks[i];

				blockViewModel[i] = new BlockInformation(block.Name, BlockMetadata.GetDescription(block.Name), new CodePointRange(block.CodePointRange.FirstCodePoint, block.CodePointRange.LastCodePoint));
			}

			return blockViewModel;
		}

		public static BlockInformation[] GetBlocks() => _blocks;
	}
}
