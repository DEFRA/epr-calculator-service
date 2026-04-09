namespace EPR.Calculator.Service.Function.Models
{
    using System.Collections.Generic;

    public class ProjectedProducersHeader
    {
        required public string Name { get; set; }
        public int? ColumnIndex { get; set; }
    }

    public class ProjectedProducersHeaders {
        public required ProjectedProducersHeader TitleHeader { get; set; }
        public required IEnumerable<ProjectedProducersHeader> MaterialBreakdownHeaders { get; set; }
        public required IEnumerable<ProjectedProducersHeader> ColumnHeaders { get; set; }
    }

    public interface CalcResultProjectedProducer<ProjectedTonnage> where ProjectedTonnage : CalcResultProjectedProducerMaterialTonnage
    {
        public int ProducerId { get; set; }
        public string? SubsidiaryId { get; set; }
        public string Level { get; set; }
        public string SubmissionPeriodCode { get; set; }
        public bool IsSubtotal { get; set; }
        public Dictionary<string, ProjectedTonnage> ProjectedTonnageByMaterial { get; set; }
    }

    public class CalcResultH2ProjectedProducer : CalcResultProjectedProducer<CalcResultH2ProjectedProducerMaterialTonnage>
    {
        public required int ProducerId { get; set; }
        public string? SubsidiaryId { get; set; }
        public required string Level { get; set; }
        public required string SubmissionPeriodCode { get; set; }
        public bool IsSubtotal { get; set; } = false;
        public required Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage> ProjectedTonnageByMaterial { get; set; }
    }

    public class CalcResultProjectedProducers
    {
        public ProjectedProducersHeaders? H2ProjectedProducersHeaders { get; set; }
        public IEnumerable<CalcResultH2ProjectedProducer>? H2ProjectedProducers { get; set; }
    }
}
