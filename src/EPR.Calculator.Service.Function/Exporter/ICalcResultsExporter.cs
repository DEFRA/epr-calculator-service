namespace EPR.Calculator.API.Exporter
{
    public interface ICalcResultsExporter<in T>
    {
        string Export(T results);
    }
}
