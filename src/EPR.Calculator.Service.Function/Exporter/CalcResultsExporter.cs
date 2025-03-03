namespace EPR.Calculator.API.Exporter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Exporter;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.IdentityModel.Tokens;

    public class CalcResultsExporter : ICalcResultsExporter<CalcResult>
    {
        private readonly ICalcResultSummaryExporter calcResultSummaryExporter;
        private readonly ICalcResultDetailExporter resultDetailexporter;
        private readonly IOnePlusFourApportionmentExporter onePlusFourApportionmentExporter;

        public CalcResultsExporter(ICalcResultDetailExporter resultDetailexporter,
                                    IOnePlusFourApportionmentExporter onePlusFourApportionmentExporter,
                                    ICalcResultSummaryExporter calcResultSummaryExporter)
        {
            this.resultDetailexporter = resultDetailexporter;
            this.onePlusFourApportionmentExporter = onePlusFourApportionmentExporter;
            this.calcResultSummaryExporter = calcResultSummaryExporter;
        }

        public string Export(CalcResult results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results), "The results parameter cannot be null.");
            }

            var csvContent = new StringBuilder();
            this.resultDetailexporter.Export(results.CalcResultDetail, csvContent);
            if (results.CalcResultLapcapData != null)
            {
                PrepareLapcapData(results.CalcResultLapcapData, csvContent);
            }

            if (results.CalcResultLateReportingTonnageData != null)
            {
                PrepareLateReportingData(results.CalcResultLateReportingTonnageData, csvContent);
            }

            if (results.CalcResultParameterOtherCost != null)
            {
                PrepareOtherCosts(results.CalcResultParameterOtherCost, csvContent);
            }

            this.onePlusFourApportionmentExporter.Export(results.CalcResultOnePlusFourApportionment, csvContent);

            if (results.CalcResultCommsCostReportDetail != null)
            {
                PrepareCommsCost(results.CalcResultCommsCostReportDetail, csvContent);
            }

            if (results.CalcResultLaDisposalCostData != null)
            {
                PrepareLaDisposalCostData(results.CalcResultLaDisposalCostData, csvContent);
            }

            if (results.CalcResultScaledupProducers != null)
            {
                PrepareScaledupProducers(results.CalcResultScaledupProducers, csvContent);
            }

            if (results.CalcResultSummary != null)
            {
                this.calcResultSummaryExporter.Export(results.CalcResultSummary, csvContent);
            }

            return csvContent.ToString();
        }

        private static void PrepareCommsCost(CalcResultCommsCost communicationCost, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();
            csvContent.AppendLine(communicationCost.Name);

            var apportionment = communicationCost.CalcResultCommsCostOnePlusFourApportionment;

            foreach (var onePlusFourApportionment in apportionment)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.England));
                csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.Wales));
                csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.Scotland));
                csvContent.Append(CsvSanitiser.SanitiseData(onePlusFourApportionment.NorthernIreland));
                csvContent.AppendLine(CsvSanitiser.SanitiseData(onePlusFourApportionment.Total));
            }

            csvContent.AppendLine();
            var commCostByMaterials = communicationCost.CalcResultCommsCostCommsCostByMaterial;

            foreach (var commCostByMaterial in commCostByMaterials)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.England));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.Wales));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.Scotland));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.NorthernIreland));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.Total));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.ProducerReportedHouseholdPackagingWasteTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.ReportedPublicBinTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.HouseholdDrinksContainers));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.LateReportingTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(commCostByMaterial.ProducerReportedHouseholdPlusLateReportingTonnage));

                if (commCostByMaterial.Total == CommonConstants.Total || string.IsNullOrWhiteSpace(commCostByMaterial.CommsCostByMaterialPricePerTonne))
                {
                    csvContent.AppendLine(CsvSanitiser.SanitiseData(commCostByMaterial.CommsCostByMaterialPricePerTonne));
                }
                else
                {
                    csvContent.AppendLine(CsvSanitiser.SanitiseData(commCostByMaterial.CommsCostByMaterialPricePerTonne, DecimalPlaces.Four, null, true, false));
                }
            }

            csvContent.AppendLine();
            var countryList = communicationCost.CommsCostByCountry;
            foreach (var country in countryList)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(country.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(country.England));
                csvContent.Append(CsvSanitiser.SanitiseData(country.Wales));
                csvContent.Append(CsvSanitiser.SanitiseData(country.Scotland));
                csvContent.Append(CsvSanitiser.SanitiseData(country.NorthernIreland));
                csvContent.Append(CsvSanitiser.SanitiseData(country.Total));
                csvContent.AppendLine();
            }
        }

        private static void PrepareOtherCosts(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(otherCost.Name);

            var saOperatingCosts = otherCost.SaOperatingCost.OrderBy(x => x.OrderId);

            foreach (var saOperatingCost in saOperatingCosts)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(saOperatingCost.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(saOperatingCost.England));
                csvContent.Append(CsvSanitiser.SanitiseData(saOperatingCost.Wales));
                csvContent.Append(CsvSanitiser.SanitiseData(saOperatingCost.Scotland));
                csvContent.Append(CsvSanitiser.SanitiseData(saOperatingCost.NorthernIreland));
                csvContent.Append(CsvSanitiser.SanitiseData(saOperatingCost.Total));
                csvContent.AppendLine();
            }

            csvContent.AppendLine();

            var laDataPreps = otherCost.Details.OrderBy(x => x.OrderId);

            foreach (var laDataPrep in laDataPreps)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(laDataPrep.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(laDataPrep.England));
                csvContent.Append(CsvSanitiser.SanitiseData(laDataPrep.Wales));
                csvContent.Append(CsvSanitiser.SanitiseData(laDataPrep.Scotland));
                csvContent.Append(CsvSanitiser.SanitiseData(laDataPrep.NorthernIreland));
                csvContent.Append(CsvSanitiser.SanitiseData(laDataPrep.Total));
                csvContent.AppendLine();
            }

            csvContent.AppendLine();
            var schemeCost = otherCost.SchemeSetupCost;
            csvContent.Append(CsvSanitiser.SanitiseData(schemeCost.Name));
            csvContent.Append(CsvSanitiser.SanitiseData(schemeCost.England));
            csvContent.Append(CsvSanitiser.SanitiseData(schemeCost.Wales));
            csvContent.Append(CsvSanitiser.SanitiseData(schemeCost.Scotland));
            csvContent.Append(CsvSanitiser.SanitiseData(schemeCost.NorthernIreland));
            csvContent.AppendLine(CsvSanitiser.SanitiseData(schemeCost.Total));

            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData(otherCost.BadDebtProvision.Key));
            csvContent.AppendLine(CsvSanitiser.SanitiseData(otherCost.BadDebtProvision.Value));

            csvContent.AppendLine();
            var materiality = otherCost.Materiality;
            foreach (var material in materiality)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(material.SevenMateriality));
                csvContent.Append(CsvSanitiser.SanitiseData(material.Amount));
                csvContent.Append(CsvSanitiser.SanitiseData(material.Percentage));
                csvContent.AppendLine();
            }
        }

        private static void AppendRpdFileInfo(StringBuilder csvContent, string rPDFileORG, string rPDFilePOM, string rpdFileORGValue, string rpdFilePOMValue)
        {
            csvContent.AppendLine($"{rPDFileORG},{CsvSanitiser.SanitiseData(rpdFileORGValue)},{rPDFilePOM},{CsvSanitiser.SanitiseData(rpdFilePOMValue)}");
        }

        private static void AppendCsvLine(StringBuilder csvContent, string label, string value)
        {
            csvContent.AppendLine($"{label},{CsvSanitiser.SanitiseData(value, false)}");
        }

        private static void PrepareLapcapData(CalcResultLapcapData calcResultLapcapData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResultLapcapData.Name);
            var lapcapDataDetails = calcResultLapcapData.CalcResultLapcapDataDetails.OrderBy(x => x.OrderId);

            foreach (var lapcapData in lapcapDataDetails)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.EnglandDisposalCost));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.WalesDisposalCost));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.ScotlandDisposalCost));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.NorthernIrelandDisposalCost));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.TotalDisposalCost, false));
                csvContent.AppendLine();
            }
        }

        private static void PrepareLateReportingData(CalcResultLateReportingTonnage calcResultLateReportingData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(CsvSanitiser.SanitiseData(calcResultLateReportingData.Name));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResultLateReportingData.MaterialHeading));
            csvContent.Append(CsvSanitiser.SanitiseData(calcResultLateReportingData.TonnageHeading));
            csvContent.AppendLine();

            foreach (var lateReportingData in calcResultLateReportingData.CalcResultLateReportingTonnageDetails)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(lateReportingData.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(lateReportingData.TotalLateReportingTonnage));
                csvContent.AppendLine();
            }
        }

        private static void PrepareLaDisposalCostData(CalcResultLaDisposalCostData calcResultLaDisposalCostData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResultLaDisposalCostData.Name);
            var lapcapDataDetails = calcResultLaDisposalCostData.CalcResultLaDisposalCostDetails.OrderBy(x => x.OrderId);

            foreach (var lapcapData in lapcapDataDetails)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.Name));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.England));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.Wales));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.Scotland));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.NorthernIreland));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.Total));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.ProducerReportedHouseholdPackagingWasteTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.ReportedPublicBinTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.HouseholdDrinkContainers));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.LateReportingTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.ProducerReportedTotalTonnage));
                csvContent.Append(CsvSanitiser.SanitiseData(lapcapData.DisposalCostPricePerTonne));
                csvContent.AppendLine();
            }
        }

        private static void PrepareScaledupProducers(CalcResultScaledupProducers producers, StringBuilder csvContent)
        {
            // Add empty lines
            csvContent.AppendLine();
            csvContent.AppendLine();

            // Add headers
            PrepareScaledupProducersHeader(producers, csvContent);

            // Add data
            if (!producers.ScaledupProducers.IsNullOrEmpty())
            {
                AppendScaledupProducers(producers, csvContent);
            }
            else
            {
                csvContent.AppendLine(CsvSanitiser.SanitiseData(CalcResultScaledupProducerHeaders.NoScaledupProducers));
            }
        }

        private static void AppendScaledupProducers(CalcResultScaledupProducers producers, StringBuilder csvContent)
        {
            foreach (var producer in producers.ScaledupProducers!)
            {
                if (producer.IsTotalRow)
                {
                    _ = csvContent.Append(new string(CommonConstants.CsvFileDelimiter[0], 7));
                    csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Totals));
                }
                else
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.SubsidiaryId));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerName));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.Level));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.SubmissionPeriodCode));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.DaysInSubmissionPeriod != -1 ? producer.DaysInSubmissionPeriod.ToString() : string.Empty));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.DaysInWholePeriod != -1 ? producer.DaysInWholePeriod.ToString() : string.Empty));
                    csvContent.Append(CsvSanitiser.SanitiseData(producer.ScaleupFactor == -1 ? CommonConstants.Totals : producer.ScaleupFactor.ToString()));
                }

                AppendScaledupProducerTonnageByMaterial(csvContent, producer);

                csvContent.AppendLine();
            }
        }

        private static void AppendScaledupProducerTonnageByMaterial(StringBuilder csvContent, CalcResultScaledupProducer producer)
        {
            foreach (var producerTonnage in producer.ScaledupProducerTonnageByMaterial)
            {
                var materialCode = producerTonnage.Key;
                var tonnage = producerTonnage.Value;

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ReportedHouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ReportedPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                if (materialCode == MaterialCodes.Glass || materialCode == MaterialNames.Glass)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.HouseholdDrinksContainersTonnageGlass, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.TotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ReportedSelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.NetReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupReportedHouseholdPackagingWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupReportedPublicBinTonnage, DecimalPlaces.Three, DecimalFormats.F3));

                if (materialCode == MaterialCodes.Glass || materialCode == MaterialNames.Glass)
                {
                    csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupHouseholdDrinksContainersTonnageGlass, DecimalPlaces.Three, DecimalFormats.F3));
                }

                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupTotalReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupReportedSelfManagedConsumerWasteTonnage, DecimalPlaces.Three, DecimalFormats.F3));
                csvContent.Append(CsvSanitiser.SanitiseData(tonnage.ScaledupNetReportedTonnage, DecimalPlaces.Three, DecimalFormats.F3));
            }
        }

        private static void PrepareScaledupProducersHeader(CalcResultScaledupProducers producers, StringBuilder csvContent)
        {
            // Add scaledup producer header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(producers.TitleHeader!.Name));
            csvContent.AppendLine();

            // Add material breakdown header
            WriteScaledupProducersSecondaryHeaders(producers.MaterialBreakdownHeaders!, csvContent);

            // Add column header
            WriteScaledupProducersColumnHeaders(producers, csvContent);
            csvContent.AppendLine();
        }

        private static void WriteScaledupProducersSecondaryHeaders(IEnumerable<CalcResultScaledupProducerHeader> headers, StringBuilder csvContent)
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

        private static void WriteScaledupProducersColumnHeaders(CalcResultScaledupProducers producers, StringBuilder csvContent)
        {
            foreach (var item in producers.ColumnHeaders!)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(item.Name));
            }
        }
    }
}