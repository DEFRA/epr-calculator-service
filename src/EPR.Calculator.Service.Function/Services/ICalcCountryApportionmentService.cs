using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public interface ICalcCountryApportionmentService
    {
        Task SaveChangesAsync(CalcCountryApportionmentServiceDto countryApportionmentServiceDto);
    }
}
