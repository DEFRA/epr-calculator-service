namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.Extensions.Azure;
    using Microsoft.IdentityModel.Tokens;

    public class CalcResultCancelledProducersExporter : ICalcResultCancelledProducersExporter
    {

        public void Export(CalcResultCancelledProducersResponse calcResultCancelledProducers, StringBuilder csvContent)
        {
            // Add empty lines
            csvContent.AppendLine();
            csvContent.AppendLine();

            // Add headers
            PrepareCancelledProducersHeader(calcResultCancelledProducers, csvContent);
            PrepareCancelledProducersValues(calcResultCancelledProducers, csvContent);
        }

        private static void PrepareCancelledProducersHeader(CalcResultCancelledProducersResponse response, StringBuilder csvContent)
        {
            // Add cancelled producers header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(response.TitleHeader!));

            // Add sub header
            WriteCancelledProducersSecondaryHeaders(csvContent);

            // Add column header
            WriteCancelledProducersColumnHeaders(response.CancelledProducers!, csvContent);
            csvContent.AppendLine();
        }

        private static void PrepareCancelledProducersValues(CalcResultCancelledProducersResponse response, StringBuilder csvContent)
        {
            foreach (var CancelledProducer in response.CancelledProducers)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.ProducerIdValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.ProducerOrSubsidiaryNameValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.TradingNameValue)); 
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.AluminiumValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.FibreCompositeValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.GlassValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.PaperOrCardValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.PlasticValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.SteelValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.WoodValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.OtherMaterialsValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LatestInvoice?.CurrentYearInvoicedTotalToDateValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LatestInvoice?.RunNumberValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LatestInvoice?.RunNameValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LatestInvoice?.BillingInstructionIdValue));
                csvContent.AppendLine();
            }

        }


        private static void WriteCancelledProducersSecondaryHeaders(StringBuilder csvContent)
        {
            const int maxColumnSize = CommonConstants.SecondaryHeaderMaxColumnSize;
            var headerRows = new string[maxColumnSize];

            headerRows[CommonConstants.LastTonnageSubHeaderIndex] = CsvSanitiser.SanitiseData(CommonConstants.LastTonnage);
            headerRows[CommonConstants.LatestInvoiceSubHeaderIndex] = CsvSanitiser.SanitiseData(CommonConstants.LatestInvoice);

            var headerRow = string.Join(CommonConstants.CsvFileDelimiter, headerRows);
            csvContent.AppendLine(headerRow);
        }

        private static void WriteCancelledProducersColumnHeaders(IEnumerable<CalcResultCancelledProducersDto> producers, StringBuilder csvContent)
        {
            if (!producers.Any())
            {
                return;
            }
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ProducerId));
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