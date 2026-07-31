using EPR.Calculator.API.Data.DataTypes;

namespace EPR.Calculator.Service.Function.Models;

public record CalcResultDetail
{
    public required string RunName { get; init; }
    public required int RunId { get; init; }
    public required RelativeYear RelativeYear { get; init; }
    public required DateTime? CutOffDate { get; init; }
    public required DateTime RunDate { get; init; }
    public required string RunBy { get; init; }
    public required string RpdFileORG { get; init; }
    public required string RpdFilePOM { get; init; }
    public required string LapcapFile { get; init; }
    public required string ParametersFile { get; init; }
    public required string CountryApportionmentFile { get; init; }
}
