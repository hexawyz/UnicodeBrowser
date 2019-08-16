using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using UnicodeBrowser.Client.Repositories;

namespace UnicodeBrowser.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ApplicationState>();
            services.AddSingleton<BlockRepository>();
            services.AddSingleton<BlockCodePointRepository>();
            services.AddSingleton<CodePointRepository>();
            services.AddSingleton<DecompositionRepository>();
            services.AddSingleton<SearchRepository>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
