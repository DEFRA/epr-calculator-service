using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    public interface IBillingFileExporter<T>
    {
        string Export(T results, IEnumerable<int> acceptedOrganisations);
    }
}
