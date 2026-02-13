// <copyright file="Startup.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Azure.Storage.Blobs;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using EPR.Calculator.Service.Common.AzureSynapse;
using EPR.Calculator.Service.Common.Logging;
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
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Reflection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Calculator.Service.Function
{
    /// <summary>
    /// Configures the startup for the Azure Functions.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Configures the services for the Azure Functions application.
        /// </summary>
        /// <param name="builder">The functions host builder.</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            RegisterDependencies(builder.Services);

            // Configure the database context.
            builder.Services.AddDbContextFactory<ApplicationDBContext>(options =>
            {
                var config = builder.Services.BuildServiceProvider().GetRequiredService<IConfigurationService>();
                options.UseSqlServer(
                    config.DbConnectionString);
            });

            // Register CustomTelemetryLogger
            builder.Services.AddSingleton<ICalculatorTelemetryLogger, CalculatorTelemetryLogger>(sp =>
            {
                var fallbackToConsole = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY") ==
                                        "00000000-0000-0000-0000-000000000000";

                return ActivatorUtilities.CreateInstance<CalculatorTelemetryLogger>(sp, fallbackToConsole);
            });
        }

        private static void SetupBlobStorage(IServiceCollection services)
        {
            services.AddSingleton<IStorageService>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfigurationService>();
                var logger = provider.GetRequiredService<ICalculatorTelemetryLogger>();
                var connectionString = configuration.BlobConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new ConfigurationErrorsException("Blob Storage connection string is not configured.");
                }

                return new BlobStorageService(new BlobServiceClient(connectionString), configuration, logger);
            });
        }

        /// <summary>
        /// Registers the dependencies for the application.
        /// </summary>
        /// <param name="services">The service collection.</param>
        private static void RegisterDependencies(IServiceCollection services)
        {
            services.AddTransient<ICalculatorRunService, CalculatorRunService>();
            services.AddTransient<ICalculatorRunParameterMapper, CalculatorRunParameterMapper>();
            services.AddTransient<IAzureSynapseRunner, AzureSynapseRunner>();
            services.AddTransient<IPipelineClientFactory, PipelineClientFactory>();
            services.AddTransient<ITransposePomAndOrgDataService, TransposePomAndOrgDataService>();
            services.AddTransient<IRpdStatusDataValidator, RpdStatusDataValidator>();
            services.AddTransient<IOrgAndPomWrapper, OrgAndPomWrapper>();
            services.AddTransient<ICalcResultBuilder, CalcResultBuilder>();
            services.AddTransient<ICalcResultsExporter<CalcResult>, CalcResultsExporter>();
            services.AddTransient<CalculatorRunValidator, CalculatorRunValidator>();
            services.AddTransient<ICommandTimeoutService, CommandTimeoutService>();
            services.AddTransient<IPrepareCalcService, PrepareCalcService>();
            services.AddTransient<ICalcResultDetailBuilder, CalcResultDetailBuilder>();
            services.AddTransient<ICalcResultLapcapDataBuilder, CalcResultLapcapDataBuilder>();
            services.AddTransient<ICalcResultParameterOtherCostBuilder, CalcResultParameterOtherCostBuilder>();
            services.AddTransient<ICalcResultOnePlusFourApportionmentBuilder, CalcResultOnePlusFourApportionmentBuilder>();
            services.AddTransient<ICalcResultCommsCostBuilder, CalcResultCommsCostBuilder>();
            services.AddTransient<ICalcResultLateReportingBuilder, CalcResultLateReportingBuilder>();
            services.AddTransient<ICalcRunLaDisposalCostBuilder, CalcRunLaDisposalCostBuilder>();
            services.AddTransient<ICalcResultScaledupProducersBuilder, CalcResultScaledupProducersBuilder>();
            services.AddTransient<ICalcResultPartialObligationBuilder, CalcResultPartialObligationBuilder>();
            services.AddTransient<ICalcResultRejectedProducersBuilder, CalcResultRejectedProducersBuilder>();
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
            services.AddTransient<LateReportingExporter, LateReportingExporter>();
            services.AddTransient<ICalcResultParameterOtherCostExporter, CalcResultParameterOtherCostExporter>();
            services.AddTransient<ICommsCostExporter, CommsCostExporter>();
            services.AddTransient<IDbLoadingChunkerService<ProducerDetail>, DbLoadingChunkerService<ProducerDetail>>();
            services.AddTransient<IDbLoadingChunkerService<ProducerReportedMaterial>, DbLoadingChunkerService<ProducerReportedMaterial>>();
            services.AddTransient<IDbLoadingChunkerService<ProducerResultFileSuggestedBillingInstruction>, DbLoadingChunkerService<ProducerResultFileSuggestedBillingInstruction>>();
            services.AddTransient<IDbLoadingChunkerService<ErrorReport>, DbLoadingChunkerService<ErrorReport>>();
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
            services.AddTransient<IProducerDetailService, ProducerDetailService>();
            services.AddTransient<ICalcResultCancelledProducersBuilder, CalcResultCancelledProducersBuilder>();
            services.AddTransient<ICalcResultCancelledProducersExporter, CalcResultCancelledProducersExporter>();
            services.AddTransient<IBillingFileExporter<CalcResult>, BillingFileExporter>();
            services.AddTransient<IProducerInvoiceNetTonnageService, ProducerInvoiceNetTonnageService>();
            services.AddTransient<IDbLoadingChunkerService<ProducerInvoicedMaterialNetTonnage>, DbLoadingChunkerService<ProducerInvoicedMaterialNetTonnage>>();
            services.AddTransient<IProducerInvoiceTonnageMapper, ProducerInvoiceTonnageMapper>();
            services.AddTransient<IPrepareProducerDataInsertService, PrepareProducerDataInsertService>();
            services.AddTransient<ICalcResultErrorReportBuilder, CalcResultErrorReportBuilder>();
            services.AddTransient<ICalcResultErrorReportExporter, CalcResultErrorReportExporter>();
            services.AddTransient<IErrorReportService, ErrorReportService>();

            services.AddScoped<PrepareCalcServiceDependencies>(provider => new PrepareCalcServiceDependencies
            {
                Context = provider.GetRequiredService<ApplicationDBContext>(),
                Builder = provider.GetRequiredService<ICalcResultBuilder>(),
                Exporter = provider.GetRequiredService<ICalcResultsExporter<CalcResult>>(),
                StorageService = provider.GetRequiredService<IStorageService>(),
                ValidationRules = provider.GetRequiredService<CalculatorRunValidator>(),
                CommandTimeoutService = provider.GetRequiredService<ICommandTimeoutService>(),
                TelemetryLogger = provider.GetRequiredService<ICalculatorTelemetryLogger>(),
                JsonExporter = provider.GetRequiredService<ICalcBillingJsonExporter<CalcResult>>(),
                ConfigService = provider.GetRequiredService<IConfigurationService>(),
                BillingFileExporter = provider.GetRequiredService<IBillingFileExporter<CalcResult>>(),
                producerDataInsertService = provider.GetRequiredService<IPrepareProducerDataInsertService>(),
            });

            SetupBlobStorage(services);
            services.AddTransient<IConfigurationService, Configuration>();

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
}