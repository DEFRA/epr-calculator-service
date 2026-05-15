namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLapcapDataDetail
    {
        public required string Name { get; set; }

        public decimal EnglandCost { get; set; }

        public decimal WalesCost { get; set; }

        public decimal ScotlandCost { get; set; }

        public decimal NorthernIrelandCost { get; set; }

        public decimal TotalCost { get; set; }

        public int OrderId { get; set; } // TODO remove this - for ordering
    }
}
