namespace EPR.Calculator.Service.Function.Models
{
    public record CalcResultOnePlusFourApportionmentDetail
    {
        public required string Name { get; set; }
        public required decimal EnglandTotal { get; set; }
        public required decimal WalesTotal { get; set; }
        public required decimal ScotlandTotal { get; set; }
        public required decimal NorthernIrelandTotal { get; set; }
        public decimal Total { get; set; }
        public int OrderId { get; set; }
    }
}
