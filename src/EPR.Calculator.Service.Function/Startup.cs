using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Azure.Storage.Blobs;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Builder.ErrorReport;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Features.Billing;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Billing.FileExports;
using EPR.Calculator.Service.Function.Features.Billing.FileExports.Json;
using EPR.Calculator.Service.Function.Features.Calculator;
using EPR.Calculator.Service.Function.Features.Calculator.Contexts;
using EPR.Calculator.Service.Function.Features.Calculator.FileExports;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.ExternalDataLoading;
using EPR.Calculator.Service.Function.Services.ExternalDataLoading.CommonDataApi;
using EPR.Calculator.Service.Function.Services.ExternalDataLoading.CommonDataApi.HttpClient;
using EPR.Calculator.Service.Function.Services.Telemetry;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Calculator.Service.Function;

/// <summary>
///     Configures the startup for the Azure Functions.
/// </summary>
[ExcludeFromCodeCoverage]
public class Startup : FunctionsStartup
{
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        builder.ConfigurationBuilder
            .SetBasePath(Environment.CurrentDirectory)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .AddEnvironmentVariables()
            .Build();
    }

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddAppDependencies();
    }
}

internal static class ServiceRegistration
{
    public static IServiceCollection AddAppDependencies(this IServiceCollection services)
    {
        RegisterCoreDependencies(services);
        RegisterTelemetry(services);
        RegisterDatabase(services);
        RegisterBlobStorage(services);
        RegisterCalculatorRunDependencies(services);
        RegisterBillingRunDependencies(services);
        RegisterCommonDependencies(services);
        return services;
    }

    private static void RegisterCoreDependencies(IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
    }

    private static void RegisterTelemetry(IServiceCollection services)
    {
        var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
        var isLocalDevelopment = "Development".Equals(environment, StringComparison.OrdinalIgnoreCase);

        if (isLocalDevelopment)
            services.AddSingleton<ITelemetryClient, LoggerTelemetryClient>();
        else
            services.AddSingleton<ITelemetryClient, AppInsightsTelemetryClient>();
    }

    private static void RegisterDatabase(IServiceCollection services)
    {
        services
            .AddOptions<DatabaseOptions>()
            .Configure<IConfiguration>((options, config) => { config.GetSection(DatabaseOptions.SectionKey).Bind(options); })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContextFactory<ApplicationDBContext>((provider, builder) =>
        {
            var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            builder.UseSqlServer(
                options.ConnectionString,
                sqlOptions => { sqlOptions.CommandTimeout((int)options.CommandTimeout.TotalSeconds); });
        });

        services.AddSingleton<IBulkOperations, BulkOperationsWrapper>();
    }

    private static void RegisterBlobStorage(IServiceCollection services)
    {
        services
            .AddOptions<BlobStorageOptions>()
            .Configure<IConfiguration>((options, config) => { config.GetSection(BlobStorageOptions.SectionKey).Bind(options); })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<BlobServiceClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<BlobStorageOptions>>().Value;
            return new BlobServiceClient(options.ConnectionString);
        });

        services.AddSingleton<IStorageService, BlobStorageService>();
    }

    private static void RegisterCalculatorRunDependencies(IServiceCollection services)
    {
        services.AddTransient<ICalculatorRunContextBuilder, CalculatorRunContextBuilder>();
        services.AddTransient<ICalculatorRunProcessor, CalculatorRunProcessor>();
        services.AddTransient<ICalculatorRunInitializer, CalculatorRunInitializer>();
        services.AddTransient<ICalculatorRunFinalizer, CalculatorRunFinalizer>();
        services.AddTransient<ICalculatorFileExporter, CalculatorFileExporter>();
        services.AddTransient<IResultsFileCsvWriter, ResultsFileCsvWriter>();

        services
            .AddOptions<CommonDataApiHttpClientOptions>()
            .Configure<IConfiguration>((options, config) => { config.GetSection(CommonDataApiHttpClientOptions.SectionKey).Bind(options); })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<CommonDataApiHttpClient>();

        services
            .AddOptions<CommonDataApiLoaderOptions>()
            .Configure<IConfiguration>((options, config) => { config.GetSection(CommonDataApiLoaderOptions.SectionKey).Bind(options); })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<IExternalDataLoader, CommonDataApiDataLoader>();

        services.AddTransient<ICalculatorRunOrgData, CalculatorRunOrgData>();
        services.AddTransient<ICalculatorRunPomData, CalculatorRunPomData>();
        services.AddTransient<ITransposePomAndOrgDataService, TransposePomAndOrgDataService>();
    }

    private static void RegisterBillingRunDependencies(IServiceCollection services)
    {
        services.AddTransient<IBillingRunContextBuilder, BillingRunContextBuilder>();
        services.AddTransient<IBillingRunProcessor, BillingRunProcessor>();
        services.AddTransient<IBillingRunFinalizer, BillingRunFinalizer>();
        services.AddTransient<IBillingFileExporter, BillingFileExporter>();
        services.AddTransient<IBillingFileCsvWriter, BillingFileCsvWriter>();
        services.AddTransient<IBillingFileJsonWriter, BillingFileJsonWriter>();
    }

    private static void RegisterCommonDependencies(IServiceCollection services)
    {
        services.AddTransient<IParameterService, ParameterService>();
        services.AddTransient<IBillingInstructionService, BillingInstructionService>();
        services.AddTransient<ICalcCountryApportionmentService, CalcCountryApportionmentService>();
        services.AddTransient<ICalcResultBuilder, CalcResultBuilder>();
        services.AddTransient<ICalcResultCancelledProducersBuilder, CalcResultCancelledProducersBuilder>();
        services.AddTransient<ICalcResultCancelledProducersExporter, CalcResultCancelledProducersExporter>();
        services.AddTransient<ICalcResultCommsCostBuilder, CalcResultCommsCostBuilder>();
        services.AddTransient<ICalcResultDetailBuilder, CalcResultDetailBuilder>();
        services.AddTransient<ICalcResultDetailExporter, CalcResultDetailexporter>();
        services.AddTransient<ICalcResultErrorReportBuilder, CalcResultErrorReportBuilder>();
        services.AddTransient<ICalcResultErrorReportExporter, CalcResultErrorReportExporter>();
        services.AddTransient<ICalcResultLaDisposalCostExporter, CalcResultLaDisposalCostExporter>();
        services.AddTransient<ICalcResultLapcapDataBuilder, CalcResultLapcapDataBuilder>();
        services.AddTransient<ICalcResultLateReportingBuilder, CalcResultLateReportingBuilder>();
        services.AddTransient<ICalcResultModulationBuilder, CalcResultModulationBuilder>();
        services.AddTransient<ICalcResultModulationExporter, CalcResultModulationExporter>();
        services.AddTransient<ICalcResultOnePlusFourApportionmentBuilder, CalcResultOnePlusFourApportionmentBuilder>();
        services.AddTransient<ICalcResultParameterOtherCostBuilder, CalcResultParameterOtherCostBuilder>();
        services.AddTransient<ICalcResultParameterOtherCostExporter, CalcResultParameterOtherCostExporter>();
        services.AddTransient<ICalcResultPartialObligationBuilder, CalcResultPartialObligationBuilder>();
        services.AddTransient<ICalcResultPartialObligationsExporter, CalcResultPartialObligationsExporter>();
        services.AddTransient<ICalcResultProjectedProducersBuilder, CalcResultProjectedProducersBuilder>();
        services.AddTransient<ICalcResultProjectedProducersExporter, CalcResultProjectedProducersExporter>();
        services.AddTransient<ICalcResultRejectedProducersBuilder, CalcResultRejectedProducersBuilder>();
        services.AddTransient<ICalcResultRejectedProducersExporter, CalcResultRejectedProducersExporter>();
        services.AddTransient<ICalcResultScaledupProducersBuilder, CalcResultScaledupProducersBuilder>();
        services.AddTransient<ICalcResultScaledupProducersExporter, CalcResultScaledupProducersExporter>();
        services.AddTransient<ICalcResultSummaryBuilder, CalcResultSummaryBuilder>();
        services.AddTransient<ICalcResultSummaryExporter, CalcResultSummaryExporter>();
        services.AddTransient<ICalcRunLaDisposalCostBuilder, CalcRunLaDisposalCostBuilder>();
        services.AddTransient<ICommsCostExporter, CommsCostExporter>();
        services.AddTransient<IErrorReportService, ErrorReportService>();
        services.AddTransient<IInvoicedProducerService, InvoicedProducerService>();
        services.AddTransient<ILapcaptDetailExporter, LapcaptDetailExporter>();
        services.AddTransient<ILateReportingExporter, LateReportingExporter>();
        services.AddTransient<IMaterialService, MaterialService>();
        services.AddTransient<IOnePlusFourApportionmentExporter, OnePlusFourApportionmentExporter>();
        services.AddTransient<IProducerInvoiceNetTonnageService, ProducerInvoiceNetTonnageService>();
        services.AddTransient<IProjectedProducersService, ProjectedProducersService>();
        services.AddTransient<IReportedProducerService, ReportedProducerService>();
        services.AddTransient<ISelfManagedConsumerWasteService, SelfManagedConsumerWasteService>();
        services.AddTransient<LateReportingExporter, LateReportingExporter>();
    }
}
