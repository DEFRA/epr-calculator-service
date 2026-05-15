using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResult
    {
        public required bool ApplyModulation {get; set;}
        public required CalcResultDetail CalcResultDetail { get; set; }

        public required CalcResultLapcapData CalcResultLapcapData { get; set; } = new()
        {
            CalcResultLapcapDataDetails = [],
            CountryApportionment = new CountryApportionmentData()
        };

        public CalcResultCommsCost CalcResultCommsCostReportDetail { get; set; } = new();

        public required CalcResultLateReportingTonnage CalcResultLateReportingTonnageData { get; set; } =
            new()
            {
                Name = string.Empty,
                CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>(),
                MaterialHeading = string.Empty,
                TonnageHeading = string.Empty
            };

        public CalcResultParameterCommunicationCost CalcResultParameterCommunicationCost { get; set; }
            = new()
            {
                Name = string.Empty
            };

        public required CalcResultParameterOtherCost CalcResultParameterOtherCost { get; set; } =
            new()
            {
                BadDebtProvision = new KeyValuePair<string, string>(),
                Name = string.Empty,
                Details = new List<CalcResultParameterOtherCostDetail>(),
                Materiality = new List<CalcResultMateriality>(),
                SaOperatingCost = new List<CalcResultParameterOtherCostDetail>(),
                SchemeSetupCost = new CalcResultParameterOtherCostDetail{
                    England = 0m,
                    Wales = 0m,
                    Scotland = 0m,
                    NorthernIreland = 0m,
                    Total = 0m
                }
            };

        public CalcResultOnePlusFourApportionment CalcResultOnePlusFourApportionment { get; set; }
            = new();

        public CalcResultLaDisposalCostData CalcResultLaDisposalCostData { get; set; }
            = new()
            {
                Name = string.Empty, // TODO remove
                CalcResultLaDisposalCostDetails = []
            };

        public required CalcResultPartialObligations CalcResultPartialObligations { get; set; }

        public required CalcResultProjectedProducers CalcResultProjectedProducers { get; set; }

        public required CalcResultScaledupProducers CalcResultScaledupProducers { get; set; }

        public CalcResultCancelledProducersResponse CalcResultCancelledProducers { get; set; }
            = new()
            {
                TitleHeader = string.Empty,
                CancelledProducers = []
            };

        public IEnumerable<CalcResultRejectedProducer> CalcResultRejectedProducers { get; set; } = [];

        public CalcResultSummary CalcResultSummary { get; set; } = new();

        public IEnumerable<CalcResultErrorReport> CalcResultErrorReports { get; set; } = [];

        public SelfManagedConsumerWaste? Smcw { get; set; }

        public ModulationResult? CalcResultModulation { get; set; }
    }
}
