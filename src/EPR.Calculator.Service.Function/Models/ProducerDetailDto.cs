using EPR.Calculator.API.Data.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models
{
    public class ProducerDetailDto
    {
        public int Id { get; set; }

        public int ProducerId { get; set; }

        public string? TradingName { get; set; }

        public string? SubsidiaryId { get; set; }

        public string? ProducerName { get; set; }

        public int CalculatorRunId { get; set; }

        public ICollection<ProducerReportedMaterial> ProducerReportedMaterials { get; } = new List<ProducerReportedMaterial>();
    }
}
