namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultParameterOtherCostDetail
    {
        public string? Name { get; set; }
        public required decimal England { get; set; }
        public required decimal Wales { get; set; }
        public required decimal Scotland { get; set; }
        public required decimal NorthernIreland { get; set; }
        public required decimal Total { get; set; }
        public int OrderId { get; set; }
    }
}
