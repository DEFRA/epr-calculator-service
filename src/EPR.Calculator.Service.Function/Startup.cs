using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Common;
using EPR.Calculator.Service.Function;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace EPR.Calculator.Service.Function
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        private CalculatorRunConfiguration configuration;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            this.RegisterDependencies(builder.Services);
        }

        private void RegisterDependencies(IServiceCollection services)
        {
            services.AddTransient<ICalculatorRunService, CalculatorRunService>();
            services.AddTransient<ICalculatorRunParameterMapper, CalculatorRunParameterMapper>();
        }

    }
}
