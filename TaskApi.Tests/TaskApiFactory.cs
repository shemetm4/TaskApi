using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TaskApi.Abstractions;

namespace TaskApi.Tests;

public class TaskApiFactory : WebApplicationFactory<Program>
{
    public FakeTaskEventPublisher FakePublisher { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(d => d.ServiceType == typeof(ITaskEventPublisher));

            services.Remove(descriptor);
            services.AddSingleton<ITaskEventPublisher>(FakePublisher);
        });
    }
}
