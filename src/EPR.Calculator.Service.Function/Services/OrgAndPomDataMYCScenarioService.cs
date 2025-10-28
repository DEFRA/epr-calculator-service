using EPR.Calculator.API.Data.DataModels;
using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Services
{
    public class OrgAndPomDataMYCScenarioService : IOrgAndPomDataMYCScenarioService
    {
        private readonly IEnumerable<ProducerDetail> producers;
        private readonly IEnumerable<CalculatorRunPomDataDetail> pomData;

        public OrgAndPomDataMYCScenarioService(
            IEnumerable<ProducerDetail> producers,
            IEnumerable<CalculatorRunPomDataDetail> pomData)
        {
            this.producers = producers;
            this.pomData = pomData;
        }

        public IEnumerable<ProducerReportedMaterial> GetProducerReportedMaterials()
        {
            var producerReportedMaterials = new List<ProducerReportedMaterial>();

            foreach (var producer in this.producers)
            {
                // TODO: Implementation to populate producerReportedMaterials based on the scenarios
                switch (producer.statusCode)
                {
                    case "01":
                        break;
                }
            }

            return producerReportedMaterials;
        }
    }
}
