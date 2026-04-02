namespace EPR.Calculator.Service.Function.Exporter
{
    public interface ICalcResultsExporter<in T>
    {
        string Export(T calcResult);
    }
}
