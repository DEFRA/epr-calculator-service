using EPR.Calculator.Service.Common;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using EPR.Calculator.Service.Function;

[assembly: FunctionsStartup(typeof(Startup))]
namespace EPR.Calculator.Service.Function
{
    public class Startup : FunctionsStartup
    {
        private CalculatorRunConfiguration _configuration;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            RegisterDependencies(builder.Services);
        }

        private void RegisterDependencies(IServiceCollection services)
        {
            services.AddTransient<ICalculatorRunService, CalculatorRunService>();
        }

    }
}
