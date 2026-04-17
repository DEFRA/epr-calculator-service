using System.Globalization;
namespace EPR.Calculator.Service.Function.UnitTests.Builder.ProjectedProducers
{
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Data.Models;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Mappers;
    using EPR.Calculator.Service.Function.Misc;

    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.Services;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using static EPR.Calculator.Service.Function.UnitTests.Builder.CalcRunLaDisposalCostBuilderTests;

    using System.Linq;
    using System.Runtime.InteropServices;
    using EPR.Calculator.Service.Function.Models.JsonExporter;

    [TestClass]
    public class CalcResultProjectedProducersBuilderTest
    {
        private readonly ApplicationDBContext dbContext;
        private CalcResultProjectedProducersBuilder builder;

        private void PrepareData()
        {
            dbContext.Material.AddRange(
                new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                new Material { Id = 2, Code = "GL", Name = "Glass", Description = "Glass" },
                new Material { Id = 3, Code = "OT", Name = "Other materials", Description = "Other materials" }
            );

            foreach (var run in new[] { 1, 2 }) {
                dbContext.CalculatorRuns.Add(new CalculatorRun
                {
                    Id = run,
                    RelativeYear = new RelativeYear(2026),
                    Name = "Run " + run
                });

                var prod11 = new ProducerDetail
                {
                    CalculatorRunId = run,
                    ProducerId = 11,
                    SubsidiaryId = null,
                    ProducerName = "Producer 11 - Parent",
                };
                dbContext.ProducerDetail.Add(prod11);

                var prod22 = new ProducerDetail
                {
                    CalculatorRunId = run,
                    ProducerId = 11,
                    SubsidiaryId = "22",
                    ProducerName = "Producer 11 - Sub 22",
                };
                dbContext.ProducerDetail.Add(prod22);

                var prod33 = new ProducerDetail
                {
                    CalculatorRunId = run,
                    ProducerId = 33,
                    SubsidiaryId = null,
                    ProducerName = "Producer 33 - No subs",
                };
                dbContext.ProducerDetail.Add(prod33);

                var prod44 = new ProducerDetail
                {
                    CalculatorRunId = run,
                    ProducerId = 44,
                    SubsidiaryId = "444",
                    ProducerName = "Producer 44 - Sub 444 - No parent",
                };
                dbContext.ProducerDetail.Add(prod44);

                foreach (var subPeriod in new[] { "2025-H1", "2025-H2"}) {
                    dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        ProducerDetailId = prod11.Id,
                        MaterialId = 1,
                        PackagingType = "HH",
                        PackagingTonnage = 100,
                        PackagingTonnageRed = 30,
                        PackagingTonnageRedMedical = 40,
                        PackagingTonnageAmber = 40,
                        PackagingTonnageAmberMedical = 0,
                        PackagingTonnageGreen = null,
                        PackagingTonnageGreenMedical = null,
                        SubmissionPeriod = subPeriod,
                    });
                    dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        ProducerDetailId = prod22.Id,
                        MaterialId = 1,
                        PackagingType = "HH",
                        PackagingTonnage = 500,
                        PackagingTonnageRed = null,
                        PackagingTonnageRedMedical = null,
                        PackagingTonnageAmber = null,
                        PackagingTonnageAmberMedical = null,
                        PackagingTonnageGreen = null,
                        PackagingTonnageGreenMedical = null,
                        SubmissionPeriod = subPeriod,
                    });
                    dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        ProducerDetailId = prod22.Id,
                        MaterialId = 2,
                        PackagingType = "HDC",
                        PackagingTonnage = 500,
                        PackagingTonnageRed = 100,
                        PackagingTonnageRedMedical = null,
                        PackagingTonnageAmber = null,
                        PackagingTonnageAmberMedical = null,
                        PackagingTonnageGreen = null,
                        PackagingTonnageGreenMedical = null,
                        SubmissionPeriod = subPeriod,
                    });
                    dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        ProducerDetailId = prod33.Id,
                        MaterialId = 1,
                        PackagingType = "PB",
                        PackagingTonnage = 150,
                        PackagingTonnageRed = 10,
                        PackagingTonnageRedMedical = 40,
                        PackagingTonnageAmber = 20,
                        PackagingTonnageAmberMedical = 30,
                        PackagingTonnageGreen = 5,
                        PackagingTonnageGreenMedical = 45,
                        SubmissionPeriod = subPeriod,
                    });
                    dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        ProducerDetailId = prod44.Id,
                        MaterialId = 1,
                        PackagingType = "HH",
                        PackagingTonnage = 150,
                        SubmissionPeriod = subPeriod,
                    });
                }
            }

            dbContext.SaveChanges();
        }

        public CalcResultProjectedProducersBuilderTest()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: "PayCal")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            builder = new CalcResultProjectedProducersBuilder(dbContext);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (dbContext != null)
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Dispose();
            }
        }

        private CalcResultsRequestDto insertData(string[][] given)
        {
            for (int i = 0; i < given.GetLength(0); i++)
            {
                var entry = given[i];
                if (entry.Length != 10)
                    throw new Exception($"Supplied test data should have length 10 - entry {i} had length {entry.Length}.");
            }

            var PeriodI = 0;
            var MaterialCodeI = 1;
            var PackagingTypeI = 2;
            var TotalTonnageI = 3;
            var RTonnageI = 4;
            var RMTonnageI = 5;
            var ATonnageI = 6;
            var AMTonnageI = 7;
            var GTonnageI = 8;
            var GMTonnageI = 9;

            var runId = 1;
            var relativeYear = new RelativeYear(2026);
            dbContext.CalculatorRuns.Add(new CalculatorRun
            {
                Id = runId,
                RelativeYear = relativeYear,
                Name = "Run " + runId
            });

            var materials = given
                   .Select(row => row[MaterialCodeI])
                   .Distinct()
                   .Select((m, i) => new Material { Id = i + 1, Code = m, Name = m, Description = m })
                   .ToArray();

            dbContext.Material.AddRange(materials);

            var producerId = 1; //TODO get from input array (and subsidiaries)
            var prod11 = new ProducerDetail
            {
                CalculatorRunId = runId,
                ProducerId = producerId,
                SubsidiaryId = null,
                ProducerName = $"Producer {producerId} - Parent",
            };
            dbContext.ProducerDetail.Add(prod11);

            decimal? ToDecimal(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return null;
                return decimal.Parse(s, CultureInfo.InvariantCulture);
            }

            foreach (var entry in given)
            {
                var (period, materialCode, packagingType, totalTonnage, rTonnage, rmTonnage, aTonnage, amTonnage, gTonnage, gmTonnage)
                    = (entry[PeriodI], entry[MaterialCodeI], entry[PackagingTypeI], entry[TotalTonnageI], entry[RTonnageI], entry[RMTonnageI], entry[ATonnageI], entry[AMTonnageI], entry[GTonnageI], entry[GMTonnageI]);
                dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    ProducerDetailId = prod11.Id, // TODO lookup
                    MaterialId = 1, // TODO lookup
                    PackagingType = packagingType,
                    PackagingTonnage = ToDecimal(totalTonnage) ?? throw new InvalidOperationException("Total tonnage cannot be blank"),
                    PackagingTonnageRed = ToDecimal(rTonnage),
                    PackagingTonnageRedMedical = ToDecimal(rmTonnage),
                    PackagingTonnageAmber = ToDecimal(aTonnage),
                    PackagingTonnageAmberMedical = ToDecimal(amTonnage),
                    PackagingTonnageGreen = ToDecimal(gTonnage),
                    PackagingTonnageGreenMedical = ToDecimal(gmTonnage),
                    SubmissionPeriod = period
                });
            }


            dbContext.SaveChanges();

            return new CalcResultsRequestDto { RunId = runId, RelativeYear = relativeYear };
        }

        private string[][] convertResult(CalcResultProjectedProducers given)
        {
            var PeriodI = 0;
            var MaterialCodeI = 1;
            var PackagingTypeI = 2;
            var TotalTonnageI = 3;
            var RTonnageI = 4;
            var RMTonnageI = 5;
            var ATonnageI = 6;
            var AMTonnageI = 7;
            var GTonnageI = 8;
            var GMTonnageI = 9;

            var result = new List<string[]>();

            string[]? createH1Row(RAMTonnage? projectedRamTonnage, string submissonPeriodCode, string materialCode, string packagingType)
            {
                if (projectedRamTonnage == null || projectedRamTonnage.Tonnage == 0)
                    return null;
                else
                {
                    var row = new string[10];
                    row[PeriodI] = submissonPeriodCode;
                    row[MaterialCodeI] = materialCode;
                    row[PackagingTypeI] = packagingType;
                    row[TotalTonnageI] = (projectedRamTonnage?.Tonnage ?? 0m).ToString();
                    row[RTonnageI] = (projectedRamTonnage?.RedTonnage ?? 0m).ToString();
                    row[RMTonnageI] = (projectedRamTonnage?.RedMedicalTonnage ?? 0m).ToString();
                    row[ATonnageI] = (projectedRamTonnage?.AmberTonnage ?? 0m).ToString();
                    row[AMTonnageI] = (projectedRamTonnage?.AmberMedicalTonnage ?? 0m).ToString();
                    row[GTonnageI] = (projectedRamTonnage?.GreenTonnage ?? 0m).ToString();
                    row[GMTonnageI] = (projectedRamTonnage?.GreenMedicalTonnage ?? 0m).ToString();
                    return row;
                }
            }

            string[]? createH2Row(RAMTonnage? originalRamTonnage, decimal? redRamTonnage, string submissonPeriodCode, string materialCode, string packagingType)
            {
                if (originalRamTonnage == null || originalRamTonnage.Tonnage == 0)
                    return null;
                else
                {
                    var row = new string[10];
                    row[PeriodI] = submissonPeriodCode;
                    row[MaterialCodeI] = materialCode;
                    row[PackagingTypeI] = packagingType;
                    row[TotalTonnageI] = (originalRamTonnage?.Tonnage ?? 0m).ToString();
                    row[RTonnageI] = ((originalRamTonnage?.RedTonnage ?? 0m) + (redRamTonnage ?? 0m)).ToString();
                    row[RMTonnageI] = (originalRamTonnage?.RedMedicalTonnage ?? 0m).ToString();
                    row[ATonnageI] = (originalRamTonnage?.AmberTonnage ?? 0m).ToString();
                    row[AMTonnageI] = (originalRamTonnage?.AmberMedicalTonnage ?? 0m).ToString();
                    row[GTonnageI] = (originalRamTonnage?.GreenTonnage ?? 0m).ToString();
                    row[GMTonnageI] = (originalRamTonnage?.GreenMedicalTonnage ?? 0m).ToString();
                    return row;
                }
            }

            if (given.H1ProjectedProducers != null)
                foreach (var producer in given.H1ProjectedProducers)
                {
                    var producerId = producer.ProducerId;
                    var submissionPeriod = producer.SubmissionPeriodCode;

                    foreach (var kv in producer.ProjectedTonnageByMaterial)
                    {
                        var materialCode = kv.Key;
                        var v = kv.Value;
                        var hhRow = createH1Row(v.ProjectedHouseholdRAMTonnage, producer.SubmissionPeriodCode, materialCode, "HH");
                        if (hhRow != null) result.Add(hhRow);
                        var pbRow = createH1Row(v.ProjectedPublicBinRAMTonnage, producer.SubmissionPeriodCode, materialCode, "PB");
                        if (pbRow != null) result.Add(pbRow);
                        var hdcRow = createH1Row(v.ProjectedHouseholdDrinksContainerRAMTonnage, producer.SubmissionPeriodCode, materialCode, "HDC");
                        if (hdcRow != null) result.Add(hdcRow);
                    }
                }

            if (given.H2ProjectedProducers != null)
                foreach (var producer in given.H2ProjectedProducers)
                {
                    var producerId = producer.ProducerId;
                    var submissionPeriod = producer.SubmissionPeriodCode;

                    foreach (var kv in producer.ProjectedTonnageByMaterial)
                    {
                        var materialCode = kv.Key;
                        var v = kv.Value;
                        var hhRow = createH2Row(v.HouseholdRAMTonnage, v.HouseholdTonnageDefaultedRed, producer.SubmissionPeriodCode, materialCode, "HH");
                        if (hhRow != null) result.Add(hhRow);
                        var pbRow = createH2Row(v.PublicBinRAMTonnage, v.PublicBinTonnageDefaultedRed, producer.SubmissionPeriodCode, materialCode, "PB");
                        if (pbRow != null) result.Add(pbRow);
                        var hdcRow = createH2Row(v.HouseholdDrinksContainerRAMTonnage, v.HouseholdDrinksContainerDefaultedRed, producer.SubmissionPeriodCode, materialCode, "HDC");
                        if (hdcRow != null) result.Add(hdcRow);
                    }
                }

            return result.OrderBy(a => a[PeriodI] + a[MaterialCodeI] + a[PackagingTypeI]).ToArray();
        }

        private async Task<string[][]> fillGaps(string[][] given) =>
            convertResult(await builder.ConstructAsync(insertData(given)));

        string ToPrintable(string[] arr) =>
          arr is null ? "null" : "[" + string.Join(", ", arr.Select(x => x?.ToString() ?? "null")) + "]";

        string ToPrintableArray(string[][] arr) =>
            arr is null ? "null" : "[\n" + string.Join("\n", arr.Select(x => ToPrintable(x))) + "\n]";

        private void assert(string[][] expected, string[][] actual)
        {
            var data = $"Arrays differ. expected={ToPrintableArray(expected)} actual={ToPrintableArray(actual)}";

            Assert.AreEqual(expected.Length, actual.Length, $"Results array sizes differ\n{data}");

            for (int i = 0; i < expected.Length; i++)
            {
                var expRow = expected[i] ?? Array.Empty<string>();
                var actRow = actual[i] ?? Array.Empty<string>();

                Assert.AreEqual(expRow.Length, actRow.Length, $"Result entry array differ at row {i}\n{data}");

                for (int j = 0; j < expRow.Length; j++)
                {
                    var exp = expRow[j];
                    var act = actRow[j];
                    Assert.AreEqual(exp, act, $"Mismatch at [{i},{j}]: expected '{exp}' but was '{act}'.\n{data}");
                }
            }
        }

        // Could move to CSV
        // format: SubmissionPeriod, Material, PackagingType, TotalTonnage, RTonnage, RMTonnage, ATonnage, AMTonnage, GTonnage, GMTonnage

        [TestMethod]
        public async Task H1H2Projection_untouched()
        {
            // RAM is complete - no modifications made
            var given = new[] {
                new[] { "2025-H1", "AL", "HH", "100", "30", "40", "40", "0", "", "" },
                new[] { "2025-H2", "AL", "HH", "100", "30", "40", "40", "0", "", "" }
            };
            var expected = new [] {
                new[] { "2025-H1", "AL", "HH", "100", "30", "40", "40", "0", "0", "0" },
                new[] { "2025-H2", "AL", "HH", "100", "30", "40", "40", "0", "0", "0" }
            };
            assert(expected, await fillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_untouched_onlyh2()
        {
            // RAM is complete - only H2 - no modifications made
            var given = new[] {
                new[] { "2025-H2", "AL", "HH", "100", "30", "40", "40", "0", "", "" }
            };
            var expected = new [] {
                new[] { "2025-H2", "AL", "HH", "100", "30", "40", "40", "0", "0", "0" }
            };
            assert(expected, await fillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_onlyh2_incomplete()
        {
            // Incomplete H2 - inferred as Red
            var given = new[] {
                new[] { "2025-H2", "AL", "HH", "100", "", "", "", "", "", "" }
            };
            var expected = new [] {
                new[] { "2025-H2", "AL", "HH", "100", "100", "0", "0", "0", "0", "0" }
            };
            assert(expected, await fillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_onlyh2_partial()
        {
            // Partial H2 - inferred as Red
            var given = new[] {
                new[] { "2025-H2", "AL", "HH", "100", "50", "", "", "", "", "" }
            };
            var expected = new [] {
                new[] { "2025-H2", "AL", "HH", "100", "100", "0", "0", "0", "0", "0" }
            };
            assert(expected, await fillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_onlyh2_partial2()
        {
            // Partial H2 - inferred as Red
            var given = new[] {
                new[] { "2025-H2", "AL", "HH", "100", "", "50", "", "", "", "" }
            };
            var expected = new [] {
                new[] { "2025-H2", "AL", "HH", "100", "50", "50", "0", "0", "0", "0" }
            };
            assert(expected, await fillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_incomplete_h1()
        {
            // Incomplete H1 - reflects proportions from H2
            var given = new[] {
                new[] { "2025-H1", "AL", "HH", "42", "", "", "", "", "", "" },
                new[] { "2025-H2", "AL", "HH", "21", "6", "5", "4", "3", "2", "1" }
            };
            var expected = new [] {
                // TODO review this
                //new[] { "2025-H1", "AL", "HH", "42", "12", "10", "8", "6", "4", "2" },
                new[] { "2025-H1", "AL", "HH", "42", "11.999988", "9.999990", "7.999992", "5.999994", "3.999996", "1.999998" },
                new[] { "2025-H2", "AL", "HH", "21", "6", "5", "4", "3", "2", "1" }
            };
            assert(expected, await fillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_partial_h1()
        {
            // Incomplete H1 - reflects proportions from H2, perserving any H1
            var given = new[] {
                new[] { "2025-H1", "AL", "HH", "43", "1", "", "", "", "", "" },
                new[] { "2025-H2", "AL", "HH", "21", "6", "5", "4", "3", "2", "1" }
            };
            var expected = new [] {
                // TODO review this
                //new[] { "2025-H1", "AL", "HH", "42", "13", "10", "8", "6", "4", "2" },
                new[] { "2025-H1", "AL", "HH", "43", "12.999988", "9.999990", "7.999992", "5.999994", "3.999996", "1.999998" },
                new[] { "2025-H2", "AL", "HH", "21", "6", "5", "4", "3", "2", "1" }
            };
            assert(expected, await fillGaps(given));
        }

        [TestMethod]
        public async Task Construct_WorksCorrectly()
        {
            // Arrange
            PrepareData();
            var requestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2026) };

            // Act
            var result = await builder.ConstructAsync(requestDto);

            // Assert H2
            Assert.IsNotNull(result.H2ProjectedProducersHeaders);
            Assert.AreEqual(6, result.H2ProjectedProducers!.Count());

            var h2Prod11Subtotal = result.H2ProjectedProducers!.First(p => p.ProducerId == 11 && p.SubsidiaryId == null && p.IsSubtotal == true && p.Level == CommonConstants.LevelOne.ToString() && p.SubmissionPeriodCode == "2025-H2");
            var h2Prod11Parent = result.H2ProjectedProducers!.First(p => p.ProducerId == 11 && p.SubsidiaryId == null && p.IsSubtotal == false && p.Level == CommonConstants.LevelTwo.ToString() && p.SubmissionPeriodCode == "2025-H2");
            var h2Prod11Subsidiary = result.H2ProjectedProducers!.First(p => p.ProducerId == 11 && p.SubsidiaryId == "22" && p.IsSubtotal == false && p.Level == CommonConstants.LevelTwo.ToString() && p.SubmissionPeriodCode == "2025-H2");
            var h2Prod33Individual = result.H2ProjectedProducers!.First(p => p.ProducerId == 33 && p.SubsidiaryId == null && p.IsSubtotal == false && p.Level == CommonConstants.LevelOne.ToString() && p.SubmissionPeriodCode == "2025-H2");
            var h2Prod44Subtotal = result.H2ProjectedProducers!.First(p => p.ProducerId == 44 && p.SubsidiaryId == null && p.IsSubtotal == true && p.Level == CommonConstants.LevelOne.ToString() && p.SubmissionPeriodCode == "2025-H2");
            var h2Prod44IndividualSub = result.H2ProjectedProducers!.First(p => p.ProducerId == 44 && p.SubsidiaryId == "444" && p.IsSubtotal == false && p.Level == CommonConstants.LevelTwo.ToString() && p.SubmissionPeriodCode == "2025-H2");

            Assert.AreEqual(
                h2Prod11Parent.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].HouseholdRAMTonnage.AmberTonnage + h2Prod11Subsidiary.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].HouseholdRAMTonnage.AmberTonnage,
                h2Prod11Subtotal.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].HouseholdRAMTonnage.AmberTonnage
            );
            Assert.AreEqual(
                h2Prod11Parent.ProjectedTonnageByMaterial[MaterialCodes.Glass].HouseholdDrinksContainerRAMTonnage?.RedTonnage + h2Prod11Subsidiary.ProjectedTonnageByMaterial[MaterialCodes.Glass].HouseholdDrinksContainerRAMTonnage?.RedTonnage,
                h2Prod11Subtotal.ProjectedTonnageByMaterial[MaterialCodes.Glass].HouseholdDrinksContainerRAMTonnage?.RedTonnage
            );
            Assert.AreEqual(
                h2Prod44IndividualSub.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].HouseholdRAMTonnage.Tonnage,
                h2Prod44Subtotal.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].HouseholdRAMTonnage.Tonnage
            );

            // Assert H1
            Assert.IsNotNull(result.H1ProjectedProducersHeaders);
            Assert.AreEqual(6, result.H1ProjectedProducers!.Count());

            var h1Prod11Subtotal = result.H1ProjectedProducers!.First(p => p.ProducerId == 11 && p.SubsidiaryId == null && p.IsSubtotal == true && p.Level == CommonConstants.LevelOne.ToString() && p.SubmissionPeriodCode == "2025-H1");
            var h1Prod11Parent = result.H1ProjectedProducers!.First(p => p.ProducerId == 11 && p.SubsidiaryId == null && p.IsSubtotal == false && p.Level == CommonConstants.LevelTwo.ToString() && p.SubmissionPeriodCode == "2025-H1");
            var h1Prod11Subsidiary = result.H1ProjectedProducers!.First(p => p.ProducerId == 11 && p.SubsidiaryId == "22" && p.IsSubtotal == false && p.Level == CommonConstants.LevelTwo.ToString() && p.SubmissionPeriodCode == "2025-H1");
            var h1Prod33Individual = result.H1ProjectedProducers!.First(p => p.ProducerId == 33 && p.SubsidiaryId == null && p.IsSubtotal == false && p.Level == CommonConstants.LevelOne.ToString() && p.SubmissionPeriodCode == "2025-H1");
            var h1Prod44Subtotal = result.H1ProjectedProducers!.First(p => p.ProducerId == 44 && p.SubsidiaryId == null && p.IsSubtotal == true && p.Level == CommonConstants.LevelOne.ToString() && p.SubmissionPeriodCode == "2025-H1");
            var h1Prod44IndividualSub = result.H1ProjectedProducers!.First(p => p.ProducerId == 44 && p.SubsidiaryId == "444" && p.IsSubtotal == false && p.Level == CommonConstants.LevelTwo.ToString() && p.SubmissionPeriodCode == "2025-H1");

            var h1h1Prod11ParentAlm = h1Prod11Parent.ProjectedTonnageByMaterial[MaterialCodes.Aluminium];
            var h1Prod11SubsidiaryAlm = h1Prod11Subsidiary.ProjectedTonnageByMaterial[MaterialCodes.Aluminium];

            Assert.AreEqual(
                ((h1h1Prod11ParentAlm.H2RamProportions.Red * h1h1Prod11ParentAlm.H2TotalTonnage) + (h1Prod11SubsidiaryAlm.H2RamProportions.Red * h1Prod11SubsidiaryAlm.H2TotalTonnage)) / (h1h1Prod11ParentAlm.H2TotalTonnage + h1Prod11SubsidiaryAlm.H2TotalTonnage),
                h1Prod11Subtotal.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].H2RamProportions.Red
            );
            Assert.AreEqual(
                h1Prod11Parent.ProjectedTonnageByMaterial[MaterialCodes.Glass].HouseholdDrinksContainerTonnageWithoutRAM + h1Prod11Subsidiary.ProjectedTonnageByMaterial[MaterialCodes.Glass].HouseholdDrinksContainerTonnageWithoutRAM,
                h1Prod11Subtotal.ProjectedTonnageByMaterial[MaterialCodes.Glass].HouseholdDrinksContainerTonnageWithoutRAM
            );
            Assert.AreEqual(
                h1Prod44IndividualSub.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].ProjectedHouseholdRAMTonnage.Tonnage,
                h1Prod44Subtotal.ProjectedTonnageByMaterial[MaterialCodes.Aluminium].ProjectedHouseholdRAMTonnage.Tonnage
            );
        }
    }
}
