namespace Pusher.PushNotifications.TestServer
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<PushNotificationsOptions>(Configuration.GetSection(nameof(PushNotificationsOptions)));
            services.AddScoped(provider => provider.GetRequiredService<IOptions<PushNotificationsOptions>>().Value);
            services.AddHttpClient<PushNotifications>();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}