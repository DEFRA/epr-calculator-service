using System.Text;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using Newtonsoft.Json;
using EPR.Calculator.Service.Function.Constants;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation
{
    public class CalcResultModulationExporter : ICalcResultModulationExporter
    {
        public void Export(CalcResultLaDisposalCostData laDisposalCostData, ModulationResult modulationResult, StringBuilder csvContent)
        {
            void nl()
            {
                csvContent.AppendLine();
            }
            void append(string? s)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(s));
            }

            void appendd(decimal? d, DecimalPlaces dp)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(d, dp, null));
            }
            void appendc(decimal? d, DecimalPlaces dp)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(d, dp, null, true));
            }

            nl();
            nl();

            append("Modulation Calculation");
            nl();

            append("Red Modulation Factor");
            appendd(modulationResult.RedFactor, DecimalPlaces.Three);
            nl();

            append("Green Modulation Factor");
            appendd(modulationResult.GreenFactor, DecimalPlaces.Six);
            nl();

            append("Material");
            append("Producer Household Packaging Tonnage");
            append("Public Bin Tonnage");
            append("Household Drinks Containers Tonnage");
            append("Late Reporting Tonnage");
            append("Actioned Self-Managed Consumer Waste");
            append("Producer Household Tonnage + Late Reporting Tonnage + Public Bin Tonnage + Household Drinks Containers Tonnage - Self-Managed Consumer Waste");
            append("Red + Red-Medical Household Tonnage + Red + Red-Medical Public Bin Tonnage + Red + Red-Medical Household Drinks Containers Tonnage (Net Tonnage)");
            append("Amber + Amber-Medical Household Tonnage + Amber Public Bin Tonnage + Amber-Medical Public Bin + Amber Household Drinks Containers + Amber Household Drinks Containers Tonnage (Net Tonnage)");
            append("Green + Green Medical Household + Green Public Bin + Green Medical Public Bin Tonnage+ Green Household Drinks Containers + Green Medical Household Drinks Containers (Net Tonnage)");
            append("Total Red Material at Amber Disposal Cost = Amber Material Disposal Cost x Red Material Tonnage");
            append("Total Green Material at Amber Disposal Cost = Amber Material Disposal Cost x Green Material Tonnage");
            append("Red Material Disposal Cost = Red Modulation Factor * Amber Material Disposal Cost");
            append("Amber Material Disposal Cost = Material Disposal Cost per Tonne");
            append("Green Material Disposal Cost = Green Modulation Factor * Amber Material Disposal Cost");
            nl();

            foreach (var kv in modulationResult.MaterialModulation)
            {
                var materialName = kv.Key;
                var modulation = kv.Value;
                append(materialName); // A

                var laDisposalCost = laDisposalCostData.CalcResultLaDisposalCostDetails.First(laDisposalCost => laDisposalCost.Name == materialName);
                append(laDisposalCost.ProducerReportedHouseholdPackagingWasteTonnage); // B
                append(laDisposalCost.ReportedPublicBinTonnage); // C
                append(laDisposalCost.HouseholdDrinkContainers); // D
                append(laDisposalCost.LateReportingTonnage); // E
                //append(laDisposalCost.ActionedSMCW); // F
                append(""); // F - TODO coming...

                var netR = laDisposalCostData.NetByMaterialAndRag[materialName][RagRating.Red];
                var netA = laDisposalCostData.NetByMaterialAndRag[materialName][RagRating.Amber];
                var netG = laDisposalCostData.NetByMaterialAndRag[materialName][RagRating.Green];
                appendd(netR + netA + netG, DecimalPlaces.Two); // G
                appendd(netR, DecimalPlaces.Two); // H
                appendd(netA, DecimalPlaces.Two); // I
                appendd(netG, DecimalPlaces.Two); // J

                appendc(modulation.TotalRedMaterialAtAmberDisposalCost, DecimalPlaces.Two); // K
                appendc(modulation.TotalGreenMaterialAtAmberDisposalCost, DecimalPlaces.Two); // L

                appendc(modulation.RedMaterialDisposalCost  , DecimalPlaces.Four); // M
                appendc(modulation.AmberMaterialDisposalCost, DecimalPlaces.Four); // N
                appendc(modulation.GreenMaterialDisposalCost, DecimalPlaces.Four); // O
                nl();
            }

            {
                append(CommonConstants.Total); // A
                var laDisposalCost = laDisposalCostData.CalcResultLaDisposalCostDetails.First(laDisposalCost => laDisposalCost.Name == CommonConstants.Total);
                append(laDisposalCost.ProducerReportedHouseholdPackagingWasteTonnage); // B
                append(laDisposalCost.ReportedPublicBinTonnage); // C
                append(laDisposalCost.HouseholdDrinkContainers); // D
                append(laDisposalCost.LateReportingTonnage); // E
                //append(laDisposalCost.ActionedSMCW); // F
                append(""); // F - TODO coming...

                var netR = laDisposalCostData.NetByMaterialAndRag.Values.Select(e => e[RagRating.Red]).Sum();
                var netA = laDisposalCostData.NetByMaterialAndRag.Values.Select(e => e[RagRating.Amber]).Sum();
                var netG = laDisposalCostData.NetByMaterialAndRag.Values.Select(e => e[RagRating.Green]).Sum();
                appendd(netR + netA + netG, DecimalPlaces.Two); // G
                appendd(netR, DecimalPlaces.Two); // H
                appendd(netA, DecimalPlaces.Two); // I
                appendd(netG, DecimalPlaces.Two); // J

                appendc(modulationResult.MaterialModulation.Values.Select(m => m.TotalRedMaterialAtAmberDisposalCost  ).Sum(), DecimalPlaces.Two); // K
                appendc(modulationResult.MaterialModulation.Values.Select(m => m.TotalGreenMaterialAtAmberDisposalCost).Sum(), DecimalPlaces.Two); // L

                appendc(null, DecimalPlaces.Two); // M
                appendc(null, DecimalPlaces.Two); // N
                appendc(null, DecimalPlaces.Two); // O
                nl();
            }
        }
    }
}
