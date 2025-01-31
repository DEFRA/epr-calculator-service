namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultParameterCommunicationCostDetail
    {
        public required string Material { get; set; }
        public required decimal Cost { get; set; }
        public required string Country { get; set; }
        public bool IsApportionment { get; set; }
        public bool IsPercentage { get; set; }
    }
}
