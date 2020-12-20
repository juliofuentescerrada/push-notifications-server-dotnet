namespace Pusher.PushNotifications.TestServer
{
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Hosting;

    public sealed class TestFixture : WebApplicationFactory<Startup>
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder().ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>().UseTestServer());
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            return base.CreateHost(builder.UseContentRoot(Directory.GetCurrentDirectory()));
        }
    }
}