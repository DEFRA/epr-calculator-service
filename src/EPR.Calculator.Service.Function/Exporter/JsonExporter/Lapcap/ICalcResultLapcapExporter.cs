using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.Lapcap
{
    /// <summary>
    /// Interface for <see cref="CalcResultLapcapExporter"/> to allow it to be injected as a service.
    /// </summary>
    public interface ICalcResultLapcapExporter
    {
        /// <summary>
        /// Convert the data.
        /// </summary>
        /// <param name="data">The data to convert.</param>
        /// <returns>The data in JSON string format.</returns>
        object Export(CalcResultLapcapData data);
    }
}