namespace EPR.Calculator.Service.Function.Interface
{
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Misc;

    public interface IOrgAndPomDataMYCPreValidationService
    {
        Task OrgAndPomDataMYCPreValidation(int calculatorRunId, string? runName, CancellationToken cancellationToken);
    }
}
