using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using UnicodeBrowser.Client.Repositories;

namespace UnicodeBrowser.Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new BrowserServiceProvider(services =>
            {
				services.AddSingleton<ApplicationState>();
				services.AddSingleton<BlockRepository>();
				services.AddSingleton<BlockCodePointRepository>();
				services.AddSingleton<CodePointRepository>();
				services.AddSingleton<DecompositionRepository>();
				services.AddSingleton<SearchRepository>();
			});

			new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}
