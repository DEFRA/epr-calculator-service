using System.Globalization;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
namespace EPR.Calculator.Service.Function.UnitTests.Builder.ProjectedProducers
{
    using System.Reflection.Metadata.Ecma335;
    using AutoFixture;
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
    using Microsoft.Azure.Amqp.Framing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using static EPR.Calculator.Service.Function.UnitTests.Builder.CalcRunLaDisposalCostBuilderTests;

    [TestClass]
    public class CalcResultProjectedProducersBuilderTest
    {
        private readonly ApplicationDBContext dbContext;
        private CalcResultProjectedProducersBuilder builder;

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

        private int ProducerI = 0;
        private int SubsidiaryI = 1;
        private int PeriodI = 2;
        private int LevelI = 3;
        private int MaterialCodeI = 4;
        private int PackagingTypeI = 5;
        private int TotalTonnageI = 6;
        private int RTonnageI = 7;
        private int RMTonnageI = 8;
        private int ATonnageI = 9;
        private int AMTonnageI = 10;
        private int GTonnageI = 11;
        private int GMTonnageI = 12;

        // Could move to CSV
        // Format: ProducerId, SubsidiaryId, SubmissionPeriod, Level, Material, PackagingType, TotalTonnage, RTonnage, RMTonnage, ATonnage, AMTonnage, GTonnage, GMTonnage

        [TestMethod]
        public async Task H1H2Projection_untouched()
        {
            // RAM is complete - no modifications made
            var given = new[] {
                new[] { "101", "", "2025-H1", "", "AL", "HH", "100", "20", "40", "40", "0", "", "" },
                new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "20", "40", "40", "0", "", "" }
            };
            var expected = new [] {
                new[] { "101", "", "2025-H1", "1", "AL", "HH", "100", "20.0", "40.0", "40.0", "0", "0", "0" },
                new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "20", "40", "40", "0", "0", "0" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_untouched_onlyh2()
        {
            // RAM is complete - only H2 - no modifications made
            var given = new[] {
                new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "30", "40", "40", "0", "", "" }
            };
            var expected = new [] {
                new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "30", "40", "40", "0", "0", "0" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_untouched_onlyh1()
        {
            // RAM is complete - only H1 - no modifications made
            var given = new[] {
                new[] { "101", "", "2025-H1", "", "AL", "HH", "100", "30", "40", "40", "0", "", "" }
            };
            var expected = new [] {
                new[] { "101", "", "2025-H1", "1", "AL", "HH", "100", "30", "40", "40", "0", "0", "0" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_untouched_hc()
        {
            // RAM is complete - no modifications made
            var given = new[] {
                new[] { "101", "", "2025-H1", "", "AL", "HH", "100", "20", "40", "40", "0", "", "" },
                new[] { "101", "A", "2025-H1", "", "GL", "HDC", "100", "20", "40", "40", "0", "", "" },
                new[] { "101", "B", "2025-H1", "", "AL", "PB", "100", "20", "40", "40", "0", "", "" },
                new[] { "101", "", "2025-H2", "", "PL", "HH", "100", "20", "40", "40", "0", "", "" },
                new[] { "101", "A", "2025-H2", "", "ST", "PB", "100", "20", "40", "40", "0", "", "" },
                new[] { "101", "B", "2025-H2", "", "AL", "HH", "100", "20", "40", "40", "0", "", "" }
            };
            var expected = new string[][]
            {
                new string[] { "101", "", "2025-H1", "1", "AL", "HH", "100", "20.0", "40.0", "40.0", "0", "0", "0" },
                new string[] { "101", "", "2025-H1", "1", "AL", "PB", "100", "20.0", "40.0", "40.0", "0", "0", "0" },
                new string[] { "101", "", "2025-H1", "1", "GL", "HDC", "100", "20", "40", "40", "0", "0", "0" },
                new string[] { "101", "", "2025-H1", "2", "AL", "HH", "100", "20.0", "40.0", "40.0", "0", "0", "0" },
                new string[] { "101", "B", "2025-H1", "2", "AL", "PB", "100", "20.0", "40.0", "40.0", "0", "0", "0" },
                new string[] { "101", "A", "2025-H1", "2", "GL", "HDC", "100", "20", "40", "40", "0", "0", "0" },
                new string[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "20", "40", "40", "0", "0", "0" },
                new string[] { "101", "", "2025-H2", "1", "PL", "HH", "100", "20", "40", "40", "0", "0", "0" },
                new string[] { "101", "", "2025-H2", "1", "ST", "PB", "100", "20", "40", "40", "0", "0", "0" },
                new string[] { "101", "", "2025-H2", "2", "PL", "HH", "100", "20", "40", "40", "0", "0", "0" },
                new string[] { "101", "B", "2025-H2", "2", "AL", "HH", "100", "20", "40", "40", "0", "0", "0" },
                new string[] { "101", "A", "2025-H2", "2", "ST", "PB", "100", "20", "40", "40", "0", "0", "0" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_onlyh2_incomplete()
        {
            // Incomplete H2 - inferred as Red
            var given = new[] {
                new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "", "", "", "", "", "" }
            };
            var expected = new [] {
                new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "100", "0", "0", "0", "0", "0" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_onlyh2_partial()
        {
            // Partial H2 - inferred as Red
            var given = new[] {
                new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "50", "", "", "", "", "" }
            };
            var expected = new [] {
                new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "100", "0", "0", "0", "0", "0" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_onlyh2_partial2()
        {
            // Partial H2 - inferred as Red
            var given = new[] {
                new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "", "50", "", "", "", "" }
            };
            var expected = new [] {
                new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "50", "50", "0", "0", "0", "0" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_incomplete_h1()
        {
            // Incomplete H1 - reflects proportions from H2
            var given = new[] {
                new[] { "101", "", "2025-H1", "", "AL", "HH", "42", "", "", "", "", "", "" },
                new[] { "101", "", "2025-H2", "", "AL", "HH", "21", "6", "5", "4", "3", "2", "1" }
            };
            var expected = new [] {
                new[] { "101", "", "2025-H1", "1", "AL", "HH", "42", "12.000", "10.000", "8.000", "6.000", "4.000", "2.000" },
                new[] { "101", "", "2025-H2", "1", "AL", "HH", "21", "6", "5", "4", "3", "2", "1" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_partial_h1()
        {
            // Incomplete H1 - reflects proportions from H2, perserving any H1
            var given = new[] {
                new[] { "101", "", "2025-H1", "", "AL", "HH", "43", "1", "", "", "", "", "" },
                new[] { "101", "", "2025-H2", "", "AL", "HH", "21", "6", "5", "4", "3", "2", "1" }
            };
            var expected = new [] {
                new[] { "101", "", "2025-H1", "1", "AL", "HH", "43", "13.000", "10.000", "8.000", "6.000", "4.000", "2.000" },
                new[] { "101", "", "2025-H2", "1", "AL", "HH", "21", "6", "5", "4", "3", "2", "1" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_partial_h2_and_partial_h1()
        {
            // Incomplete H2 - defaults some to red
            // Incomplete H1 - reflects proportions from H2, perserving any H1
            var given = new[] {
                new[] { "101", "", "2025-H1", "", "AL", "HH", "43", "", "", "3", "", "", "" },
                new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "50", "", "", "", "", "" }
            };
            var expected = new [] {
                new[] { "101", "", "2025-H1", "1", "AL", "HH", "43", "40", "0", "3", "0", "0", "0" },
                new[] { "101", "", "2025-H2", "1", "AL", "HH", "100", "100", "0", "0", "0", "0", "0" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_partial_h1_no_h2()
        {
            // Incomplete H1 - cannot reflect proportions from H2, defaults remaining tonnage to red
            var given = new[] {
                new[] { "101", "", "2025-H1", "", "AL", "HH", "100", "", "", "10", "", "", "" }
            };
            var expected = new [] {
                new[] { "101", "", "2025-H1", "1", "AL", "HH", "100", "90", "0", "10", "0", "0", "0" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_subtotal_hg_h1_noh2()
        {
            // Level 1 subtotal added for parent who reports for themselves too
            var given = new[] {
                new[] { "101", "", "2025-H1", "", "PL", "PB", "100", "", "10", "10", "", "", "" },
                new[] { "101", "A", "2025-H1", "", "PL", "PB", "200", "", "", "20", "", "", "20" }
            };
            var expected = new [] {
                new[] { "101", "", "2025-H1", "1", "PL", "PB", "300", "240", "10", "30", "0", "0", "20" },
                new[] { "101", "", "2025-H1", "2", "PL", "PB", "100", "80", "10", "10", "0", "0", "0" },
                new[] { "101", "A", "2025-H1", "2", "PL", "PB", "200", "160", "0", "20", "0", "0", "20" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_subtotal_hg_indv()
        {
            // Part of holding group but subsidiary reports individually - expected subtotal row for parent
            var given = new[] {
                new[] { "101", "A", "2025-H1", "", "GL", "HDC", "43", "1", "", "", "", "", "" },
                new[] { "101", "A", "2025-H2", "", "GL", "HDC", "21", "6", "5", "4", "3", "2", "1" }
            };
            var expected = new [] {
                new[] { "101", "", "2025-H1", "1", "GL", "HDC", "43", "13.000", "10.000", "8.000", "6.000", "4.000", "2.000" },
                new[] { "101", "A", "2025-H1", "2", "GL", "HDC", "43", "13.000", "10.000", "8.000", "6.000", "4.000", "2.000" },
                new[] { "101", "", "2025-H2", "1", "GL", "HDC", "21", "6", "5", "4", "3", "2", "1" },
                new[] { "101", "A", "2025-H2", "2", "GL", "HDC", "21", "6", "5", "4", "3", "2", "1" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_subtotal_hg_no_parent_report_multi_materials()
        {
            // Part of holding group where holding group doesn't report for themselves - different materials
            var given = new[] {
                new[] { "101", "A", "2025-H1", "", "ST", "HH", "100", "20",  "20", "40",  "0",  "", "" },
                new[] { "101", "A", "2025-H2", "", "AL", "HH", "100", "20",  "40", "40",  "0",  "", "" },
                new[] { "101", "A", "2025-H1", "", "PL", "PB", "100", "20",  "40", "40",  "0",  "", "" },
                new[] { "101", "A", "2025-H2", "", "PL", "PB", "100", "20",  "40", "40",  "0",  "", "" },
                new[] { "101", "B", "2025-H1", "", "ST", "HH", "200", "50",  "50", "100", "0",  "", "" },
                new[] { "101", "B", "2025-H2", "", "AL", "HH", "200", "150", "25", "25",  "0",  "", "" }
            };
            var expected = new[]  {
                new[] { "101", "",  "2025-H1", "1", "PL", "PB", "100", "20.0", "40.0", "40.0", "0", "0", "0" },
                new[] { "101", "",  "2025-H1", "1", "ST", "HH", "300", "90",   "70",   "140",  "0", "0", "0" },
                new[] { "101", "A", "2025-H1", "2", "PL", "PB", "100", "20.0", "40.0", "40.0", "0", "0", "0" },
                new[] { "101", "A", "2025-H1", "2", "ST", "HH", "100", "40",   "20",   "40",   "0", "0", "0" },
                new[] { "101", "B", "2025-H1", "2", "ST", "HH", "200", "50",   "50",   "100",  "0", "0", "0" },

                new[] { "101", "",  "2025-H2", "1", "AL", "HH", "300", "170",  "65",   "65",   "0", "0", "0" },
                new[] { "101", "",  "2025-H2", "1", "PL", "PB", "100", "20",   "40",   "40",   "0", "0", "0" },
                new[] { "101", "A", "2025-H2", "2", "AL", "HH", "100", "20",   "40",   "40",   "0", "0", "0" },
                new[] { "101", "B", "2025-H2", "2", "AL", "HH", "200", "150",  "25",   "25",   "0", "0", "0" },
                new[] { "101", "A", "2025-H2", "2", "PL", "PB", "100", "20",   "40",   "40",   "0", "0", "0" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        [TestMethod]
        public async Task H1H2Projection_h1_use_subtotal_h2_projection()
        {
            var given = new[] {
                new[] { "101", "", "2025-H2", "", "AL", "HH", "100", "20",  "40", "40",  "0",  "", "" },
                new[] { "101", "A", "2025-H2", "", "AL", "HH", "200", "20",  "40", "40",  "0",  "", "" },
                new[] { "101", "B", "2025-H2", "", "AL", "HH", "300", "150", "25", "25",  "0",  "", "" },
                new[] { "101", "", "2025-H1", "", "AL", "HH", "100", "20",  "40", "40",  "0",  "", "" },
                new[] { "101", "A", "2025-H2", "", "AL", "HH", "200", "20",  "40", "40",  "0",  "", "" },
                new[] { "101", "B", "2025-H2", "", "AL", "HH", "300", "150", "25", "25",  "0",  "", "" },
            };
            var expected = new[]  {
                new[] { "101", "", "2025-H1", "1", "AL", "HH", "100", "20.000", "40.000", "40.000", "0", "0", "0" },
                new[] { "101", "", "2025-H2", "1", "AL", "HH", "1100", "760", "170", "170", "0", "0", "0" },
                new[] { "101", "", "2025-H2", "2", "AL", "HH", "100", "20", "40", "40", "0", "0", "0" },
                new[] { "101", "A", "2025-H2", "2", "AL", "HH", "400", "240", "80", "80", "0", "0", "0" },
                new[] { "101", "B", "2025-H2", "2", "AL", "HH", "600", "500", "50", "50", "0", "0", "0" }
            };
            AssertExcepted(expected, await FillGaps(given));
        }

        private CalcResultsRequestDto InsertData(string[][] given)
        {
            for (int i = 0; i < given.GetLength(0); i++)
            {
                var entry = given[i];
                if (entry.Length != 13)
                    throw new Exception($"Supplied test data should have length 13 - entry {i} had length {entry.Length}.");
            }

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

            var producers = given
                   .Select(row => (row[ProducerI], row[SubsidiaryI]))
                   .Distinct()
                   .Select(ps => new ProducerDetail
                   {
                       CalculatorRunId = runId,
                       ProducerId = int.Parse(ps.Item1),
                       SubsidiaryId = string.IsNullOrWhiteSpace(ps.Item2) ? null : ps.Item2,
                       ProducerName = $"Producer {ps.Item1}/{ps.Item2}",
                   })
                   .ToArray();

            dbContext.ProducerDetail.AddRange(producers);
            var idByProducerId = producers.ToDictionary(p => (p.ProducerId, p.SubsidiaryId ?? ""), p => p.Id);

            decimal? ToDecimal(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return null;
                return decimal.Parse(s, CultureInfo.InvariantCulture);
            }

            foreach (var entry in given)
            {
                var (producerId, subsidiaryId, period, materialCode, packagingType, totalTonnage, rTonnage, rmTonnage, aTonnage, amTonnage, gTonnage, gmTonnage)
                    = (entry[ProducerI], entry[SubsidiaryI], entry[PeriodI], entry[MaterialCodeI], entry[PackagingTypeI], entry[TotalTonnageI], entry[RTonnageI], entry[RMTonnageI], entry[ATonnageI], entry[AMTonnageI], entry[GTonnageI], entry[GMTonnageI]);
                dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                {
                    ProducerDetailId = idByProducerId[(int.Parse(producerId), subsidiaryId)],
                    MaterialId = materials.First(m => m.Code == materialCode).Id,
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

        private string[][] ConvertResult(List<(L1, ProjectionData?)> given)
        {
            string[]? createRow(int producerId, string? subsidiaryId, string submissonPeriodCode, string materialCode, string packagingType, string? level, RAMTonnage? projectedRamTonnage)
            {
                if (projectedRamTonnage == null || projectedRamTonnage.Tonnage == 0)
                    return null;
                else
                {
                    var row = new string[13];
                    row[ProducerI] = producerId.ToString();
                    row[SubsidiaryI] = subsidiaryId ?? "";
                    row[PeriodI] = submissonPeriodCode;
                    row[LevelI] = level ?? "";
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

            RAMTonnage toRAMTonnage(ReportedData data)
            {
                return new RAMTonnage
                {
                    Tonnage = data.R + data.A + data.G + data.RM + data.AM + data.GM,
                    RedTonnage = data.R,
                    AmberTonnage = data.A,
                    GreenTonnage = data.G,
                    RedMedicalTonnage = data.RM,
                    AmberMedicalTonnage = data.AM,
                    GreenMedicalTonnage = data.GM
                };
            }

            List<string[]> populateForSingleL1(SingleL1 sl1)
            {
                var result = new List<string[]>();
                foreach (var submission in sl1.MaterialSubmissions)
                {
                    result.Add(createRow(sl1.OrgId, null, submission.SubmissionPeriod, submission.Material.Name, "HH", "L1", toRAMTonnage(submission.HH)));
                    result.Add(createRow(sl1.OrgId, null, submission.SubmissionPeriod, submission.Material.Name, "PB", "L1", toRAMTonnage(submission.PB)));
                    result.Add(createRow(sl1.OrgId, null, submission.SubmissionPeriod, submission.Material.Name, "HDC", "L1", toRAMTonnage(submission.HDC)));
                }
                return result;
            }

            List<string[]> populateForHoldingCompany(HC hc)
            {
                var result = new List<string[]>();
                foreach (var l2 in hc.L2s)
                {
                    foreach (var submission in l2.MaterialSubmissions)
                    {
                        result.Add(createRow(l2.OrgId, l2.SubsidiaryId, submission.SubmissionPeriod, submission.Material.Name, "HH", "L1", toRAMTonnage(submission.HH)));
                        result.Add(createRow(l2.OrgId, l2.SubsidiaryId, submission.SubmissionPeriod, submission.Material.Name, "PB", "L1", toRAMTonnage(submission.PB)));
                        result.Add(createRow(l2.OrgId, l2.SubsidiaryId, submission.SubmissionPeriod, submission.Material.Name, "HDC", "L1", toRAMTonnage(submission.HDC)));
                    }
                }
                return result;
            }

            var result = new List<string[]>();
            foreach (var l1 in given.Select(e => e.Item1))
            {
                result.AddRange(l1 switch
                {
                    SingleL1 sl1 => populateForSingleL1(sl1),
                    HC hc => populateForHoldingCompany(hc),
                    //_ => throw new ArgumentException("Unsupported L1 type")
                });
            }
/*
            if (given.H1ProjectedProducers != null && given.H2ProjectedProducers != null) {
                foreach (var producer in given.H1ProjectedProducers.Cast<ICalcResultProjectedProducer>().Concat(given.H2ProjectedProducers))
                {
                    var producerId = producer.ProducerId;
                    var subsidiaryId = producer.SubsidiaryId;
                    var submissionPeriod = producer.SubmissionPeriodCode;

                    foreach (var kv in producer.ProjectedTonnageByMaterial)
                    {
                        var materialCode = kv.Key;
                        var v = kv.Value;
                        var hhRow = createRow(producerId, subsidiaryId, producer.SubmissionPeriodCode, materialCode, "HH", producer.Level, v.ProjectedHouseholdRAMTonnage);
                        if (hhRow != null) result.Add(hhRow);
                        var pbRow = createRow(producerId, subsidiaryId, producer.SubmissionPeriodCode, materialCode, "PB", producer.Level, v.ProjectedPublicBinRAMTonnage);
                        if (pbRow != null) result.Add(pbRow);
                        var hdcRow = createRow(producerId, subsidiaryId, producer.SubmissionPeriodCode, materialCode, "HDC", producer.Level, v.ProjectedHouseholdDrinksContainerRAMTonnage);
                        if (hdcRow != null) result.Add(hdcRow);
                    }
                }
            }*/

            return result
                    .OrderBy(a => a[PeriodI])
                    .ThenBy(a => a[ProducerI])
                    .ThenBy(a => string.IsNullOrEmpty(a[SubsidiaryI]) ? 0 : 1)
                    .ThenBy(a => a[LevelI])
                    .ThenBy(a => a[MaterialCodeI])
                    .ThenBy(a => a[PackagingTypeI])
                    .ToArray();
        }
        private List<L1> mockProducers = new List<L1>();

        private async Task<string[][]> FillGaps(string[][] given) =>
            ConvertResult(await builder.ConstructAsync(InsertData(given), mockProducers));

        private string ToPrintable(string[] arr) =>
          arr is null ? "null" : "[" + string.Join(", ", arr.Select(x => x?.ToString() ?? "null")) + "]";

        private string ToPrintableArray(string[][] arr) =>
            arr is null ? "null" : "[\n" + string.Join("\n", arr.Select(x => ToPrintable(x))) + "\n]";

        private void AssertExcepted(string[][] expected, string[][] actual)
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
    }
}
