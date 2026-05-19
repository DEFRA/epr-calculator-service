namespace EPR.Calculator.Service.Function.Models
{
    public class ByCountryCost
    {
        public decimal England { get; set; }

        public decimal Wales { get; set; }

        public decimal Scotland { get; set; }

        public decimal NorthernIreland { get; set; }

        private decimal? total;
        public decimal Total => total ??= England + Wales + Scotland + NorthernIreland;
    }

    public class ByCountryApportionment
    {
        public decimal England { get; set; }

        public decimal Wales { get; set; }

        public decimal Scotland { get; set; }

        public decimal NorthernIreland { get; set; }

        // TODO This should always be 100%
        private decimal? total;
        public decimal Total => total ??= England + Wales + Scotland + NorthernIreland;
    }
}
