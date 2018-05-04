using Microsoft.AspNetCore.Mvc;
using System;
using UnicodeBrowser.Data;
using UnicodeBrowser.Models;

namespace UnicodeBrowser.Controllers
{
	[Route("api/blocks")]
	internal sealed class BlocksController
    {
        [HttpGet]
        [ResponseCache(CacheProfileName = "BlockCacheProfile")]
        public BlockInformation[] Get() => BlockInformationProvider.GetBlocks();
        
        [HttpGet("{name}")]
        [ResponseCache(CacheProfileName = "BlockCacheProfile")]
        public BlockInformation Get(string name)
        {
            foreach (var block in BlockInformationProvider.GetBlocks())
            {
                if (string.Equals(block.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return block;
                }
            }

            return null;
        }
    }
}
