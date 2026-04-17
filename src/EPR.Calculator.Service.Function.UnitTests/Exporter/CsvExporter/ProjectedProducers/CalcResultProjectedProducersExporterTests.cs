
using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.ProjectedProducers
{
    [TestClass]
    public class CalcResultProjectedProducersExporterTests
    {
        private CalcResultProjectedProducersExporter exporter;

        public CalcResultProjectedProducersExporterTests()
        {
            exporter = new CalcResultProjectedProducersExporter();
        }

        [TestMethod]
        public void Export_ShouldIncludeProjectedProducers_WhenNotNull()
        {
            // Arrange
            var projectedProducers = new CalcResultProjectedProducers
            {
                H2ProjectedProducersHeaders = GetH2ProjectedProducersHeaders(),
                H2ProjectedProducers = GetH2ProjectedProducersList(),
            };

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(projectedProducers, csvContent);
            var result = csvContent.ToString().Split(Environment.NewLine);

            // Assert
            Assert.IsTrue(result[2].Contains(CalcResultProjectedProducersHeaders.H2ProjectedProducers));

            var materialBreakdownHeaders = result[4].Split(',');
            Assert.IsTrue(materialBreakdownHeaders[0].Contains("Aluminium"));
            Assert.IsTrue(materialBreakdownHeaders[1].Contains("Glass"));

            var columnHeaders = result[5].Split(',');
            var columnValues = result[6].Split(',');

            Assert.IsTrue(columnHeaders[0].Contains(CalcResultProjectedProducersHeaders.ProducerId));
            Assert.IsTrue(columnValues[0].Contains("101001"));
            Assert.IsTrue(columnHeaders[1].Contains(CalcResultProjectedProducersHeaders.SubsidiaryId));
            Assert.IsTrue(columnValues[1].Contains(string.Empty));
            Assert.IsTrue(columnHeaders[2].Contains(CalcResultProjectedProducersHeaders.Level));
            Assert.IsTrue(columnValues[2].Contains("1"));
            Assert.IsTrue(columnHeaders[3].Contains(CalcResultProjectedProducersHeaders.SubmissionPeriodCode));
            Assert.IsTrue(columnValues[3].Contains("2026-H2"));
            Assert.IsTrue(columnHeaders[4].Contains(CalcResultProjectedProducersHeaders.HouseholdPackagingTonnage));
            Assert.IsTrue(columnValues[4].Contains("100.000"));
            Assert.IsTrue(columnHeaders[5].Contains(CalcResultProjectedProducersHeaders.HouseholdRedTonnage));
            Assert.IsTrue(columnValues[5].Contains("30.000"));
            Assert.IsTrue(columnHeaders[6].Contains(CalcResultProjectedProducersHeaders.HouseholdAmberTonnage));
            Assert.IsTrue(columnValues[6].Contains("40.000"));
            Assert.IsTrue(columnHeaders[7].Contains(CalcResultProjectedProducersHeaders.HouseholdGreenTonnage));
            Assert.IsTrue(columnValues[7].Contains("0.000"));
            Assert.IsTrue(columnHeaders[8].Contains(CalcResultProjectedProducersHeaders.HouseholdRedMedicalTonnage));
            Assert.IsTrue(columnValues[8].Contains("40.000"));
            Assert.IsTrue(columnHeaders[9].Contains(CalcResultProjectedProducersHeaders.HouseholdAmberMedicalTonnage));
            Assert.IsTrue(columnValues[9].Contains("0.000"));
            Assert.IsTrue(columnHeaders[10].Contains(CalcResultProjectedProducersHeaders.HouseholdGreenMedicalTonnage));
            Assert.IsTrue(columnValues[10].Contains("0.000"));
            Assert.IsTrue(columnHeaders[11].Contains(CalcResultProjectedProducersHeaders.HouseholdTonnageWithoutRAMDefaultedToRed));
            Assert.IsTrue(columnValues[11].Contains("0.000"));
            
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
                                    HouseholdRAMTonnage = new RAMTonnage{ Tonnage = 100, RedTonnage = 30, RedMedicalTonnage = 40, AmberTonnage = 40, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
                                    HouseholdTonnageDefaultedRed = 0,
                                    PublicBinRAMTonnage = new RAMTonnage{ Tonnage = 200, RedTonnage = 50, RedMedicalTonnage = 20, AmberTonnage = 30, AmberMedicalTonnage = 0, GreenTonnage = 0, GreenMedicalTonnage = 0 },
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

        private ProjectedProducersHeaders GetH2ProjectedProducersHeaders()
        {
            return new ProjectedProducersHeaders
            {
                TitleHeader = GetTitleHeader(),
                MaterialBreakdownHeaders = GetMaterialBreakdownHeaders(),
                ColumnHeaders = GetH2ProjectedProducersColumnHeaders()
            };
        }

        private ProjectedProducersHeader GetTitleHeader()
        {
            return new ProjectedProducersHeader

            {
                Name = CalcResultProjectedProducersHeaders.H2ProjectedProducers,
                ColumnIndex = 1,
            };
        }

        private IEnumerable<ProjectedProducersHeader> GetMaterialBreakdownHeaders()
        {
            return new List<ProjectedProducersHeader>
            {
                new ProjectedProducersHeader
                {
                    Name = "Aluminium Breakdown",
                    ColumnIndex = 1,
                },
                new ProjectedProducersHeader
                {
                    Name = "Glass Breakdown",
                    ColumnIndex = 2,
                },
            };
        }

        private IEnumerable<ProjectedProducersHeader> GetH2ProjectedProducersColumnHeaders()
        {
            var columnHeaders = new List<ProjectedProducersHeader>
                {
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.ProducerId },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.SubsidiaryId },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.Level },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.SubmissionPeriodCode },
                };

            var hhAndPbColumns = new List<ProjectedProducersHeader>{
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdPackagingTonnage },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdRedTonnage },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdAmberTonnage },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdGreenTonnage },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdRedMedicalTonnage },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdAmberMedicalTonnage },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdGreenMedicalTonnage },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdTonnageWithoutRAMDefaultedToRed },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinPackagingTonnage },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinRedTonnage },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinAmberTonnage },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinGreenTonnage },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinRedMedicalTonnage },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinAmberMedicalTonnage },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinGreenMedicalTonnage },
                    new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.PublicBinTonnageWithoutRAMDefaultedToRed }
                };

            var glassColumns = new List<ProjectedProducersHeader>{
                        new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersPackagingTonnage },
                        new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedTonnage },
                        new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberTonnage },
                        new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenTonnage },
                        new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersRedMedicalTonnage },
                        new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersAmberMedicalTonnage },
                        new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersGreenMedicalTonnage },
                        new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.HouseholdDrinksContainersTonnageWithoutRAMDefaultedToRed }
                };

            var totalTonnageColumn = new ProjectedProducersHeader { Name = CalcResultProjectedProducersHeaders.TotalTonnage };

            return columnHeaders.Concat(hhAndPbColumns).Append(totalTonnageColumn).Concat(hhAndPbColumns).Concat(glassColumns).Append(totalTonnageColumn);
        }
    }
}