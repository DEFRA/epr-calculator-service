namespace EPR.Calculator.Service.Function.Models
{
    /// <summary>
    /// The CommsCost report.
    /// </summary>
    public class CalcResultCommsCost
    {
        public CalcResultCommsCostOnePlusFourApportionment CalcResultCommsCostOnePlusFourApportionment { get; set; }
        public IEnumerable<CalcResultCommsCostCommsCostByMaterial> CalcResultCommsCostCommsCostByMaterial { get; set; }
            = [];

        public CalcResultCommsCostOnePlusFourApportionment CommsCostUkWide { get; set; }
        public CalcResultCommsCostOnePlusFourApportionment CommsCostByCountry { get; set; }
    }
}
