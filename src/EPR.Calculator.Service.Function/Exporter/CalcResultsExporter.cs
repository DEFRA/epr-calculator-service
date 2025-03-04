namespace EPR.Calculator.API.Exporter
{
    using EPR.Calculator.API.Utils;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Exporter;
    using EPR.Calculator.Service.Function.Exporter.LaDisposalCost;
    using EPR.Calculator.Service.Function.Exporter.OtherCosts;
    using EPR.Calculator.Service.Function.Exporter.ScaledupProducers;
    using EPR.Calculator.Service.Function.Models;
    using System;
    using System.Linq;
    using System.Text;

    public class CalcResultsExporter : ICalcResultsExporter<CalcResult>
    {
        private readonly ICalcResultSummaryExporter calcResultSummaryExporter;
        private readonly ICalcResultDetailExporter resultDetailexporter;
        private readonly IOnePlusFourApportionmentExporter onePlusFourApportionmentExporter;
        private readonly ILapcaptDetailExporter lapcaptDetailExporter;
        private readonly ICalcResultParameterOtherCostExporter parameterOtherCosts;
        private readonly ILateReportingExporter lateReportingExporter;
        private readonly ICalcResultScaledupProducersExporter calcResultScaledupProducersExporter;
        private readonly ICalcResultLaDisposalCostExporter laDisposalCostExporter;

        public CalcResultsExporter(
            ILateReportingExporter lateReportingExporter,
            ICalcResultDetailExporter resultDetailexporter,
            IOnePlusFourApportionmentExporter onePlusFourApportionmentExporter,
            ICalcResultLaDisposalCostExporter laDisposalCostExporter,
            ICalcResultScaledupProducersExporter calcResultScaledupProducersExporter,
            ILapcaptDetailExporter lapcaptDetailExporter,
            ICalcResultParameterOtherCostExporter parameterOtherCosts,
            ICalcResultSummaryExporter calcResultSummaryExporter)
        {
            this.resultDetailexporter = resultDetailexporter;
            this.onePlusFourApportionmentExporter = onePlusFourApportionmentExporter;
            this.lateReportingExporter = lateReportingExporter;
            this.calcResultScaledupProducersExporter = calcResultScaledupProducersExporter;
            this.lapcaptDetailExporter = lapcaptDetailExporter;
            this.parameterOtherCosts = parameterOtherCosts;
            this.calcResultSummaryExporter = calcResultSummaryExporter;
            this.laDisposalCostExporter = laDisposalCostExporter;
        }

        public string Export(CalcResult results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results), "The results parameter cannot be null.");
            }

            var csvContent = new StringBuilder();
            resultDetailexporter.Export(results.CalcResultDetail, csvContent);

            lapcaptDetailExporter.Export(results.CalcResultLapcapData, csvContent);

            csvContent.Append(lateReportingExporter.Export(results.CalcResultLateReportingTonnageData));

            parameterOtherCosts.Export(results.CalcResultParameterOtherCost, csvContent);

            onePlusFourApportionmentExporter.Export(results.CalcResultOnePlusFourApportionment, csvContent);

            PrepareCommsCost(results.CalcResultCommsCostReportDetail, csvContent);

            this.laDisposalCostExporter.Export(results.CalcResultLaDisposalCostData, csvContent);

            calcResultScaledupProducersExporter.Export(results.CalcResultScaledupProducers, csvContent);

            this.calcResultSummaryExporter.Export(results.CalcResultSummary, csvContent);

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
    }
}