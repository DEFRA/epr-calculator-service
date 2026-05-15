using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Models;
using NetTopologySuite.Algorithm;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.CommsCost
{
    [TestClass]
    public class CalcResultCommsCostExporterTests
    {
        private ICalcResultCommsCostExporter exporter = new CalcResultCommsCostExporter();

        [TestMethod]
        public void ExportCommsCost_PopulatedLists_ReturnsStringBuilderWithData()
        {
            // Arrange
            var communicationCost = new CalcResultCommsCost
            {
                CalcResultCommsCostOnePlusFourApportionment = new List<CalcResultCommsCostOnePlusFourApportionment>
                    {
                        new CalcResultCommsCostOnePlusFourApportionment { Name = "1 + 4 Apportionment %s", England = 10, Wales = 20, Scotland = 30, NorthernIreland = 40, Total = 100 }
                    },
                CalcResultCommsCostCommsCostByMaterial = new List<CalcResultCommsCostCommsCostByMaterial>
                    {
                        new CalcResultCommsCostCommsCostByMaterial { Name = "Aluminium", England = 10, Wales = 20, Scotland = 30, NorthernIreland = 40, Total = 100, ProducerReportedHouseholdPackagingWasteTonnage = 50, ReportedPublicBinTonnage = 60, HouseholdDrinksContainers = 70, LateReportingTonnage = 80, ProducerReportedTotalTonnage = 90, CommsCostByMaterialPricePerTonne = 100 },
                        new CalcResultCommsCostCommsCostByMaterial { Name = "Total"    , England = 10, Wales = 20, Scotland = 30, NorthernIreland = 40, Total = 100, ProducerReportedHouseholdPackagingWasteTonnage = 50, ReportedPublicBinTonnage = 60, HouseholdDrinksContainers = 70, LateReportingTonnage = 80, ProducerReportedTotalTonnage = 90, CommsCostByMaterialPricePerTonne = 100 }
                    },
                CommsCostByCountry = new List<CalcResultCommsCostOnePlusFourApportionment>
                    {
                        new CalcResultCommsCostOnePlusFourApportionment { Name = "2b Comms Costs - UK wide"   , England = 10, Wales = 20, Scotland = 30, NorthernIreland = 40, Total = 100 },
                        new CalcResultCommsCostOnePlusFourApportionment { Name = "2c Comms Costs - by Country", England = 11, Wales = 21, Scotland = 31, NorthernIreland = 41, Total = 104 }
                    }
            };

            // Act
            var csvContent = new StringBuilder();
            exporter.Export(communicationCost, csvContent);

            var result = csvContent.ToString().Split("\n").Select(s => s.TrimEnd(',')).ToArray();
            //Console.WriteLine($">> {JsonConvert.SerializeObject(result, Formatting.Indented)}");
            Console.WriteLine(string.Join("\n", result));

            var expected = new[] {
                new string[] {},
                new string[] {},
                new[] { "Parameters - Comms Costs" },
                new[] { null, "England","Wales","Scotland","Northern Ireland","Total" },
                new[] {"1 + 4 Apportionment %s","10.00000000%","20.00000000%","30.00000000%","40.00000000%","100.00000000%"},
                new string[] { },
                new[] { "2a Comms Costs - by Material","England","Wales","Scotland","Northern Ireland","Total","Producer Household Packaging Tonnage","Late Reporting Tonnage","Public Bin Tonnage","Household Drinks Containers Tonnage","Producer Household Tonnage + Late Reporting Tonnage + Public Bin Tonnage + Household Drinks Containers Tonnage","Comms Cost - by Material Price Per Tonne" },
                new[] { "Aluminium","£10.00","£20.00","£30.00","£40.00","£100.00","50.000","60.000","70.000","80.000","90.000","£100" },
                new[] { "Total","£10.00","£20.00","£30.00","£40.00","£100.00","50.000","60.000","70.000","80.000","90.000","£100" },
                new string[] { },
                new [] {  "2b Comms Costs - UK wide","£10.00","£20.00","£30.00","£40.00","£100.00" },
                new [] {  "2c Comms Costs - by Country","£11.00","£21.00","£31.00","£41.00","£104.00" },
                new string[] { }
            };

            CsvTestUtils.AssertCsv(expected, result);
        }
    }
}
