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
using EPR.Calculator.Service.Function.Messaging;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using EPR.Calculator.Service.Function.Services.DataLoading;
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
    /// <summary>
    ///     Configures the services for the Azure Functions application.
    /// </summary>
    /// <param name="builder">The functions host builder.</param>
    public override void Configure(IFunctionsHostBuilder builder)
    {
        RegisterDatabase(builder.Services);
        RegisterBlobStorage(builder.Services);
        RegisterDependencies(builder.Services);
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

    private static void RegisterDependencies(IServiceCollection services)
    {
        services.AddTransient<ICalculatorRunContextBuilder, CalculatorRunContextBuilder>();
        services.AddTransient<ICalculatorRunProcessor, CalculatorRunProcessor>();
        services.AddTransient<ICalculatorRunInitializer, CalculatorRunInitializer>();
        services.AddTransient<ICalculatorRunFinalizer, CalculatorRunFinalizer>();
        services.AddTransient<ICalculatorFileExporter, CalculatorFileExporter>();
        services.AddTransient<IResultsFileCsvWriter, ResultsFileCsvWriter>();

        services.AddTransient<IBillingRunContextBuilder, BillingRunContextBuilder>();
        services.AddTransient<IBillingRunProcessor, BillingRunProcessor>();
        services.AddTransient<IBillingRunFinalizer, BillingRunFinalizer>();
        services.AddTransient<IBillingFileExporter, BillingFileExporter>();
        services.AddTransient<IBillingFileCsvWriter, BillingFileCsvWriter>();
        services.AddTransient<IBillingFileJsonWriter, BillingFileJsonWriter>();

        services.AddTransient<IBillingInstructionService, BillingInstructionService>();
        services.AddTransient<ITransposePomAndOrgDataService, TransposePomAndOrgDataService>();
        services.AddTransient<ICalcResultBuilder, CalcResultBuilder>();
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
        services.AddTransient<ICalcResultSummaryBuilder, CalcResultSummaryBuilder>();
        services.AddTransient<IOnePlusFourApportionmentExporter, OnePlusFourApportionmentExporter>();
        services.AddTransient<ICalculatorRunOrgData, CalculatorRunOrgData>();
        services.AddTransient<ICalculatorRunPomData, CalculatorRunPomData>();
        services.AddTransient<ILapcaptDetailExporter, LapcaptDetailExporter>();
        services.AddTransient<ICalcResultDetailExporter, CalcResultDetailexporter>();
        services.AddTransient<ICalcResultLaDisposalCostExporter, CalcResultLaDisposalCostExporter>();
        services.AddTransient<ICalcResultScaledupProducersExporter, CalcResultScaledupProducersExporter>();
        services.AddTransient<ICalcResultPartialObligationsExporter, CalcResultPartialObligationsExporter>();
        services.AddTransient<ICalcResultProjectedProducersExporter, CalcResultProjectedProducersExporter>();
        services.AddTransient<ICalcResultRejectedProducersExporter, CalcResultRejectedProducersExporter>();
        services.AddTransient<LateReportingExporter, LateReportingExporter>();
        services.AddTransient<ICalcResultParameterOtherCostExporter, CalcResultParameterOtherCostExporter>();
        services.AddTransient<ICommsCostExporter, CommsCostExporter>();
        services.AddTransient<ICalcResultSummaryExporter, CalcResultSummaryExporter>();
        services.AddTransient<ILateReportingExporter, LateReportingExporter>();
        services.AddTransient<IMaterialService, MaterialService>();
        services.AddTransient<IMessageTypeService, MessageTypeService>();
        services.AddTransient<ICalcCountryApportionmentService, CalcCountryApportionmentService>();
        services.AddTransient<IInvoicedProducerService, InvoicedProducerService>();
        services.AddTransient<IProjectedProducersService, ProjectedProducersService>();
        services.AddTransient<ICalcResultCancelledProducersBuilder, CalcResultCancelledProducersBuilder>();
        services.AddTransient<ICalcResultCancelledProducersExporter, CalcResultCancelledProducersExporter>();
        services.AddTransient<IProducerInvoiceNetTonnageService, ProducerInvoiceNetTonnageService>();
        services.AddTransient<ICalcResultErrorReportBuilder, CalcResultErrorReportBuilder>();
        services.AddTransient<ICalcResultErrorReportExporter, CalcResultErrorReportExporter>();
        services.AddTransient<IErrorReportService, ErrorReportService>();

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

        services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
        services.AddSingleton<IBulkOperations, BulkOperationsWrapper>();
        services.AddSingleton<ITelemetryClient, TelemetryClientWrapper>();
        services.AddTransient<IDataLoader, CommonDataApiLoader>();
    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        builder.ConfigurationBuilder.Build();

        builder.ConfigurationBuilder
            .SetBasePath(Environment.CurrentDirectory)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
            .AddEnvironmentVariables()
            .Build();
    }
}
