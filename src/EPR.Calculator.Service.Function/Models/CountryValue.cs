namespace EPR.Calculator.Service.Function.Models
{
    public record ByCountryCost
    {
        public required decimal England { get; init; }

        public required decimal Wales { get; init; }

        public required decimal Scotland { get; init; }

        public required decimal NorthernIreland { get; init; }

        public decimal Total => England + Wales + Scotland + NorthernIreland;

        public static readonly ByCountryCost Empty =
            new(){ England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 };

        public static ByCountryCost operator +(ByCountryCost a, ByCountryCost b) =>
            new()
            {
                England         = a.England         + b.England,
                Wales           = a.Wales           + b.Wales,
                Scotland        = a.Scotland        + b.Scotland,
                NorthernIreland = a.NorthernIreland + b.NorthernIreland,
            };

        public static ByCountryCost Sum(IReadOnlyCollection<ByCountryCost> costs)
        {
            return new ByCountryCost
            {
                England          = costs.Sum(x => x.England),
                Wales            = costs.Sum(x => x.Wales),
                NorthernIreland  = costs.Sum(x => x.NorthernIreland),
                Scotland         = costs.Sum(x => x.Scotland),
            };
        }
    }

    public record ByCountryApportionment
    {
        public required decimal England { get; init; }

        public required decimal Wales { get; init; }

        public required decimal Scotland { get; init; }

        public required decimal NorthernIreland { get; init; }

        public decimal Total => England + Wales + Scotland + NorthernIreland;

        public static readonly ByCountryApportionment Empty =
            new(){ England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 };
    }
}
