using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A
{
    public interface ICalcResultCommsCostOnePlusFourApportionmentExporter
    {
        object? ConvertToJsonByUKWide(CalcResultCommsCost data);

        object? ConvertToJsonByCountry(CalcResultCommsCost data);
    }
}