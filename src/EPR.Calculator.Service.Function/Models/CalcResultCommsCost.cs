namespace EPR.Calculator.Service.Function.Models
{
    /// <summary>
    /// The CommsCost report.
    /// </summary>
    public class CalcResultCommsCost
    {
        // TODO do we need this - it's lifted from LAPCAP data section
        public CalcResultCommsCostOnePlusFourApportionment CalcResultCommsCostOnePlusFourApportionment { get; set; }
        public Dictionary<string, CalcResultCommsCostCommsCostByMaterial> CommsCostByMaterial { get; set; }
            = [];
        public CalcResultCommsCostCommsCostByMaterial CommsCostByMaterialTotal { get; set; }

        public ByCountryValue CommsCostUkWide { get; set; }
        public ByCountryValue CommsCostByCountry { get; set; }
    }
}
