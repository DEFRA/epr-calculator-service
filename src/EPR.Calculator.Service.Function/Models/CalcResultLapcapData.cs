using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLapcapData
    {
        public required Dictionary<string, ByCountryCost> ByMaterial { get; set; }

        public required ByCountryCost Total { get; set; }

        public required ByCountryApportionment CountryApportionment { get; set; }
    }
}
