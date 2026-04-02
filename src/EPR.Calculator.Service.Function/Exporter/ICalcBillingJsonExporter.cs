namespace EPR.Calculator.Service.Function.Exporter
{
    public interface ICalcBillingJsonExporter<T>
    {
        string Export(T results, IEnumerable<int> acceptedProducerIds);
    }
}
