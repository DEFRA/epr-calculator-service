// <copyright file="Startup.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Azure.Storage.Blobs;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using EPR.Calculator.Service.Common.AzureSynapse;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.RejectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.BillingInstructions;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CalculationResults;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CancelledProducersData;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.LaDisposalCostData;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.LateReportingTonnage;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.ScaledupProducers;
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
            builder.Services.AddSingleton<ICalculatorTelemetryLogger, CalculatorTelemetryLogger>();
        }

#pragma warning disable S1144 // Keeping just in case its required in future
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
#pragma warning restore
        
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
            services.AddTransient<ICalcResultRejectedProducersBuilder, CalcResultRejectedProducersBuilder>();
            services.AddTransient<ICalcResultSummaryBuilder, CalcResultSummaryBuilder>();
            services.AddTransient<IBillingInstructionService, BillingInstructionService>();
            services.AddTransient<IOnePlusFourApportionmentExporter, OnePlusFourApportionmentExporter>();
            services.AddTransient<IRpdStatusService, RpdStatusService>();
            services.AddTransient<ILapcaptDetailExporter, LapcaptDetailExporter>();
            services.AddTransient<ICalcResultDetailExporter, CalcResultDetailexporter>();
            services.AddTransient<ICalcResultLaDisposalCostExporter, CalcResultLaDisposalCostExporter>();
            services.AddTransient<ICalcResultScaledupProducersExporter, CalcResultScaledupProducersExporter>();
            services.AddTransient<ICalcResultRejectedProducersExporter, CalcResultRejectedProducersExporter>();
            services.AddTransient<LateReportingExporter, LateReportingExporter>();
            services.AddTransient<ICalcResultParameterOtherCostExporter, CalcResultParameterOtherCostExporter>();
            services.AddTransient<ICommsCostExporter, CommsCostExporter>();
            services.AddTransient<IDbLoadingChunkerService<ProducerDetail>, DbLoadingChunkerService<ProducerDetail>>();
            services.AddTransient<IDbLoadingChunkerService<ProducerReportedMaterial>, DbLoadingChunkerService<ProducerReportedMaterial>>();
            services.AddTransient<IDbLoadingChunkerService<ProducerResultFileSuggestedBillingInstruction>, DbLoadingChunkerService<ProducerResultFileSuggestedBillingInstruction>>();
            services.AddTransient<ICalcResultSummaryExporter, CalcResultSummaryExporter>();
            services.AddTransient<ICalcBillingJsonExporter<CalcResult>, CalcResultsJsonExporter>();
            services.AddTransient<ILateReportingExporter, LateReportingExporter>();
            services.AddTransient<IRunNameService, RunNameService>();
            services.AddTransient<IClassificationService, ClassificationService>();
            services.AddTransient<IMaterialService, MaterialService>();
            services.AddTransient<ITelemetryClientWrapper, TelemetryClientWrapper>();
            services.AddTransient<IMessageTypeService, MessageTypeService>();
            services.AddTransient<IPrepareBillingFileService, PrepareBillingFileService>();
            services.AddTransient<ILateReportingTonnageMapper, LateReportingTonnageMapper>();
            services.AddTransient<ILateReportingTonnage, LateReportingTonnage>();
            services.AddTransient<ICommsCostsByMaterialFeesSummary2AMapper, CommsCostsByMaterialFeesSummary2AMapper>();
            services.AddTransient<ICalcCountryApportionmentService, CalcCountryApportionmentService>();
            services.AddTransient<ICalcResultScaledupProducersJsonExporter, CalcResultScaledupProducersJsonExporter>();
            services.AddTransient<ICalcResultScaledupProducersJsonMapper, CalcResultScaledupProducersJsonMapper>();
            services.AddTransient<ICalcResultCancelledProducersBuilder, CalcResultCancelledProducersBuilder>();
            services.AddTransient<ICalcResultCancelledProducersExporter, CalcResultCancelledProducersExporter>();
            services.AddTransient<ICalcResultCommsCostByMaterial2AJsonExporter, CalcResultCommsCostByMaterial2AJsonExporter>();
            services.AddTransient<ISAOperatingCostsWithBadDebtProvisionMapper, SAOperatingCostsWithBadDebtProvisionMapper>();
            services.AddTransient<ICalcResultLADataPrepCostsWithBadDebtProvision4Mapper, CalcResultLADataPrepCostsWithBadDebtProvision4Mapper>();
            services.AddTransient<IFeeForCommsCostsWithBadDebtProvision2AMapper, FeeForCommsCostsWithBadDebtProvision2AMapper>();
            services.AddTransient<IFeeForCommsCostsWithBadDebtProvision2BMapper, FeeForCommsCostsWithBadDebtProvision2BMapper>();
            services.AddTransient<ITotalProducerFeeWithBadDebtProvisionFor2Con12A2B2CMapper, TotalProducerFeeWithBadDebtProvisionFor2Con1And2AAnd2BAnd2CMapper>();
            services.AddTransient<IFeeForSaSetUpCostsWithBadDebtProvision5Mapper, FeeForSaSetUpCostsWithBadDebtProvision5Mapper>();
            services.AddTransient<ICalcResultCommsCostsWithBadDebtProvision2CMapper, CalcResultCommsCostsWithBadDebtProvision2CMapper>();
            services.AddTransient<ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsExporter, CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsExporter>();
            services.AddTransient<ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper, CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper>();
            services.AddTransient<IParametersOtherMapper, ParametersOtherMapper>();
            services.AddTransient<IParametersOtherJsonExporter, ParametersOtherJsonExporter>();
            services.AddTransient<ICalculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper, CalculationResultsProducerCalculationResultsFeeForLADisposalCosts1Mapper>();
            services.AddTransient<ITotalProducerBillWithBadDebtProvisionMapper, TotalProducerBillWithBadDebtProvisionMapper>();
            services.AddTransient<ICommsCostMapper, CommsCostMapper>();
            services.AddTransient<ICommsCostJsonExporter, CommsCostJsonExporter>();
            services.AddTransient<ICalcResult2ACommsDataByMaterialMapper, CalcResult2ACommsDataByMaterialMapper>();
            services.AddTransient<ICommsCostByMaterial2AExporter, CommsCostByMaterial2AExporter>();
            services.AddTransient<ICalcResultProducerCalculationResultsTotalMapper, CalcResultProducerCalculationResultsTotalMapper>();
            services.AddTransient<IDisposalFeeSummary1Mapper, DisposalFeeSummary1Mapper>();
            services.AddTransient<ICancelledProducersMapper, CancelledProducersMapper>();
            services.AddTransient<ICancelledProducersExporter, CancelledProducersExporter>();
            services.AddTransient<ICalcResultLapcapExporter, CalcResultLapcapExporter>();
            services.AddTransient<IOnePlusFourApportionmentJsonExporter, OnePlusFourApportionmentJsonExporter>();
            services.AddTransient<ICalculationResultsExporter, CalculationResultsExporter>();
            services.AddTransient<ICalcResultScaledupProducersJsonExporter, CalcResultScaledupProducersJsonExporter>();
            services.AddTransient<IOnePlusFourApportionmentJsonExporter, OnePlusFourApportionmentJsonExporter>();
            services.AddTransient<IOnePlusFourApportionmentMapper, OnePlusFourApportionmentMapper>();
            services.AddTransient<ICalcResultScaledupProducersJsonMapper, CalcResultScaledupProducersJsonMapper>();
            services.AddTransient<IProducerDisposalFeesWithBadDebtProvision1JsonMapper, ProducerDisposalFeesWithBadDebtProvision1JsonMapper>();
            services.AddTransient<ICalcResultCommsCostByMaterial2AJsonMapper, CalcResultCommsCostByMaterial2AJsonMapper>();
            services.AddTransient<ISAOperatingCostsWithBadDebtProvisionMapper, SAOperatingCostsWithBadDebtProvisionMapper>();
            services.AddTransient<ICalcResultDetailJsonExporter, CalcResultDetailJsonExporter>();
            services.AddTransient<ICalcResultLaDisposalCostDataMapper, CalcResultLaDisposalCostDataMapper>();
            services.AddTransient<ICalcResultLaDisposalCostDataExporter, CalcResultLaDisposalCostDataExporter>();
            services.AddTransient<ICalcResultCommsCostOnePlusFourApportionmentExporter, CalcResultCommsCostOnePlusFourApportionmentExporter>();
            services.AddTransient<IBillingFileExporter<CalcResult>, BillingFileExporter>();
            services.AddTransient<IProducerInvoiceNetTonnageService, ProducerInvoiceNetTonnageService>();
            services.AddTransient<IDbLoadingChunkerService<ProducerInvoicedMaterialNetTonnage>, DbLoadingChunkerService<ProducerInvoicedMaterialNetTonnage>>();
            services.AddTransient<IProducerInvoiceTonnageMapper, ProducerInvoiceTonnageMapper>();
            services.AddTransient<IPrepareProducerDataInsertService, PrepareProducerDataInsertService>();

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
#if !DEBUG

            SetupBlobStorage(services);
            services.AddTransient<IConfigurationService, Configuration>();
#elif DEBUG
            services.AddTransient<IStorageService, LocalFileStorageService>();
            services.AddTransient<IConfigurationService, LocalDevelopmentConfiguration>();
            services.AddTransient<IProducerDetailService, ProducerDetailService>();
#endif
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