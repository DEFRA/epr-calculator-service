using System.Text;
using EPR.Calculator.Service.Function.Builder.Modulation;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using Newtonsoft.Json;

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
            void append(string s)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(s));
            }

            void appendd(decimal d, DecimalPlaces dp)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(d, dp, null));
            }

            append(null);nl(); // TODO trailing , for consistency in tests
            append(null);nl();

            append("Modulation Calculation");
            nl();

            append("Red Modulation Factor");
            appendd(modulationResult.RedFactor, DecimalPlaces.Two);
            nl();

            append("Green discount"); // TODO needs explanation?
            //Green discount  = (red modulation factor - 1) * total red material at amber cost/total green material at amber cost
            //Green discount factor = 1 - green discount
            // TODO can't span columns
            appendd(modulationResult.GreenFactor, DecimalPlaces.Six); // TODO confirm precision
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
            append("Green Material Disposal Cost = Green Factor * Amber Material Disposal Cost");
            nl();

            foreach (var materialName in modulationResult.MaterialNames)
            {
                append(materialName); // A

                Console.WriteLine($">> {materialName} - {JsonConvert.SerializeObject(laDisposalCostData.CalcResultLaDisposalCostDetails, Formatting.Indented)}");

                var laDisposalCost = laDisposalCostData.CalcResultLaDisposalCostDetails.First(laDisposalCost => laDisposalCost.Material == materialName);
                Console.WriteLine($">> {materialName} - {JsonConvert.SerializeObject(laDisposalCost, Formatting.Indented)}");
                append(laDisposalCost.ProducerReportedHouseholdPackagingWasteTonnage); // B
                append(laDisposalCost.ReportedPublicBinTonnage); // C
                append(laDisposalCost.HouseholdDrinkContainers); // D
                append(laDisposalCost.LateReportingTonnage); // E
                //append(laDisposalCost.ActionedSMCW); // F
                append(""); // F - TODO coming...

                appendd(modulationResult.RedTotal + modulationResult.AmberTotal + modulationResult.GreenTotal, DecimalPlaces.Two); // G
                appendd(modulationResult.RedTotal  , DecimalPlaces.Two); // H
                appendd(modulationResult.AmberTotal, DecimalPlaces.Two); // I
                appendd(modulationResult.GreenTotal, DecimalPlaces.Two); // J

                // ...

                var costPerMaterialRag = modulationResult.CostPerMaterial[materialName];
                appendd(costPerMaterialRag[RagRating.Red]  , DecimalPlaces.Two); // K
                //csvContent.Append(CsvSanitiser.SanitiseData(costPerMaterialRag[RagRating.Amber], DecimalPlaces.Two, null));
                appendd(costPerMaterialRag[RagRating.Green], DecimalPlaces.Two); // L

                var pricePerTonneRag = modulationResult.PricePerTonnePerMaterial[materialName];
                appendd(pricePerTonneRag[RagRating.Red]  , DecimalPlaces.Two); // M
                appendd(pricePerTonneRag[RagRating.Amber], DecimalPlaces.Two); // N
                appendd(pricePerTonneRag[RagRating.Green], DecimalPlaces.Two); // O
                nl();
            }
        }
    }
}
