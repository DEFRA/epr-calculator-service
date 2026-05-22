using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Messaging;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Services.CommonDataApi;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

[TestClass]
public class CalculatorRunIntegrationTests : BaseIntegrationTest
{
    private static readonly DateTime Now = DateTime.UtcNow;

    [TestMethod]
    public async Task IntegrationTest_2025() => await RunTest("test2025", new RelativeYear(2025), "some-user");

    [TestMethod]
    public async Task IntegrationTest_2026() => await RunTest("test2026", new RelativeYear(2026), "some-user");

    private async Task RunTest(string name, RelativeYear relativeYear, String rundBy)
    {
        await using var db = await Provider
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<IDbContextFactory<ApplicationDBContext>>()
            .CreateDbContextAsync();

        var calculatorRunId = await SeedCalculatorRun(db, name, relativeYear, "IntegrationTests/TestData/defaultParams.csv", "IntegrationTests/TestData/lapcap.csv");

        var fakeCommonDataApi                   = Provider.GetRequiredService<FakeCommonDataApiClient>();
        fakeCommonDataApi.OrganisationResponses = OrganisationResponses($"IntegrationTests/TestData/{relativeYear.Value}-organisation-data.csv");
        fakeCommonDataApi.PomResponses          = PomResponses($"IntegrationTests/TestData/{relativeYear.Value}-pom-data.csv");

        var fakeBlobStorage = Provider.GetRequiredService<FakeBlobStorageService>();

        var calculatorRunResult = await Provider.GetRequiredService<ICalculatorRunService>().PrepareResultsFileAsync(
            new CreateResultFileMessage
            {
                CalculatorRunId = calculatorRunId,
                RelativeYear    = relativeYear,
                CreatedBy       = rundBy,
                MessageType     = "some-message-type"
            }, name);
        calculatorRunResult.ShouldBe(true);
        {
            var skip          = 8;
            var contents      = fakeBlobStorage.Get(ToResultsCsvFileName(calculatorRunId, name));
            var actualLines   = string.Join(Environment.NewLine, contents.Split(Environment.NewLine, StringSplitOptions.None                                     ).Skip(skip)).Trim().Split(Environment.NewLine);
            var expectedLines = string.Join(Environment.NewLine, (await File.ReadAllLinesAsync($"IntegrationTests/ExpectedData/{relativeYear.Value}-results.csv")).Skip(skip)).Trim().Split(Environment.NewLine);

            actualLines.Length.ShouldBe(expectedLines.Length, $"Results CSV\n\n{contents}");

            for (var i = 0; i < actualLines.Length; i++)
            {
                actualLines[i].ShouldBe(expectedLines[i], $"Results CSV mismatch at line {i + skip}");
            }
        }

        await SeedAcceptOrRejectProducers(db, calculatorRunId, rundBy, $"IntegrationTests/TestData/{relativeYear.Value}-accept-or-reject-producers.csv");
        var billingRunResult = await Provider.GetRequiredService<IPrepareBillingFileService>().PrepareBillingFileAsync(calculatorRunId, name, rundBy);
        billingRunResult.ShouldBe(true);
        {
            var skip          = 8;
            var contents      = fakeBlobStorage.Get(ToBillingCsvFileName(calculatorRunId, name));  // Can fail as mintues in filename - return filename from PrepareBillingFileAsync instead?
            var actualLines   = string.Join(Environment.NewLine, contents.Split(Environment.NewLine, StringSplitOptions.None                                     ).Skip(skip)).Trim().Split(Environment.NewLine);
            var expectedLines = string.Join(Environment.NewLine, (await File.ReadAllLinesAsync($"IntegrationTests/ExpectedData/{relativeYear.Value}-billing.csv")).Skip(skip)).Trim().Split(Environment.NewLine);

            actualLines.Length.ShouldBe(expectedLines.Length, $"Billing CSV\n\n{contents}");

            for (var i = 0; i < actualLines.Length; i++)
            {
                actualLines[i].ShouldBe(expectedLines[i], $"Billing CSV mismatch at line {i + skip}");
            }
        }
        {   // TODO sort json fields before comparison?
            var skip          = 19;
            var contents      = fakeBlobStorage.Get(ToBillingJsonFileName(calculatorRunId, name));
            var actualLines   = string.Join(Environment.NewLine, contents.Split(Environment.NewLine, StringSplitOptions.None                                      ).Skip(skip)).Trim().Split(Environment.NewLine);
            var expectedLines = string.Join(Environment.NewLine, (await File.ReadAllLinesAsync($"IntegrationTests/ExpectedData/{relativeYear.Value}-billing.json")).Skip(skip)).Trim().Split(Environment.NewLine);

            actualLines.Length.ShouldBe(expectedLines.Length, $"Billing JSON\n\n{contents}");

            for (var i = 0; i < actualLines.Length; i++)
            {
                actualLines[i].ShouldBe(expectedLines[i], $"Billing JSON missmatch at line {i + skip}");
            }
        }
    }

    private static string ToResultsCsvFileName(int calculatorRunId, string name) =>
        new CalcResultsAndBillingFileName(calculatorRunId, name, Now, isDraftBillingFile: false);   // $"{calculatorRunId}-{name}_Results File_{Now.ToString("yyyyMMdd")}.csv";

    private static string ToBillingCsvFileName(int calculatorRunId, string name) =>
        new CalcResultsAndBillingFileName(calculatorRunId, name, Now, isDraftBillingFile: true);    // $"{calculatorRunId}-{name}_Billing File_{Now.ToString("yyyyMMddHHmm")}.csv";

    private static string ToBillingJsonFileName(int calculatorRunId, string name) =>
        new CalcResultsAndBillingFileName(calculatorRunId, isDraftBillingFile: true, isJson: true); // $"{calculatorRunId}billing.json";

    private static CsvReader SlurpCsv(string csvPath) =>
        new(new StreamReader(csvPath), new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

    private async Task<int> SeedCalculatorRun(ApplicationDBContext db, String name, RelativeYear relativeYear, String defaultParamsPath, String lapcapPath)
    {
        var parameterMaster = new DefaultParameterSettingMaster
        {
            RelativeYear = relativeYear,
            EffectiveFrom = Now,
            EffectiveTo = null
        };

        db.DefaultParameterSettings.Add(parameterMaster);
        await db.SaveChangesAsync();
        db.DefaultParameterSettingDetail.AddRange(DefaultParameterSettingDetails(defaultParamsPath, parameterMaster.Id));
        await db.SaveChangesAsync();

        var lapcap = new LapcapDataMaster
        {
            RelativeYear  = relativeYear,
            EffectiveFrom = Now,
            EffectiveTo   = null
        };
        db.LapcapDataMaster.Add(lapcap);
        await db.SaveChangesAsync();
        var templates = await db.LapcapDataTemplateMaster.ToImmutableListAsync();
        db.LapcapDataDetail.AddRange(LapcapDataDetails(lapcapPath, lapcap, templates));
        await db.SaveChangesAsync();

        var run = new CalculatorRun
        {
            Name                            = name,
            RelativeYear                    = relativeYear,
            CreatedBy                       = "some-user",
            CreatedAt                       = Now,
            CalculatorRunClassificationId   = (int)RunClassification.RUNNING,
            DefaultParameterSettingMasterId = parameterMaster.Id,
            LapcapDataMasterId              = lapcap.Id,
            IsBillingFileGenerating         = true // Should really be set just before PrepareBillingFileAsync is run but work here.
        };
        db.CalculatorRuns.Add(run);
        await db.SaveChangesAsync();

        return run.Id;
    }

    private static async Task SeedAcceptOrRejectProducers(ApplicationDBContext db, int calculatorRunId, string modifiedBy, string csvPath)
    {
        var csvRows     = SlurpCsv(csvPath).GetRecords<dynamic>().ToImmutableList();
        var producerIds = csvRows.Select(x => int.Parse(x.producer_id)).ToHashSet();
        var dbRows      = await db.ProducerResultFileSuggestedBillingInstruction
                            .Where(x => x.CalculatorRunId == calculatorRunId && producerIds.Contains(x.ProducerId))
                            .ToListAsync();

        foreach (var dbRow in dbRows)
        {
            var csvRow = csvRows.Single(x => int.Parse(x.producer_id) == dbRow.ProducerId);

            dbRow.BillingInstructionAcceptReject = csvRow.billing_status;
            dbRow.ReasonForRejection             = csvRow.rejected_reason == "NULL" ? null : csvRow.rejected_reason;
            dbRow.LastModifiedAcceptReject       = Now;
            dbRow.LastModifiedAcceptRejectBy     = modifiedBy;
        }

        await db.SaveChangesAsync();
    }

    private static ImmutableList<DefaultParameterSettingDetail> DefaultParameterSettingDetails(string defaultParamsPath, int masterId) =>
        SlurpCsv(defaultParamsPath)
            .GetRecords<dynamic>()
            .Select(row => (IDictionary<string, object>)row)
            .SelectMany(row =>
            {
                var paramRef = row["Parameter Unique Ref"]?.ToString();
                var rawValue = row["Parameter Value"]?.ToString();

                if (string.IsNullOrWhiteSpace(paramRef))
                    return Enumerable.Empty<DefaultParameterSettingDetail>();

                if (paramRef.StartsWith("Parameter upload version"))
                    return Enumerable.Empty<DefaultParameterSettingDetail>();

                if (string.IsNullOrWhiteSpace(rawValue))
                    return Enumerable.Empty<DefaultParameterSettingDetail>();

                var valueClean = rawValue!
                    .Replace("£", "")
                    .Replace("%", "")
                    .Replace(",", "")
                    .Trim();

                if (!decimal.TryParse(valueClean, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value))
                    throw new Exception($"Failed to parse value '{rawValue}' -> '{valueClean}' for {paramRef}");

                return
                [
                    new DefaultParameterSettingDetail
                    {
                        DefaultParameterSettingMasterId = masterId,
                        ParameterUniqueReferenceId      = paramRef!,
                        ParameterValue                  = value
                    }
                ];
            }).ToImmutableList();

    private static ImmutableList<LapcapDataDetail> LapcapDataDetails(string lapcapPath, LapcapDataMaster master, ImmutableList<LapcapDataTemplateMaster> templates) =>
        SlurpCsv(lapcapPath)
            .GetRecords<dynamic>()
            .Select(row => new LapcapDataDetail
            {
                LapcapDataMasterId = master.Id,
                UniqueReference    = templates.Single(x => x.Material == row.material && x.Country == row.country).UniqueReference,
                TotalCost          = decimal.Parse(row.total_cost),
                LapcapDataMaster   = master // TODO why is this a required field?
            }).ToImmutableList();

    private static ImmutableList<OrganisationResponse> OrganisationResponses(string organisationsPath) =>
        SlurpCsv(organisationsPath)
            .GetRecords<dynamic>()
            .Select(row => new OrganisationResponse
            {
                OrganisationId   = int.Parse(row.organisation_id),
                SubsidiaryId     = row.subsidiary_id,
                OrganisationName = row.organisation_name,
                TradingName      = row.trading_name,
                ObligationStatus = row.obligation_status,
                SubmitterId      = row.submitter_id == "NULL" ? null : row.submitter_id,
                StatusCode       = row.status_code  == "NULL" ? null : row.status_code,
                HasH1            = row.has_h1 == "1",
                HasH2            = row.has_h2 == "1"
            }).ToImmutableList();

    private static ImmutableList<PomResponse> PomResponses(string pomsPath) =>
        SlurpCsv(pomsPath)
            .GetRecords<dynamic>()
            .Select(row => new PomResponse
            {
                OrganisationId              = int.Parse(row.organisation_id),
                SubsidiaryId                = row.subsidiary_id,
                SubmissionPeriod            = row.submission_period,
                PackagingActivity           = row.packaging_activity,
                PackagingType               = row.packaging_type,
                PackagingClass              = row.packaging_class,
                PackagingMaterial           = row.packaging_material,
                PackagingMaterialWeight     = double.Parse(row.packaging_material_weight, CultureInfo.InvariantCulture),
                SubmissionPeriodDescription = row.submission_period_desc,
                SubmitterId                 = row.submitter_id               == "NULL" ? null : row.submitter_id,
                PackagingMaterialSubtype    = row.packaging_material_subtype == "NULL" ? null : row.packaging_material_subtype,
                RamRagRating                = row.ram_rag_rating             == "NULL" ? null : row.ram_rag_rating
            }).ToImmutableList();
    }