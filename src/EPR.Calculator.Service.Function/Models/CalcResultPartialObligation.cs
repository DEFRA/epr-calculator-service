namespace EPR.Calculator.Service.Function.Models;

public class CalcResultPartialObligation
{
    public int ProducerId { get; set; }

    public string? SubsidiaryId { get; set; }

    public string? ProducerName { get; set; }

    public string? TradingName { get; set; }

    public string? Level { get; set; }

    public string? SubmissionYear { get; set; }

    public int DaysInSubmissionYear { get; set; }

    public string? JoiningDate { get; set; }

    public int? DaysObligated { get; set; }

    public string? ObligatedPercentage { get; set; }

    public Dictionary<string, CalcResultPartialObligationTonnage> PartialObligationTonnageByMaterial { get; set; } = new();
}