using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Converter;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public static class CalcResultDetailJsonMapper
    {
        public static CalcResultDetailJson Map(CalcResultDetail calcResultDetail)
        {
            return new CalcResultDetailJson
            {
                RunName = calcResultDetail.RunName,
                RunId = calcResultDetail.RunId,
                RunDate = calcResultDetail.RunDate.ToString(CalculationResults.DateFormatISO8601),
                RunBy = calcResultDetail.RunBy,
                FinancialYear = calcResultDetail.FinancialYear,
                RpdFileORG = string.Empty,
                RpdFileORGTimeStamp = DateTimeConversion.ConvertToIso8601Utc(calcResultDetail.RpdFileORG),
                RpdFilePOM = string.Empty,
                RpdFilePOMTimeStamp = DateTimeConversion.ConvertToIso8601Utc(calcResultDetail.RpdFilePOM),
                LapcapFile = GetFileInfo(calcResultDetail.LapcapFile)[0],
                LapcapFileTimeStamp = DateTimeConversion.ConvertToIso8601Utc(GetFileInfo(calcResultDetail.LapcapFile)[1]),
                LapcapFileUploader = GetFileInfo(calcResultDetail.LapcapFile)[2],
                ParametersFile = GetFileInfo(calcResultDetail.ParametersFile)[0],
                ParametersFileTimeStamp = DateTimeConversion.ConvertToIso8601Utc(GetFileInfo(calcResultDetail.ParametersFile)[1]),
                ParametersFileUploader = GetFileInfo(calcResultDetail.ParametersFile)[2],
            };
        }

        private static string[] GetFileInfo(string filePath)
        {
            return filePath.Split(CommonConstants.Comma);
        }
    }
}