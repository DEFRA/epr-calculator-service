using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CommsCostByMaterial2A
{
    public interface ICalcResultCommsCostOnePlusFourApportionmentExporter
    {
        string ConvertToJsonByUKWide(CalcResultCommsCost data);

        string ConvertToJsonByCountry(CalcResultCommsCost data);
    }
}