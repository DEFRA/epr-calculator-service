namespace EPR.Calculator.Service.Function.Exporter
{
    using System.Text;
    using EPR.Calculator.Service.Function.Models;

    public interface ILapcaptDetailExporter
    {
        void Export(CalcResultLapcapData calcResultLapcapData, StringBuilder csvContent);
    }
}
