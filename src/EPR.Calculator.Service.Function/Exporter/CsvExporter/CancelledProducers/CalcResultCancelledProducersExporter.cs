namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.IdentityModel.Tokens;

    public class CalcResultCancelledProducersExporter : ICalcResultCancelledProducersExporter
    {
        public void Export(CalcResultCancelledProducersResponse cancelledProducers, StringBuilder csvContent)
        {
            // Add empty lines
            csvContent.AppendLine();
            csvContent.AppendLine();

            // Add headers
            PrepareCancelledProducersHeader(cancelledProducers, csvContent);            
        }

        private static void PrepareCancelledProducersHeader(CalcResultCancelledProducersResponse response, StringBuilder csvContent)
        {
            // Add cancelled producers header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(response.TitleHeader!));            

            // Add sub header
            WriteCancelledProducersSecondaryHeaders(response.CancelledProducers!, csvContent);

            // Add column header
            WriteCancelledProducersColumnHeaders(response.CancelledProducers!, csvContent);
            csvContent.AppendLine();
        }

        private static void WriteCancelledProducersSecondaryHeaders(IEnumerable<CalcResultCancelledProducersDTO> headers, StringBuilder csvContent)
        {
            const int maxColumnSize = CommonConstants.SecondaryHeaderMaxColumnSize;
            var headerRows = new string[maxColumnSize];

            headerRows[CommonConstants.LastTonnageSubHeaderIndex] = CsvSanitiser.SanitiseData(CommonConstants.LastTonnage);
            headerRows[CommonConstants.LatestInvoiceSubHeaderIndex] = CsvSanitiser.SanitiseData(CommonConstants.LatestInvoice);

            var headerRow = string.Join(CommonConstants.CsvFileDelimiter, headerRows);
            csvContent.AppendLine(headerRow);
        }

        private static void WriteCancelledProducersColumnHeaders(IEnumerable<CalcResultCancelledProducersDTO> producers, StringBuilder csvContent)
        {
            var header = producers.FirstOrDefault();
            if (header == null)
            {
                return;
            }
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ProducerId));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.SubsidiaryId));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ProducerOrSubsidiaryName));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.TradingName));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Aluminium));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.FibreComposite));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Glass));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.PaperOrCard));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Plastic));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Steel));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Wood));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.OtherMaterials));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.LastInvoicedTotal));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.RunNumber));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.RunName));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.BillingInstructionId));
        }
    }
}