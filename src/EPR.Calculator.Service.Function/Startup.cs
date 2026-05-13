using System.Configuration;
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
using EPR.Calculator.Service.Function.Exporter;
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
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Messaging;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using EPR.Calculator.Service.Function.Services.DataLoading;
using EPR.Calculator.Service.Function.Telemetry;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Calculator.Service.Function;

/// <summary>
///     Configures the startup for the Azure Functions.
/// </summary>
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

    public override void Configure(IFunctionsHostBuilder builder) => builder.Services.AddAppDependencies();
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
        RegisterCommonDependencies(services);

        return services;
    }

    private static void RegisterCoreDependencies(IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
        services.AddTransient<IConfigurationService, Configuration>();
    }

    private static void RegisterTelemetry(IServiceCollection services)
    {
        services.AddSingleton<ICalculatorTelemetryLogger, CalculatorTelemetryLogger>(sp =>
        {
            var key = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
            var fallbackToConsole =
                string.IsNullOrWhiteSpace(key) ||
                key == "00000000-0000-0000-0000-000000000000";
            return ActivatorUtilities.CreateInstance<CalculatorTelemetryLogger>(sp, fallbackToConsole);
        });
    }

    private static void RegisterDatabase(IServiceCollection services)
    {
        services.AddDbContextFactory<ApplicationDBContext>(options =>
        {
            var config = services.BuildServiceProvider().GetRequiredService<IConfigurationService>();
            options.UseSqlServer(config.DbConnectionString);
        });

        services.AddSingleton<IBulkOperations, BulkOperationsWrapper>();
    }

    private static void RegisterBlobStorage(IServiceCollection services)
    {
        services.AddSingleton<IStorageService>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfigurationService>();
            var logger = provider.GetRequiredService<ICalculatorTelemetryLogger>();
            var connectionString = configuration.BlobConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                throw new ConfigurationErrorsException("Blob Storage connection string is not configured.");

            return new BlobStorageService(new BlobServiceClient(connectionString), logger);
        });
    }

    private static void RegisterCalculatorRunDependencies(IServiceCollection services)
    {
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

        services.AddTransient<IDataLoader, CommonDataApiLoader>();

        services.AddTransient<ICalculatorRunOrgData, CalculatorRunOrgData>();
        services.AddTransient<ICalculatorRunPomData, CalculatorRunPomData>();
        services.AddTransient<IProducerDataTransposer, ProducerDataTransposer>();
    }

    private static void RegisterCommonDependencies(IServiceCollection services)
    {
        services.AddTransient<ICalculatorRunService, CalculatorRunService>();
        services.AddTransient<ICalculatorRunParameterMapper, CalculatorRunParameterMapper>();
        services.AddTransient<IRpdStatusDataValidator, RpdStatusDataValidator>();
        services.AddTransient<IOrgAndPomWrapper, OrgAndPomWrapper>();
        services.AddTransient<ICalcResultBuilder, CalcResultBuilder>();
        services.AddTransient<ICalcResultsExporter<CalcResult>, CalcResultsExporter>();
        services.AddTransient<CalculatorRunValidator, CalculatorRunValidator>();
        services.AddTransient<ICommandTimeoutService, CommandTimeoutService>();
        services.AddTransient<IPrepareCalcService, PrepareCalcService>();
        services.AddTransient<IParameterService, ParameterService>();
        services.AddTransient<ICalcResultDetailBuilder, CalcResultDetailBuilder>();
        services.AddTransient<ICalcResultLapcapDataBuilder, CalcResultLapcapDataBuilder>();
        services.AddTransient<ICalcResultParameterOtherCostBuilder, CalcResultParameterOtherCostBuilder>();
        services.AddTransient<ICalcResultOnePlusFourApportionmentBuilder, CalcResultOnePlusFourApportionmentBuilder>();
        services.AddTransient<ICalcResultCommsCostBuilder, CalcResultCommsCostBuilder>();
        services.AddTransient<ICalcResultLateReportingBuilder, CalcResultLateReportingBuilder>();
        services.AddTransient<ICalcRunLaDisposalCostBuilder, CalcRunLaDisposalCostBuilder>();
        services.AddTransient<ICalcResultScaledupProducersBuilder, CalcResultScaledupProducersBuilder>();
        services.AddTransient<ICalcResultPartialObligationBuilder, CalcResultPartialObligationBuilder>();
        services.AddTransient<ICalcResultProjectedProducersBuilder, CalcResultProjectedProducersBuilder>();
        services.AddTransient<ICalcResultRejectedProducersBuilder, CalcResultRejectedProducersBuilder>();
        services.AddTransient<ICalcResultModulationBuilder, CalcResultModulationBuilder>();
        services.AddTransient<ICalcResultSummaryBuilder, CalcResultSummaryBuilder>();
        services.AddTransient<IBillingInstructionService, BillingInstructionService>();
        services.AddTransient<IOnePlusFourApportionmentExporter, OnePlusFourApportionmentExporter>();
        services.AddTransient<IRpdStatusService, RpdStatusService>();
        services.AddTransient<ILapcaptDetailExporter, LapcaptDetailExporter>();
        services.AddTransient<ICalcResultDetailExporter, CalcResultDetailexporter>();
        services.AddTransient<ICalcResultLaDisposalCostExporter, CalcResultLaDisposalCostExporter>();
        services.AddTransient<ICalcResultScaledupProducersExporter, CalcResultScaledupProducersExporter>();
        services.AddTransient<ICalcResultPartialObligationsExporter, CalcResultPartialObligationsExporter>();
        services.AddTransient<ICalcResultRejectedProducersExporter, CalcResultRejectedProducersExporter>();
        services.AddTransient<ICalcResultProjectedProducersExporter, CalcResultProjectedProducersExporter>();
        services.AddTransient<LateReportingExporter, LateReportingExporter>();
        services.AddTransient<ICalcResultParameterOtherCostExporter, CalcResultParameterOtherCostExporter>();
        services.AddTransient<ICalcResultModulationExporter, CalcResultModulationExporter>();
        services.AddTransient<ICommsCostExporter, CommsCostExporter>();
        services.AddTransient<ICalcResultSummaryExporter, CalcResultSummaryExporter>();
        services.AddTransient<ICalcBillingJsonExporter<CalcResult>, CalcResultsJsonExporter>();
        services.AddTransient<ILateReportingExporter, LateReportingExporter>();
        services.AddTransient<IRunNameService, RunNameService>();
        services.AddTransient<IClassificationService, ClassificationService>();
        services.AddTransient<IMaterialService, MaterialService>();
        services.AddTransient<ITelemetryClientWrapper, TelemetryClientWrapper>();
        services.AddTransient<IMessageTypeService, MessageTypeService>();
        services.AddTransient<IPrepareBillingFileService, PrepareBillingFileService>();
        services.AddTransient<ICalcCountryApportionmentService, CalcCountryApportionmentService>();
        services.AddTransient<IInvoicedProducerService, InvoicedProducerService>();
        services.AddTransient<ICalcResultCancelledProducersBuilder, CalcResultCancelledProducersBuilder>();
        services.AddTransient<ICalcResultCancelledProducersExporter, CalcResultCancelledProducersExporter>();
        services.AddTransient<IBillingFileExporter<CalcResult>, BillingFileExporter>();
        services.AddTransient<IProducerInvoiceNetTonnageService, ProducerInvoiceNetTonnageService>();
        services.AddTransient<IProducerInvoiceTonnageMapper, ProducerInvoiceTonnageMapper>();
        services.AddTransient<IPrepareProducerDataInsertService, PrepareProducerDataInsertService>();
        services.AddTransient<ICalcResultErrorReportBuilder, CalcResultErrorReportBuilder>();
        services.AddTransient<ICalcResultErrorReportExporter, CalcResultErrorReportExporter>();
        services.AddTransient<IErrorReportService, ErrorReportService>();
        services.AddTransient<IProjectedProducersService, ProjectedProducersService>();
        services.AddTransient<ISelfManagedConsumerWasteService, SelfManagedConsumerWasteService>();
        services.AddTransient<IReportedProducerService, ReportedProducerService>();
    }
}
