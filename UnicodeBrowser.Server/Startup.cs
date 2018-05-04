using Microsoft.AspNetCore.Blazor.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Net.Mime;
using UnicodeBrowser.Controllers;
using UnicodeBrowser.Json;
using UnicodeBrowser.MediaFormatters;
using UnicodeBrowser.Mvc;
using UnicodeBrowser.Search;
using UnicodeBrowser.Services;

namespace UnicodeBrowser.Server
{
	public class Startup
	{
		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddMvcCore
				(
					options =>
					{
						options.InputFormatters.Add(new PlainTextInputFormatter());
						options.CacheProfiles.Add("BlockCacheProfile", new CacheProfile { Duration = 7 * 24 * 60 * 60, Location = ResponseCacheLocation.Any });
						options.CacheProfiles.Add("CodePointCacheProfile", new CacheProfile { Duration = 7 * 24 * 60 * 60, Location = ResponseCacheLocation.Client });
						options.CacheProfiles.Add("CodePointRangeCacheProfile", new CacheProfile { Duration = 24 * 60 * 60, Location = ResponseCacheLocation.Client, VaryByHeader = "Range" });
						options.CacheProfiles.Add("TextDecompositionCacheProfile", new CacheProfile { Duration = 60 * 60, Location = ResponseCacheLocation.Client });
					}
				)
				.ConfigureApplicationPartManager
				(
					applicationPartManager =>
					{
						var featureProviders = applicationPartManager.FeatureProviders;

						for (int i = 0; i < featureProviders.Count; i++)
						{
							var featureProvider = featureProviders[i];

							if (featureProvider is ControllerFeatureProvider)
							{
								featureProviders[i] = new NonPublicControllerFeatureProvider();
								break;
							}
						}
					}
				)
				.AddJsonFormatters
				(
					options =>
					{
						//options.ContractResolver = new CamelCasePropertyNamesContractResolver();
						options.ContractResolver = new DefaultContractResolver();
						options.Converters.Add(new FlagsEnumConverter());
						options.Converters.Add(new StringEnumConverter());
						options.DefaultValueHandling = DefaultValueHandling.Ignore;
						options.NullValueHandling = NullValueHandling.Ignore;
					}
				)
				.AddControllersAsServices();

			services.AddResponseCompression(options =>
			{
				options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
				{
					MediaTypeNames.Application.Octet,
					WasmMediaTypeNames.Application.Wasm,
				});
			});

			services.AddSingleton<ICharacterSearchService, CharacterSearchService>();

			// Add every controller as a singleton
			services.AddSingleton<BlocksController>();
			services.AddSingleton<CodePointsController>();
			services.AddSingleton<SearchController>();
			services.AddSingleton<TextController>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseResponseCompression();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMvc();

			app.UseBlazor<Client.Program>();
		}
	}
}
