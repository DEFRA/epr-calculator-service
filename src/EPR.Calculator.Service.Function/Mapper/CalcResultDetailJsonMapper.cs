using EPR.Calculator.Service.Function.Constants;
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
            return filePath.Split(CommonConstants.Comma);
        }
    }
}