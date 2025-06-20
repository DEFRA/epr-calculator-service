using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Models
{
    /// <summary>
    /// The CommsCost report.
    /// </summary>
    public class CalcResultCommsCost
    {
        public string Name { get; set; } = string.Empty;
        public IEnumerable<CalcResultCommsCostOnePlusFourApportionment> CalcResultCommsCostOnePlusFourApportionment { get; set; }
            = [];
        public IEnumerable<CalcResultCommsCostCommsCostByMaterial> CalcResultCommsCostCommsCostByMaterial { get; set; }
            = [];

        /// <summary>
        /// Contains records for several different types of comms cost records.
        /// </summary>
        /// <remarks>
        /// Find specific records by cross referencing the constants in <see cref="CalcResultCommsCostBuilder"/>  
        /// with the records' name property.
        /// </remarks>
        public IEnumerable<CalcResultCommsCostOnePlusFourApportionment> CommsCostByCountry { get; set; }
            = [];
    }
}
