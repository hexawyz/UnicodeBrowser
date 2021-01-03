using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnicodeBrowser.Client.Repositories;

namespace UnicodeBrowser.Client
{
    public class Program
    {
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("#app");

			builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
			builder.Services.AddSingleton<ApplicationState>();
			builder.Services.AddSingleton<UnicodeVersionRepository>();
			builder.Services.AddSingleton<BlockRepository>();
			builder.Services.AddSingleton<BlockCodePointRepository>();
			builder.Services.AddSingleton<CodePointRepository>();
			builder.Services.AddSingleton<DecompositionRepository>();
			builder.Services.AddSingleton<SearchRepository>();

			await builder.Build().RunAsync();
        }
    }
}
