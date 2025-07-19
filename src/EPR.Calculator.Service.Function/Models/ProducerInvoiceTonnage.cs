using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models
{
    public class ProducerInvoiceTonnage
    {
        public int RunId {  get; set; }
        public int ProducerId { get; set; }
        public int MaterialId { get; set; }
        public decimal? NetTonnage { get; set; }
    }
}
