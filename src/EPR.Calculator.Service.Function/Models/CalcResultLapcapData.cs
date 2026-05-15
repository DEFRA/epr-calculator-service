using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLapcapData
    {
        public required IEnumerable<CalcResultLapcapDataDetail> CalcResultLapcapDataDetails { get; set; }

        public required CountryApportionmentData CountryApportionment { get; set; }
    }

    public class CountryApportionmentData
    {
        public decimal England { get; set; }

        public decimal Wales { get; set; }

        public decimal Scotland { get; set; }

        public decimal NorthernIreland { get; set; }
    }
}
