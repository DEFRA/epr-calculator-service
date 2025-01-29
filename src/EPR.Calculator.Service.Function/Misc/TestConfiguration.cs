using EPR.Calculator.Service.Function.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Misc
{
    public class TestConfiguration : IConfigurationService
    {
        public string CheckInterval => throw new NotImplementedException();
        public string DbConnectionString => "Data Source=.;Initial Catalog=PayCal;Integrated Security=True;TrustServerCertificate=True";
        public string ExecuteRPDPipeline => string.Empty;
        public string MaxCheckCount => throw new NotImplementedException();
        public string OrgDataPipelineName => throw new NotImplementedException();
        public string PipelineUrl => throw new NotImplementedException();
        public string PomDataPipelineName => throw new NotImplementedException();
        public Uri PrepareCalcResultEndPoint => new Uri("http://localhost:5055/v1/internal/prepareCalcResults");
        public TimeSpan PrepareCalcResultsTimeout => TimeSpan.FromDays(1);
        public TimeSpan RpdStatusTimeout => TimeSpan.FromDays(1);
        public Uri StatusEndpoint => new Uri("http://localhost:5055/v1/internal/rpdStatus");
        public Uri TransposeEndpoint => throw new NotImplementedException();
        public TimeSpan TransposeTimeout => TimeSpan.FromDays(1);
    }
}
