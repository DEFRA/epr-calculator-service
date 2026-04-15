
namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.ProjectedProducers
{
    using System.Text;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
    using EPR.Calculator.Service.Function.Builder.ProjectedProducers;


    [TestClass]
    public class CalcResultProjectedProducersExporterTests
    {
        private CalcResultProjectedProducersExporter exporter = new CalcResultProjectedProducersExporter();

        private readonly List<MaterialDetail> materials = new List<MaterialDetail>()
        {
            new MaterialDetail { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
            new MaterialDetail { Id = 2, Code = "GL", Name = "Glass", Description = "Glass" }
        };

        [TestMethod]
        public void Export_ShouldIncludeProjectedProducers_WhenNotNull()
        {
            // Arrange
            var projectedProducers = new CalcResultProjectedProducers
            {
                H2ProjectedProducersHeaders = H2ProjectedProducersBuilderUtils.GetProjectedProducerHeaders(materials),
                H2ProjectedProducers = GetH2ProjectedProducersList(),
                H1ProjectedProducersHeaders = H1ProjectedProducersBuilderUtils.GetProjectedProducerHeaders(materials),
                H1ProjectedProducers = GetH1ProjectedProducersList(),
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(projectedProducers, csvContent);
            var rows = csvContent.ToString()
                        .Split(Environment.NewLine)
                        .Select(r => r.Split(','))
                        .ToArray();

            // Assert H2
            Assert.IsTrue(rows[2][0].Contains(CalcResultProjectedProducersHeaders.H2ProjectedProducers));

            var h2ColumnHeaders = rows[5];
            var h2ColumnValues = rows[6];
            var h2MaterialHeaders = rows[4];

            var h2MaterialHeadersIndexes = FindAllHeaderIndexes(h2ColumnHeaders, CalcResultProjectedProducersHeaders.HouseholdPackagingTonnage);
            Assert.IsTrue(h2MaterialHeaders[h2MaterialHeadersIndexes.First()].Contains("Aluminium Breakdown"));
            Assert.IsTrue(h2MaterialHeaders[h2MaterialHeadersIndexes.Last()].Contains("Glass Breakdown"));

            var h2Data = GetColumnHeaderValues(h2ColumnHeaders, h2ColumnValues);

            Assert.AreEqual("101001", h2Data[CalcResultProjectedProducersHeaders.ProducerId].First());
            Assert.AreEqual(string.Empty, h2Data[CalcResultProjectedProducersHeaders.SubsidiaryId].First());
            Assert.AreEqual("1", h2Data[CalcResultProjectedProducersHeaders.Level].First());
            Assert.AreEqual("2026-H2", h2Data[CalcResultProjectedProducersHeaders.SubmissionPeriodCode].First());
            //Aluminium
            Assert.AreEqual("100.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdPackagingTonnage].First());
            Assert.AreEqual("30.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdRedTonnage].First());
            Assert.AreEqual("40.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdAmberTonnage].First());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdGreenTonnage].First());
            Assert.AreEqual("40.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdRedMedicalTonnage].First());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdAmberMedicalTonnage].First());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdGreenMedicalTonnage].First());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdTonnageWithoutRAMDefaultedToRed].First());
            Assert.AreEqual("300.000", h2Data[CalcResultProjectedProducersHeaders.TotalTonnage].First());
            //Glass
            Assert.AreEqual("500.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersPackagingTonnage].Last());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedTonnage].Last());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberTonnage].Last());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenTonnage].Last());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedMedicalTonnage].Last());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberMedicalTonnage].Last());
            Assert.AreEqual("0.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenMedicalTonnage].Last());
            Assert.AreEqual("500.000", h2Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersTonnageWithoutRAMDefaultedToRed].Last());
            Assert.AreEqual("800.000", h2Data[CalcResultProjectedProducersHeaders.TotalTonnage].Last());

            // Assert H1
            Assert.IsTrue(rows[9][0].Contains(CalcResultProjectedProducersHeaders.H1ProjectedProducers));

            var h1ColumnHeaders = rows[12];
            var h1ColumnValues = rows[13];
            var aluminiumIndex = 0;
            var projectedAluminiumIndex = 1;
            var glassIndex = 2;
            var projectedGlassIndex = 3;

            var h1MaterialHeaders = rows[11];
            var h1MaterialHeadersIndexes = FindAllHeaderIndexes(h1ColumnHeaders, CalcResultProjectedProducersHeaders.HouseholdPackagingTonnage);
            Assert.IsTrue(h1MaterialHeaders[h1MaterialHeadersIndexes[aluminiumIndex]].Contains("Aluminium Breakdown"));
            Assert.IsTrue(h1MaterialHeaders[h1MaterialHeadersIndexes[projectedAluminiumIndex]].Contains("Projected Aluminium Breakdown"));
            Assert.IsTrue(h1MaterialHeaders[h1MaterialHeadersIndexes[glassIndex]].Contains("Glass Breakdown"));
            Assert.IsTrue(h1MaterialHeaders[h1MaterialHeadersIndexes[projectedGlassIndex]].Contains("Projected Glass Breakdown"));

            var h1Data = GetColumnHeaderValues(h1ColumnHeaders, h1ColumnValues);

            Assert.AreEqual("101001", h1Data[CalcResultProjectedProducersHeaders.ProducerId].First());
            Assert.AreEqual(string.Empty, h1Data[CalcResultProjectedProducersHeaders.SubsidiaryId].First());
            Assert.AreEqual("1", h1Data[CalcResultProjectedProducersHeaders.Level].First());
            Assert.AreEqual("2026-H1", h1Data[CalcResultProjectedProducersHeaders.SubmissionPeriodCode].First());
            //Aluminium
            Assert.AreEqual("100.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinPackagingTonnage][aluminiumIndex]);
            Assert.AreEqual("30.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinRedTonnage][aluminiumIndex]);
            Assert.AreEqual("40.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinAmberTonnage][aluminiumIndex]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinGreenTonnage][aluminiumIndex]);
            Assert.AreEqual("40.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinRedMedicalTonnage][aluminiumIndex]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinAmberMedicalTonnage][aluminiumIndex]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinGreenMedicalTonnage][aluminiumIndex]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinTonnageWithoutRAM][aluminiumIndex]);
            // H2 Aluminium proportions
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.RedH2MaterialTonnageProportion].First());
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.AmberH2MaterialTonnageProportion].First());
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.GreenH2MaterialTonnageProportion].First());
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.RedMedicalH2MaterialTonnageProportion].First());
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.AmberMedicalH2MaterialTonnageProportion].First());
            Assert.AreEqual("-", h1Data[CalcResultProjectedProducersHeaders.GreenMedicalH2MaterialTonnageProportion].First());
            //Projected Aluminium
            Assert.AreEqual("100.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinPackagingTonnage][projectedAluminiumIndex]);
            Assert.AreEqual("25.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinRedTonnage][projectedAluminiumIndex]);
            Assert.AreEqual("20.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinAmberTonnage][projectedAluminiumIndex]);
            Assert.AreEqual("50.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinGreenTonnage][projectedAluminiumIndex]);
            Assert.AreEqual("10.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinRedMedicalTonnage][projectedAluminiumIndex]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinAmberMedicalTonnage][projectedAluminiumIndex]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.PublicBinGreenMedicalTonnage][projectedAluminiumIndex]);
            
            var hdcIndex = 0;
            var projectedHdcIndex = 1;
            //Glass
            Assert.AreEqual("500.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersPackagingTonnage][hdcIndex]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedTonnage][hdcIndex]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberTonnage][hdcIndex]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenTonnage][hdcIndex]);
            Assert.AreEqual("100.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedMedicalTonnage][hdcIndex]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberMedicalTonnage][hdcIndex]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenMedicalTonnage][hdcIndex]);
            Assert.AreEqual("400.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersTonnageWithoutRAM][hdcIndex]);
            // H2 Glass proportions
            Assert.AreEqual("60.00%", h1Data[CalcResultProjectedProducersHeaders.RedH2MaterialTonnageProportion].Last());
            Assert.AreEqual("50.00%", h1Data[CalcResultProjectedProducersHeaders.AmberH2MaterialTonnageProportion].Last());
            Assert.AreEqual("40.00%", h1Data[CalcResultProjectedProducersHeaders.GreenH2MaterialTonnageProportion].Last());
            Assert.AreEqual("30.00%", h1Data[CalcResultProjectedProducersHeaders.RedMedicalH2MaterialTonnageProportion].Last());
            Assert.AreEqual("20.00%", h1Data[CalcResultProjectedProducersHeaders.AmberMedicalH2MaterialTonnageProportion].Last());
            Assert.AreEqual("10.00%", h1Data[CalcResultProjectedProducersHeaders.GreenMedicalH2MaterialTonnageProportion].Last());
            //Projected Glass
            Assert.AreEqual("700.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersPackagingTonnage][projectedHdcIndex]);
            Assert.AreEqual("50.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedTonnage][projectedHdcIndex]);
            Assert.AreEqual("200.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberTonnage][projectedHdcIndex]);
            Assert.AreEqual("50.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenTonnage][projectedHdcIndex]);
            Assert.AreEqual("200.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedMedicalTonnage][projectedHdcIndex]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberMedicalTonnage][projectedHdcIndex]);
            Assert.AreEqual("0.000", h1Data[CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenMedicalTonnage][projectedHdcIndex]);
        }

        [TestMethod]
        public void Export_ShouldIncludeEmptyProjectedProducers_WhenNull()
        {
            var projectedProducers = new CalcResultProjectedProducers
            {
                H2ProjectedProducersHeaders = H2ProjectedProducersBuilderUtils.GetProjectedProducerHeaders(materials),
                H2ProjectedProducers = null,
                H1ProjectedProducersHeaders = H1ProjectedProducersBuilderUtils.GetProjectedProducerHeaders(materials),
                H1ProjectedProducers = null,
            };

            var csvContent = new StringBuilder();

            exporter.Export(projectedProducers, csvContent);

            var rows = csvContent.ToString()
                        .Split(Environment.NewLine)
                        .Select(r => r.Split(','))
                        .ToArray();

            Assert.IsTrue(rows[2][0].Contains(CalcResultProjectedProducersHeaders.H2ProjectedProducers));
            Assert.IsTrue(rows[6][0].Contains(CalcResultProjectedProducersHeaders.NoProjectedProducers));
            Assert.IsTrue(rows[9][0].Contains(CalcResultProjectedProducersHeaders.H1ProjectedProducers));
            Assert.IsTrue(rows[13][0].Contains(CalcResultProjectedProducersHeaders.NoProjectedProducers));
        }

        private List<CalcResultH2ProjectedProducer> GetH2ProjectedProducersList()
        {
            return new List<CalcResultH2ProjectedProducer>()
                {
                    new CalcResultH2ProjectedProducer
                    {
                        ProducerId = 101001,
                        SubsidiaryId = null,
                        Level = "1",
                        SubmissionPeriodCode = "2026-H2",
                        ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>
                        {
                            {
                                "AL",
                                new CalcResultH2ProjectedProducerMaterialTonnage
                                {
                                   HouseholdRAMTonnage = new RAMTonnage{ Tonnage = 100, RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                   HouseholdTonnageDefaultedRed = 0,
                                   PublicBinRAMTonnage = new RAMTonnage{ Tonnage = 200, RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                   PublicBinTonnageDefaultedRed = 0,
                                   TotalTonnage = 300
                                }
                            },
                            {
                                "GL",
                                new CalcResultH2ProjectedProducerMaterialTonnage
                                {
                                    HouseholdRAMTonnage = new RAMTonnage{ Tonnage = 200, RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                    HouseholdTonnageDefaultedRed = 0,
                                    PublicBinRAMTonnage = new RAMTonnage{ Tonnage = 100, RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                    PublicBinTonnageDefaultedRed = 0,
                                    HouseholdDrinksContainerRAMTonnage = new RAMTonnage{ Tonnage = 500, RedTonnage = 0, RedMedicalTonnage = 0, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                    HouseholdDrinksContainerDefaultedRed = 500,
                                    TotalTonnage = 800
                                }
                            },
                        },
                    },
                };
        }

        private List<CalcResultH1ProjectedProducer> GetH1ProjectedProducersList()
        {
            return new List<CalcResultH1ProjectedProducer>()
                {
                    new CalcResultH1ProjectedProducer
                    {
                        ProducerId = 101001,
                        SubsidiaryId = null,
                        Level = "1",
                        SubmissionPeriodCode = "2026-H1",
                        ProjectedTonnageByMaterial = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>
                        {
                            {
                                "AL",
                                new CalcResultH1ProjectedProducerMaterialTonnage
                                {
                                   HouseholdRAMTonnage = new RAMTonnage{ Tonnage = 200, RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                   HouseholdTonnageWithoutRAM = 0,
                                   PublicBinRAMTonnage = new RAMTonnage{ Tonnage = 100, RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                   PublicBinTonnageWithoutRAM = 0,
                                   RedH2Proportion = 0.1m,
                                   AmberH2Proportion = 0.2m,
                                   GreenH2Proportion = 0.3m,
                                   RedMedicalH2Proportion = 0.4m,
                                   AmberMedicalH2Proportion = 0.5m,
                                   GreenMedicalH2Proportion = 0.6m,
                                   ProjectedHouseholdRAMTonnage = new RAMTonnage{ Tonnage = 50, RedTonnage = 15, RedMedicalTonnage = 20, AmberTonnage = 10, AmberMedicalTonnage = 0, GreenTonnage = 20, GreenMedicalTonnage = 0 },
                                   ProjectedPublicBinRAMTonnage = new RAMTonnage{ Tonnage = 100, RedTonnage = 25, RedMedicalTonnage = 10, AmberTonnage = 20, AmberMedicalTonnage = 0, GreenTonnage = 50, GreenMedicalTonnage = 0 },
                                }
                            },
                            {
                                "GL",
                                new CalcResultH1ProjectedProducerMaterialTonnage
                                {
                                    HouseholdRAMTonnage = new RAMTonnage{ Tonnage = 100, RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                    HouseholdTonnageWithoutRAM = 0,
                                    PublicBinRAMTonnage = new RAMTonnage{ Tonnage = 200, RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                    PublicBinTonnageWithoutRAM = 0,
                                    HouseholdDrinksContainerRAMTonnage = new RAMTonnage{ Tonnage = 500, RedTonnage = 0, RedMedicalTonnage = 100, AmberTonnage = 0, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                    HouseholdDrinksContainerTonnageWithoutRAM = 400,
                                    RedH2Proportion = 0.6m,
                                    AmberH2Proportion = 0.5m,
                                    GreenH2Proportion = 0.4m,
                                    RedMedicalH2Proportion = 0.3m,
                                    AmberMedicalH2Proportion = 0.2m,
                                    GreenMedicalH2Proportion = 0.1m,
                                    ProjectedHouseholdRAMTonnage = new RAMTonnage{ Tonnage = 100, RedTonnage = 15, RedMedicalTonnage = 20, AmberTonnage = 10, AmberMedicalTonnage = 0, GreenTonnage = 20, GreenMedicalTonnage = 0 },
                                    ProjectedPublicBinRAMTonnage = new RAMTonnage{ Tonnage = 50, RedTonnage = 25, RedMedicalTonnage = 10, AmberTonnage = 20, AmberMedicalTonnage = 0, GreenTonnage = 50, GreenMedicalTonnage = 0 },
                                    ProjectedHouseholdDrinksContainerRAMTonnage = new RAMTonnage{ Tonnage = 700, RedTonnage = 50, RedMedicalTonnage = 200, AmberTonnage = 200, AmberMedicalTonnage = 0, GreenTonnage = 50, GreenMedicalTonnage = 0 },
                                }
                            },
                        },
                    },
                };
        }

        private Dictionary<string, List<string>> GetColumnHeaderValues(string[] headers, string[] values)
        {
            return headers
                    .Select((h, i) => new
                    {
                        Key = h?.Trim().Trim('"'),
                        Value = (i < values.Length ? values[i] : null)?
                                    .Trim()
                                    .Trim('"') 
                                ?? string.Empty
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                    .GroupBy(x => x.Key!)
                    .ToDictionary(
                        g => g.Key!,
                        g => g.Select(x => x.Value).ToList()
                    );
        }

        private int[] FindAllHeaderIndexes(string[] h1ColumnHeaders, string headerToFind)
        {
            return h1ColumnHeaders
                        .Select((h, i) => new { h, i })
                        .Where(x => x.h.Contains(headerToFind))
                        .Select(x => x.i)
                        .ToArray();
        }
    }
}