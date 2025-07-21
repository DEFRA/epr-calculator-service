using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models
{
    public record ProducerInvoiceTonnage
    {
        public int RunId {  get; init; }
        public int ProducerId { get; init; }
        public int MaterialId { get; init; }
        public decimal? NetTonnage { get; init; }
    }
}
