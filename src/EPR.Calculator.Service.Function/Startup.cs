// <copyright file="Startup.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Common.AzureSynapse;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Misc;
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
            this.RegisterDependencies(builder.Services);

            // Configure the database context.
            builder.Services.AddDbContext<ApplicationDBContext>(options =>
            {
                options.UseSqlServer(
                    new LocalDevelopmentConfiguration().DbConnectionString);
            });
        }

        /// <summary>
        /// Registers the dependencies for the application.
        /// </summary>
        /// <param name="services">The service collection.</param>
        private void RegisterDependencies(IServiceCollection services)
        {
            services.AddTransient<ICalculatorRunService, CalculatorRunService>();
            services.AddTransient<ICalculatorRunParameterMapper, CalculatorRunParameterMapper>();
            services.AddTransient<IAzureSynapseRunner, AzureSynapseRunner>();
            services.AddTransient<IPipelineClientFactory, PipelineClientFactory>();
            services.AddTransient<ITransposePomAndOrgDataService, TransposePomAndOrgDataService>();

            // Configuration reads the configuration from environment variables. Switch to TestConfiguration
            // when testing locally to read from hard-coded strings instead.
            // TODO: Maybe replace this with loading from the config file?
#if !DEBUG
            services.AddTransient<IConfigurationService, Configuration>();
#endif
#if DEBUG
            services.AddTransient<IConfigurationService, LocalDevelopmentConfiguration>();
#endif
        }
    }
}
