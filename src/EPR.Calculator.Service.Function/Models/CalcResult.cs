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
            ByMaterial = []
        };

        public CalcResultCommsCost CalcResultCommsCostReportDetail { get; set; } = new();

        public required CalcResultLateReportingTonnage CalcResultLateReportingTonnageData { get; set; } =
            new()
            {
                LateReportingTonnageByMaterial = []
            };

        public CalcResultParameterCommunicationCost CalcResultParameterCommunicationCost { get; set; }
            = new()
            {
                Name = string.Empty
            };

        public required CalcResultParameterOtherCost CalcResultParameterOtherCost { get; set; } =
            new()
            {
                SchemeSetupCost  = new ByCountryCost { England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 }
            };

        public CalcResultOnePlusFourApportionment CalcResultOnePlusFourApportionment { get; set; }
            = new();

        public CalcResultLaDisposalCostData CalcResultLaDisposalCostData { get; set; }
            = new()
            {
                ByMaterial = []
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
