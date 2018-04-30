using Microsoft.AspNetCore.Mvc;
using System;
using System.Unicode;
using UnicodeBrowser.Models;

namespace UnicodeBrowser.Controllers
{
	[Route("api/blocks")]
    public class BlocksController
    {
        [HttpGet]
        [ResponseCache(CacheProfileName = "CodePointCacheProfile")]
        public BlockInformation[] Get()
        {
            var blocks = UnicodeInfo.GetBlocks();

            var blockViewModel = new BlockInformation[blocks.Length];

            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];

                blockViewModel[i] = new BlockInformation(block.Name, new CodePointRange(block.CodePointRange.FirstCodePoint, block.CodePointRange.LastCodePoint));
            }

            return blockViewModel;
        }
        
        [HttpGet("{name}")]
        [ResponseCache(CacheProfileName = "CodePointCacheProfile")]
        public BlockInformation Get(string name)
        {
            foreach (var block in UnicodeInfo.GetBlocks())
            {
                if (string.Equals(block.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return new BlockInformation(block.Name, new CodePointRange(block.CodePointRange.FirstCodePoint, block.CodePointRange.LastCodePoint));
                }
            }

            return null;
        }
    }
}
