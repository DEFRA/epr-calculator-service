using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using System.Text.Json;

namespace EPR.Calculator.Service.Function.UnitTests.Models.JsonExporter
{
    [TestClass]
    public class CalcResult2ACommsDataByMaterialTests
    {
        [TestMethod]
        public void From_MapsMaterialsAndTotal()
        {
            // Arrange
            var comms = new Dictionary<string, CalcResultCommsCostCommsCostByMaterial>
            {
                ["AL"] = new CalcResultCommsCostCommsCostByMaterial
                    {
                        CommsCostByMaterialPricePerTonne = 1.23m,
                        ProducerReportedHouseholdPackagingWasteTonnage = 2.34m,
                        LateReportingTonnage = 3.45m,
                        ReportedPublicBinTonnage = 4.56m,
                        ProducerReportedTotalTonnage = 5.67m
                    },
                ["GL"] = new CalcResultCommsCostCommsCostByMaterial
                {
                    CommsCostByMaterialPricePerTonne = 2.34m,
                    ProducerReportedHouseholdPackagingWasteTonnage = 3.45m,
                    LateReportingTonnage = 4.56m,
                    ReportedPublicBinTonnage = 5.67m,
                    ProducerReportedTotalTonnage = 6.78m
                }
            };
            var total = new CalcResultCommsCostCommsCostByMaterial
            {
                ProducerReportedHouseholdPackagingWasteTonnage = 5.79m,
                LateReportingTonnage = 8.01m,
                ReportedPublicBinTonnage = 10.23m,
                ProducerReportedTotalTonnage = 12.45m
            };
            var materials = TestDataHelper.GetMaterials();

            // Act
            var result = CalcResult2ACommsDataByMaterial.From(materials, comms, total);

            // Assert
            Assert.IsNotNull(result);
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            var expectedJson = """
                {
                "name": "2a Comms Costs - by Material",
                "calcResult2aCommsDataDetails": [
                  {
                    "materialName"                          : "Aluminium",
                    "englandCommsCost"                      : "£0.00",
                    "walesCommsCost"                        : "£0.00",
                    "scotlandCommsCost"                     : "£0.00",
                    "northernIrelandCommsCost"              : "£0.00",
                    "totalCommsCost"                        : "£0.00",
                    "commsCostByMaterialPricePerTonne"      : "£1.2300",
                    "producerHouseholdPackagingWasteTonnage": 2.340,
                    "publicBinTonnage"                      : 4.560,
                    "householdDrinksContainersTonnage"      : 0,
                    "lateReportingTonnage"                  : 3.450,
                    "totalTonnage"                          : 5.670
                  },
                  {
                    "materialName"                          : "Glass",
                    "englandCommsCost"                      : "£0.00",
                    "walesCommsCost"                        : "£0.00",
                    "scotlandCommsCost"                     : "£0.00",
                    "northernIrelandCommsCost"              : "£0.00",
                    "totalCommsCost"                        : "£0.00",
                    "commsCostByMaterialPricePerTonne"      : "£2.3400",
                    "producerHouseholdPackagingWasteTonnage": 3.450,
                    "publicBinTonnage"                      : 5.670,
                    "householdDrinksContainersTonnage"      : 0,
                    "lateReportingTonnage"                  : 4.560,
                    "totalTonnage"                          : 6.780
                  }
                ],
                "calcResult2aCommsDataDetailsTotal": {
                  "total"                                      : "Total",
                  "englandCommsCostTotal"                      : "£0.00",
                  "walesCommsCostTotal"                        : "£0.00",
                  "scotlandCommsCostTotal"                     : "£0.00",
                  "northernIrelandCommsCostTotal"              : "£0.00",
                  "totalCommsCostTotal"                        : "£0.00",
                  "producerHouseholdPackagingWasteTonnageTotal": 5.79,
                  "publicBinTonnage"                           : 10.23,
                  "householdDrinksContainersTonnageTotal"      : 0,
                  "lateReportingTonnageTotal"                  : 8.01,
                  "totalTonnageTotal"                          : 12.45
                }
                }
                """;
            JsonTestUtils.AssertJson(expectedJson, json);
        }
    }
}
