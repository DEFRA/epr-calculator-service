namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Common.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;

    public class CalcResultSummaryExporter : ICalcResultSummaryExporter
    {
        private readonly IEnumerable<string> extraColumns = [MaterialCodes.Glass];

        public void Export(CalcResultSummary resultSummary, StringBuilder csvContent)
        {
            // Add empty lines
            csvContent.AppendLine();
            csvContent.AppendLine();

            // Add headers
            PrepareSummaryDataHeader(resultSummary, csvContent);

            // Add data
            foreach (var producer in resultSummary.ProducerDisposalFees)
            {
                AddNewRow(csvContent, producer);
            }
        }

        public void AddNewRow(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            AddFirstFiveColumns(csvContent, producer);

            AppendProducerDisposalFeesByMaterial(csvContent, producer);

            AddProducerDisposal(csvContent, producer);

            AppendProducerCommsFeesByMaterial(csvContent, producer);

            AddProducerCommsFee(csvContent, producer);

            // 1 Local authority disposal costs section
            AddSectionContent(csvContent, producer.LocalAuthorityDisposalCostsSectionOne);

            // 2a comms costs section
            AddSectionContent(csvContent, producer.CommunicationCostsSectionTwoA);

            csvContent.Append(CsvSanitiser.SanitiseData(producer.PercentageofProducerReportedTonnagevsAllProducers, DecimalPlaces.Eight, null, false, true));

            // 2b comms costs section
            AddSectionContent(csvContent, producer.CommunicationCostsSectionTwoB);

            // 2c comms costs section
            AddTwoC(csvContent, producer);

            // Total bill 1 + 2a + 2b + 2c
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerTotalOnePlus2A2B2CWithBadDeptProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, DecimalPlaces.Eight, null, false, true));

            AddSectionContent(csvContent, producer.SchemeAdministratorOperatingCosts);

            // Local authority data preparation costs section 4
            AddSectionContent(csvContent, producer.LocalAuthorityDataPreparationCosts);

            // One-off scheme administration setup costs section 5
            AddSectionContent(csvContent, producer.OneOffSchemeAdministrationSetupCosts);

            // Total producer bill breakdown section
            AddSectionContent(csvContent, producer.TotalProducerBillBreakdownCosts);

            // Billing instructions section
            AddBillingInstructionsSection(csvContent, producer);

            csvContent.AppendLine();
        }

        public void AddBillingInstructionsSection(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(CurrencyConverter.FormattedCurrencyValue(producer.BillingInstructionSection!.CurrentYearInvoiceTotalToDate), DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.TonnageChangeSinceLastInvoice ?? CommonConstants.Hyphen));
            csvContent.Append(CsvSanitiser.SanitiseData(CurrencyConverter.FormattedCurrencyValue(producer.BillingInstructionSection.LiabilityDifference), DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.MaterialThresholdBreached, appendLrmCharacterToPreventRenderedAsFormula: true)); // prefixed with LRM character as the values(+ve and -ve) are parsed as formula in csv
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.TonnageThresholdBreached, appendLrmCharacterToPreventRenderedAsFormula: true)); // prefixed with LRM character added as values(+ve and -ve) are parsed as formula in csv
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.PercentageLiabilityDifference, DecimalPlaces.Two, null, false, true));
            csvContent.Append(CsvSanitiser.SanitiseData("\u200E" + producer.BillingInstructionSection.MaterialPercentageThresholdBreached));
            csvContent.Append(CsvSanitiser.SanitiseData("\u200E" + producer.BillingInstructionSection.TonnagePercentageThresholdBreached));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.SuggestedBillingInstruction));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.SuggestedInvoiceAmount, DecimalPlaces.Two, null, true));
        }

        public void AddTwoC(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCTotalProducerFeeForCommsCostsWithoutBadDebt, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCTotalProducerFeeForCommsCostsWithBadDebt, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCEnglandTotalWithBadDebt, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCWalesTotalWithBadDebt, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCScotlandTotalWithBadDebt, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TwoCNorthernIrelandTotalWithBadDebt, DecimalPlaces.Two, null, true));
        }

        public void AddProducerCommsFee(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerCommsFee, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerCommsFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalComms, DecimalPlaces.Two, null, true));
        }

        public void AddProducerDisposal(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerDisposalFee, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerDisposalFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotal, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotal, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotal, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotal, DecimalPlaces.Two, null, true));
            AppendCsvValue(csvContent, producer.TonnageChangeCount, producer.isOverallTotalRow);
            AppendCsvValue(csvContent, producer.TonnageChangeAdvice, producer.isOverallTotalRow);
        }

        public void AddFirstFiveColumns(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerName));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TradingName));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.IsProducerScaledup));
        }

        private void AppendProducerDisposalFeesByMaterial(
            StringBuilder csvContent,
            CalcResultSummaryProducerDisposalFees producer)
        {
            if (producer.ProducerCommsFeesByMaterial == null) { return; }
            foreach (var disposalFee in producer.ProducerDisposalFeesByMaterial!)
            {
                if (!producer.isOverallTotalRow && (producer.Level != "1" || disposalFee.Value.PreviousInvoicedTonnage == null))
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Hyphen));
                }
                else
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.PreviousInvoicedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                }
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.PublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                if (extraColumns.Contains(disposalFee.Key))
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdDrinksContainersTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.NetReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                AppendCsvValue(csvContent, disposalFee.Value.TonnageChange, producer.isOverallTotalRow);
                csvContent.Append(producer.IsProducerScaledup != CommonConstants.Totals ? CsvSanitiser.SanitiseData(disposalFee.Value.PricePerTonne, null, null, true) : CommonConstants.CsvFileDelimiter);
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ProducerDisposalFee, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.BadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ProducerDisposalFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.EnglandWithBadDebtProvision, DecimalPlaces.Two, DecimalFormats.F2, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.WalesWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ScotlandWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.NorthernIrelandWithBadDebtProvision, DecimalPlaces.Two, null, true));
            }
        }

        private void AppendProducerCommsFeesByMaterial(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            if (producer.ProducerCommsFeesByMaterial == null) { return; }
            foreach (var disposalFee in producer.ProducerCommsFeesByMaterial!)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ReportedPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                if (extraColumns.Contains(disposalFee.Key))
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdDrinksContainers, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(producer.IsProducerScaledup != CommonConstants.Totals ? CsvSanitiser.SanitiseData(disposalFee.Value.PriceperTonne, null, null, true) : CommonConstants.CsvFileDelimiter);
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ProducerTotalCostWithoutBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.BadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ProducerTotalCostwithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.EnglandWithBadDebtProvision, DecimalPlaces.Two, DecimalFormats.F2, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.WalesWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ScotlandWithBadDebtProvision, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.NorthernIrelandWithBadDebtProvision, DecimalPlaces.Two, null, true));
            }
        }

        public void WriteSecondaryHeaders(StringBuilder csvContent, IEnumerable<CalcResultSummaryHeader> headers)
        {
            const int maxColumnSize = CommonConstants.SecondaryHeaderMaxColumnSize;
            var headerRows = new string[maxColumnSize];
            foreach (var item in headers)
            {
                if (item.ColumnIndex.HasValue)
                {
                    headerRows[item.ColumnIndex.Value - 1] = CsvSanitiser.SanitiseData(item.Name, false);
                }
            }

            var headerRow = string.Join(CommonConstants.CsvFileDelimiter, headerRows);
            csvContent.AppendLine(headerRow);
        }

        public void WriteColumnHeaders(CalcResultSummary resultSummary, StringBuilder csvContent)
        {
            foreach (var item in resultSummary.ColumnHeaders)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(item.Name));
            }
        }

        private void PrepareSummaryDataHeader(CalcResultSummary resultSummary, StringBuilder csvContent)
        {
            // Add result summary header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(resultSummary.ResultSummaryHeader?.Name))
                .AppendLine()
                .AppendLine();

            // Add notes header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(resultSummary.NotesHeader?.Name));

            // Add producer disposal fees header
            WriteSecondaryHeaders(csvContent, resultSummary.ProducerDisposalFeesHeaders);

            // Add material breakdown header
            WriteSecondaryHeaders(csvContent, resultSummary.MaterialBreakdownHeaders);

            // Add column header
            WriteColumnHeaders(resultSummary, csvContent);

            csvContent.AppendLine();
        }

        private void AddSectionContent(StringBuilder csvContent, CalcResultSummaryBadDebtProvision costs)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(costs.TotalProducerFeeWithoutBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(costs.BadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(costs.TotalProducerFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(costs.EnglandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(costs.WalesTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(costs.ScotlandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(costs.NorthernIrelandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
        }

        private void AppendCsvValue(StringBuilder csvContent, object? value, bool isOverallTotalRow = false,
                                    DecimalPlaces decimalPlaces = DecimalPlaces.Zero,
                                    DecimalFormats decimalFormat = DecimalFormats.F2)
        {
            if (value == null && !isOverallTotalRow)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Hyphen));
            }
            else if (value is int or decimal or double)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(value, decimalPlaces, decimalFormat));
            }
            else
            {
                csvContent.Append(CsvSanitiser.SanitiseData(value));
            }
        }

    }
}
