using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exporter.JsonExporter.Model;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Mappers
{
    public class CalcResultDetailJsonMapper
    {
        public static CalcResultDetailJson Map(CalcResultDetail calcResultDetail)
        {
            return new CalcResultDetailJson
            {
                RunName = calcResultDetail.RunName,
                RunId = calcResultDetail.RunId,
                RunDate = calcResultDetail.RunDate.ToString(CalculationResults.DateFormat),
                RunBy = calcResultDetail.RunBy,
                FinancialYear = calcResultDetail.FinancialYear,
                RpdFileORG = string.Empty,
                RpdFileORGTimeStamp = calcResultDetail.RpdFileORG,
                RpdFilePOM = string.Empty,
                RpdFilePOMTimeStamp = calcResultDetail.RpdFilePOM,
                LapcapFile = GetFileInfo(calcResultDetail.LapcapFile)[0],
                LapcapFileTimeStamp = GetFileInfo(calcResultDetail.LapcapFile)[1],
                LapcapFileUploader = GetFileInfo(calcResultDetail.LapcapFile)[2],
                ParametersFile = GetFileInfo(calcResultDetail.ParametersFile)[0],
                ParametersFileTimeStamp = GetFileInfo(calcResultDetail.ParametersFile)[1],
                ParametersFileUploader = GetFileInfo(calcResultDetail.ParametersFile)[2],
            };
        }

        private static string[] GetFileInfo(string filePath)
        {
            return filePath.Split(',');
        }
    }
}