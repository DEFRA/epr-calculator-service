using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultOrganisationDetail
    {
        public int? OrganisationId { get; set; }

        public required string OrganisationName { get; set; }

        public string? TradingName { get; set; }

        public string? SubmissionPeriod { get; set; }

        public string? SubmissionPeriodDescription { get; set; }

        public string? SubsidaryId { get; set; }
    }
}
