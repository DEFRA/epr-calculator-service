using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EPR.Calculator.Service.Function.UnitTests;

[TestCategory(TestCategories.Core)]
[TestClass]
public class StartupTests
{
    [TestMethod]
    public void All_services_stemming_from_ServiceBusQueueTrigger_should_resolve()
    {
        var services = new ServiceCollection();

        // Manually wire up a few things since we're not calling from a real Functions Host
        Environment.SetEnvironmentVariable(EnvironmentVariableKeys.BlobConnectionString, "UseDevelopmentStorage=true");

        services
            .AddTransient<IConfiguration>(_ => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"{CommonDataApiHttpClientOptions.SectionKey}:{nameof(CommonDataApiHttpClientOptions.BaseUrl)}"] = "https://ignored"
                })
                .Build())
            .AddTransient<TelemetryClient>(_ => new TelemetryClient(new TelemetryConfiguration
            {
                ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000"
            }))
            .AddTransient<ApplicationDBContext>(provider => provider
                .GetRequiredService<IDbContextFactory<ApplicationDBContext>>()
                .CreateDbContext())
            .AddTransient<ServiceBusQueueTrigger>();

        var rootService = services
            .AddAppDependencies()
            .BuildServiceProvider(true)
            .GetService<ServiceBusQueueTrigger>();

        rootService.ShouldNotBeNull();
    }
}
