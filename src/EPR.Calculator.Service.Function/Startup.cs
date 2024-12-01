// <copyright file="Startup.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Common;
using EPR.Calculator.Service.Common.AzureSynapse;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Calculator.Service.Function
{
    /// Configures the startup for the Azure Functions application.
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            this.RegisterDependencies(builder.Services);
        }

        private void RegisterDependencies(IServiceCollection services)
        {
            services.AddTransient<ICalculatorRunService, CalculatorRunService>();
            services.AddTransient<ICalculatorRunParameterMapper, CalculatorRunParameterMapper>();
            services.AddTransient<IAzureSynapseRunner, AzureSynapseRunner>();
            services.AddTransient<IPipelineClientFactory, PipelineClientFactory>();
        }
    }
}
