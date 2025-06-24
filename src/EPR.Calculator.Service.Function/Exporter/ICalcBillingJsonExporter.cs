using System.Collections.Generic;

namespace EPR.Calculator.API.Exporter
{
    public interface ICalcBillingJsonExporter<T>
    {
        string Export(T results, IEnumerable<int> acceptedProducerIds);
    }
}
