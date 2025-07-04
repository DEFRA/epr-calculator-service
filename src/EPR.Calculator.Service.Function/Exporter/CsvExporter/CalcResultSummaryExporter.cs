namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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

            AddProducerFee(csvContent, producer);

            AddProducerFeeForComms(csvContent, producer);

            csvContent.Append(CsvSanitiser.SanitiseData(producer.PercentageofProducerReportedTonnagevsAllProducers, DecimalPlaces.Eight, null, false, true));

            AddTotalProducerFee(csvContent, producer);

            // 2c comms Total
            AddTwoC(csvContent, producer);

            // Total bill 1 + 2a + 2b + 2c
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerTotalOnePlus2A2B2CWithBadDeptProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerOverallPercentageOfCostsForOnePlus2A2B2C, DecimalPlaces.Eight, null, false, true));

            AddSchemeAdministratorOperatingCostsSection(csvContent, producer);

            // LA data prep costs section 4
            AddLocalAuthorityDataPreparationCosts(csvContent, producer);

            AddOneOffSchemeAdministrationSetupCosts(csvContent, producer);

            // Total bill section
            AddTotalSection(csvContent, producer);

            // Billing instructions section
            AddBillingInstructionsSection(csvContent, producer);

            csvContent.AppendLine();
        }

        public void AddTotalSection(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerBillBreakdownSection.TotalProducerFeeWithoutBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerBillBreakdownSection.BadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerBillBreakdownSection.TotalProducerFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerBillBreakdownSection.EnglandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerBillBreakdownSection.WalesTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerBillBreakdownSection.ScotlandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerBillBreakdownSection.NorthernIrelandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
        }

        public void AddOneOffSchemeAdministrationSetupCosts(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.OneOffSchemeAdministrationSetupCosts!.TotalProducerFeeWithoutBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.OneOffSchemeAdministrationSetupCosts.BadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.OneOffSchemeAdministrationSetupCosts.TotalProducerFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.OneOffSchemeAdministrationSetupCosts.EnglandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.OneOffSchemeAdministrationSetupCosts.WalesTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.OneOffSchemeAdministrationSetupCosts.ScotlandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.OneOffSchemeAdministrationSetupCosts.NorthernIrelandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
        }

        public void AddBillingInstructionsSection(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.CurrentYearInvoiceTotalToDate, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.TonnageChangeSinceLastInvoice));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.LiabilityDifference, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.MaterialThresholdBreached));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.TonnageThresholdBreached));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.PercentageLiabilityDifference, DecimalPlaces.Two, null, false, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.MaterialPercentageThresholdBreached));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.TonnagePercentageThresholdBreached));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.SuggestedBillingInstruction));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BillingInstructionSection.SuggestedInvoiceAmount, DecimalPlaces.Two, null, true));
        }

        public void AddLocalAuthorityDataPreparationCosts(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.LocalAuthorityDataPreparationCosts.TotalProducerFeeWithoutBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.LocalAuthorityDataPreparationCosts.BadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.LocalAuthorityDataPreparationCosts.TotalProducerFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.LocalAuthorityDataPreparationCosts.EnglandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.LocalAuthorityDataPreparationCosts.WalesTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.LocalAuthorityDataPreparationCosts.ScotlandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.LocalAuthorityDataPreparationCosts.NorthernIrelandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
        }

        public void AddSchemeAdministratorOperatingCostsSection(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.SchemeAdministratorOperatingCostsSection.TotalProducerFeeWithoutBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.SchemeAdministratorOperatingCostsSection.BadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.SchemeAdministratorOperatingCostsSection.TotalProducerFeeWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.SchemeAdministratorOperatingCostsSection.EnglandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.SchemeAdministratorOperatingCostsSection.WalesTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.SchemeAdministratorOperatingCostsSection.ScotlandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.SchemeAdministratorOperatingCostsSection.NorthernIrelandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
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

        public void AddTotalProducerFee(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeWithoutBadDebtFor2bComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionFor2bComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtFor2bComms, DecimalPlaces.Two, null, true));
        }

        public void AddProducerFeeForComms(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionFor2A, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtProvision2A, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtProvision2A, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtProvision2A, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtProvision2A, DecimalPlaces.Two, null, true));
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

        public void AddProducerFee(StringBuilder csvContent, CalcResultSummaryProducerDisposalFees producer)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeforLADisposalCostswoBadDebtprovision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.BadDebtProvisionFor1, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TotalProducerFeeforLADisposalCostswithBadDebtprovision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.EnglandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.WalesTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.ScotlandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.NorthernIrelandTotalWithBadDebtProvision, DecimalPlaces.Two, null, true));
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
            csvContent.Append(CsvSanitiser.SanitiseData(producer.Level == ((int)CalcResultSummaryLevelIndex.One).ToString() ? "0" : producer.TonnageChangeCount));
            csvContent.Append(CsvSanitiser.SanitiseData(producer.TonnageChangeAdvice));
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
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.PreviousInvoicedTonnage, DecimalPlaces.Zero, DecimalFormats.F2));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.PublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                if (extraColumns.Contains(disposalFee.Key))
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdDrinksContainersTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.ManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.NetReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(disposalFee.Value.TonnageChange, DecimalPlaces.Zero, DecimalFormats.F2));

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
            if(producer.ProducerCommsFeesByMaterial == null) { return; }
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
    }
}
