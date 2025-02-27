// <copyright file="Startup.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Azure.Storage.Blobs;
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
using EPR.Calculator.Service.Function.Data;
using EPR.Calculator.Service.Function.Exporter;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

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
            builder.Services.AddDbContext<ApplicationDBContext>(options =>
            {
                var config = builder.Services.BuildServiceProvider().GetRequiredService<IConfigurationService>();
                options.UseSqlServer(
                    config.DbConnectionString);
            });

            SetupBlobStorage(builder);
        }

        private static void SetupBlobStorage(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<BlobServiceClient>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfigurationService>();
                var connectionString = configuration.BlobConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new ConfigurationErrorsException("Blob Storage connection string is not configured.");
                }

                return new BlobServiceClient(connectionString);
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
            services.AddTransient<IOnePlusFourApportionmentExporter, OnePlusFourApportionmentExporter>();
#if !DEBUG
            services.AddTransient<IStorageService, BlobStorageService>();
            services.AddTransient<IConfigurationService, Configuration>();
#elif DEBUG
            services.AddTransient<IStorageService, LocalFileStorageService>();
            services.AddTransient<IConfigurationService, LocalDevelopmentConfiguration>();
#endif
        }
    }
}
