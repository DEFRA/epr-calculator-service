namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    public interface IBillingFileExporter<T>
    {
        string Export(T results, ImmutableHashSet<int> acceptedProducerIds);
    }
}
