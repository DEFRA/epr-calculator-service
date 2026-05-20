namespace EPR.Calculator.Service.Function.Models
{
    public class  ByCountryCost
    {
        public required decimal England { get; init; }

        public required decimal Wales { get; init; }

        public required decimal Scotland { get; init; }

        public required decimal NorthernIreland { get; init; }

        private decimal? total;
        public decimal Total => total ??= England + Wales + Scotland + NorthernIreland;

        public static readonly ByCountryCost Empty =
            new(){ England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 };
    }

    public class ByCountryApportionment
    {
        public required decimal England { get; init; }

        public required decimal Wales { get; init; }

        public required decimal Scotland { get; init; }

        public required decimal NorthernIreland { get; init; }

        // TODO This should always be 100%
        private decimal? total;
        public decimal Total => total ??= England + Wales + Scotland + NorthernIreland;

        public static readonly ByCountryApportionment Empty =
            new(){ England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 };
    }
}
