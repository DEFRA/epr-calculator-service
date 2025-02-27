namespace EPR.Calculator.Service.Function.Exporter.Lapcap
{
    using System.Text;
    using EPR.Calculator.Service.Function.Models;

    public interface ILapcaptDetailExporter
    {
        void PrepareLapcapData(CalcResultLapcapData calcResultLapcapData, StringBuilder csvContent);
    }
}
