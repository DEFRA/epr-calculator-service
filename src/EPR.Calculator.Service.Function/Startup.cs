﻿// <copyright file="Startup.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Configuration;
using Azure.Storage.Blobs;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using EPR.Calculator.Service.Common.AzureSynapse;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using EPR.Calculator.Service.Common.Logging;
using System;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.LateReportingTonnage;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.BillingInstructions;

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
            services.AddScoped<ICalcResultScaledupProducersBuilder, CalcResultScaledupProducersBuilder>();
            services.AddTransient<ICalcResultSummaryBuilder, CalcResultSummaryBuilder>();
            services.AddTransient<IBillingInstructionService, BillingInstructionService>();
            services.AddTransient<IOnePlusFourApportionmentExporter, OnePlusFourApportionmentExporter>();
            services.AddTransient<IRpdStatusService, RpdStatusService>();
            services.AddTransient<ILapcaptDetailExporter, LapcaptDetailExporter>();
            services.AddTransient<ICalcResultDetailExporter, CalcResultDetailexporter>();
            services.AddTransient<ICalcResultLaDisposalCostExporter, CalcResultLaDisposalCostExporter>();
            services.AddTransient<ICalcResultScaledupProducersExporter, CalcResultScaledupProducersExporter>();
            services.AddTransient<LateReportingExporter, LateReportingExporter>();
            services.AddTransient<ICalcResultParameterOtherCostExporter, CalcResultParameterOtherCostExporter>();
            services.AddTransient<ICommsCostExporter, CommsCostExporter>();
            services.AddTransient<IDbLoadingChunkerService<ProducerDetail>, DbLoadingChunkerService<ProducerDetail>>();
            services.AddTransient<IDbLoadingChunkerService<ProducerReportedMaterial>, DbLoadingChunkerService<ProducerReportedMaterial>>();
            services.AddTransient<IDbLoadingChunkerService<ProducerResultFileSuggestedBillingInstruction>, DbLoadingChunkerService<ProducerResultFileSuggestedBillingInstruction>>();
            services.AddTransient<ICalcResultSummaryExporter, CalcResultSummaryExporter>();
            services.AddTransient<ILateReportingExporter, LateReportingExporter>();
            services.AddTransient<IRunNameService, RunNameService>();
            services.AddTransient<ITelemetryClientWrapper, TelemetryClientWrapper>();
            services.AddTransient<IMessageTypeService, MessageTypeService>();
            services.AddTransient<IPrepareBillingFileService, PrepareBillingFileService>();
            services.AddTransient<ILateReportingTonnageMapper, LateReportingTonnageMapper>();
            services.AddTransient<ILateReportingTonnage, LateReportingTonnage>();
            services.AddTransient<ICommsCostsByMaterialFeesSummary2aMapper, CommsCostsByMaterialFeesSummary2aMapper>();
            services.AddTransient<ICalcCountryApportionmentService, CalcCountryApportionmentService>();
            services.AddTransient<ICalcResultCommsCostByMaterial2AJsonExporter, CalcResultCommsCostByMaterial2AJsonExporter>();
            services.AddTransient<ISAOperatingCostsWithBadDebtProvisionMapper, SAOperatingCostsWithBadDebtProvisionMapper>();
            services.AddTransient<IFeeForCommsCostsWithBadDebtProvision2aMapper, FeeForCommsCostsWithBadDebtProvision2aMapper>();
            services.AddTransient<ITotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper, TotalProducerFeeWithBadDebtProvisibadDebProvisionFor2con_1_2a_2b_2cMapper>();
            services.AddTransient<IFeeForSASetUpCostsWithBadDebtProvision_5Mapper, FeeForSASetUpCostsWithBadDebtProvision_5Mapper>();
            services.AddTransient<ICalcResultCommsCostsWithBadDebtProvision2cMapper, CalcResultCommsCostsWithBadDebtProvision2cMapper>();
            services.AddTransient<ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsExporter, CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsExporter>();
            services.AddTransient<ICalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper, CalculationOfSuggestedBillingInstructionsAndInvoiceAmountsMapper>();
#if !DEBUG
            SetupBlobStorage(services);
            services.AddTransient<IConfigurationService, Configuration>();
#elif DEBUG
            services.AddTransient<IStorageService, LocalFileStorageService>();
            services.AddTransient<IConfigurationService, LocalDevelopmentConfiguration>();
#endif
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var builtConfig = builder.ConfigurationBuilder.Build();

            builder.ConfigurationBuilder
               .SetBasePath(Environment.CurrentDirectory)
               .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
               .AddEnvironmentVariables()
               .Build();

        }
    }
}