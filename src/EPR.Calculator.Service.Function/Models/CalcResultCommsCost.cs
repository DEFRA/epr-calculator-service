namespace EPR.Calculator.Service.Function.Models
{
    /// <summary>
    /// The CommsCost report.
    /// </summary>
    public class CalcResultCommsCost
    {
        public ByCountryApportionment CalcResultCommsCostOnePlusFourApportionment { get; set; }
        public Dictionary<string, CalcResultCommsCostCommsCostByMaterial> CommsCostByMaterial { get; set; }
            = [];
        public CalcResultCommsCostCommsCostByMaterial CommsCostByMaterialTotal { get; set; }

        public ByCountryCost CommsCostUkWide { get; set; }
        public ByCountryCost CommsCostByCountry { get; set; }
    }
}
