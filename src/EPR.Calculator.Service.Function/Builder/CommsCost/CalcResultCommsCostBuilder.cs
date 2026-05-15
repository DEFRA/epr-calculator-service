using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using EPR.Calculator.Service.Function.Builder.CommsCost;

namespace EPR.Calculator.Service.Function.Builder.CommsCost
{
    public interface ICalcResultCommsCostBuilder
    {
        Task<CalcResultCommsCost> ConstructAsync(
            IImmutableList<MaterialDetail> materialDetails,
            CalcResultsRequestDto resultsRequestDto,
            CalcResultOnePlusFourApportionment apportionment,
            CalcResultLateReportingTonnage calcResultLateReportingTonnage
        );
    }

    [ExcludeFromCodeCoverage]
    public class CalcResultCommsCostBuilder(ApplicationDBContext context, TelemetryClient telemetryClient)
        : ICalcResultCommsCostBuilder
    {
        public const string CommunicationCostByMaterial = "Communication costs by material";
        public const string CommunicationCostByCountry = "Communication costs by country";
        public const string TwoCCommsCostByCountry = "2c Comms Costs - by Country";
        public const string Uk = "United Kingdom";
        public const string TwoBCommsCostUkWide = "2b Comms Costs - UK wide";
        public const string OnePlusFourApportionment = "1 + 4 Apportionment %s";

        public async Task<CalcResultCommsCost> ConstructAsync(
            IImmutableList<MaterialDetail> materialDetails,
            CalcResultsRequestDto resultsRequestDto,
            CalcResultOnePlusFourApportionment apportionment,
            CalcResultLateReportingTonnage calcResultLateReportingTonnage
        )
        {
            telemetryClient.TrackTrace("Begining constructing comms cost...");
            var runId = resultsRequestDto.RunId;

            var apportionmentDetails = apportionment.CalcResultOnePlusFourApportionmentDetails;
            var apportionmentDetail = apportionmentDetails.Last();

            var result = new CalcResultCommsCost();
            telemetryClient.TrackTrace("Calculating apportionment...");
            result.CalcResultCommsCostOnePlusFourApportionment = GetApportionment(apportionmentDetail, result);

            telemetryClient.TrackTrace("Getting material defaults...");
            var allDefaultResults = await (
                from run in context.CalculatorRuns
                join defaultMaster in context.DefaultParameterSettings on run.DefaultParameterSettingMasterId equals defaultMaster.Id
                join defaultDetail in context.DefaultParameterSettingDetail on defaultMaster.Id equals defaultDetail.DefaultParameterSettingMasterId
                join defaultTemplate in context.DefaultParameterTemplateMasterList on defaultDetail.ParameterUniqueReferenceId equals defaultTemplate.ParameterUniqueReferenceId
                where run.Id == runId
                select new CalcCommsBuilderResult
                {
                    ParameterValue = defaultDetail.ParameterValue,
                    ParameterType = defaultTemplate.ParameterType,
                    ParameterCategory = defaultTemplate.ParameterCategory,
                }).Distinct().ToListAsync();

            var materialDefaults = allDefaultResults.Where(x =>
                x.ParameterType == CommunicationCostByMaterial && materialDetails.Select(m => m.Name).Contains(x.ParameterCategory));

            telemetryClient.TrackTrace("Getting producer reported materials...");
            var producerReportedMaterials = await GetProducerReportedMaterials(context, runId);

            telemetryClient.TrackTrace("Getting headers...");
            var list = new List<CalcResultCommsCostCommsCostByMaterial>();

            telemetryClient.TrackTrace($"Generating comms costs for {materialDetails.Count} materials...");
            Console.WriteLine($"materialDetails={materialDetails.Count}");
            foreach (var material in materialDetails)
            {
                telemetryClient.TrackTrace($"Generating comms cost for {material.Name}...");

                var producerReportedTon = producerReportedMaterials.Where(x => x.MaterialId == material.Id && x.PackagingType != PackagingTypes.PublicBin && x.PackagingType != PackagingTypes.HouseholdDrinksContainers)
                    .Sum(x => x.PackagingTonnage);

                var lateReportingTonnage = calcResultLateReportingTonnage.CalcResultLateReportingTonnageDetails.Single(x => x.Name == material.Name);
                var publicBinTonnage = producerReportedMaterials.Where(p => p.MaterialId == material.Id && p.PackagingType == PackagingTypes.PublicBin).Sum(p => p.PackagingTonnage);
                var householdcontainers = producerReportedMaterials.Where(p => p.MaterialId == material.Id && p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.PackagingTonnage);


                var materialDefault = materialDefaults.Single(m => m.ParameterCategory == material.Name);
                var total = Math.Round(materialDefault.ParameterValue, 2);
                var producerReportedTotalTonnage =
                        producerReportedTon
                        + lateReportingTonnage.TotalLateReportingTonnage
                        + publicBinTonnage
                        + householdcontainers;
                var commsCost = new CalcResultCommsCostCommsCostByMaterial{
                    Name = materialDefault.ParameterCategory,
                    England = apportionmentDetail.EnglandTotal * total / 100,
                    Wales = apportionmentDetail.WalesTotal * total / 100,
                    NorthernIreland = apportionmentDetail.NorthernIrelandTotal * total / 100,
                    Scotland = apportionmentDetail.ScotlandTotal * total / 100,

                    ProducerReportedHouseholdPackagingWasteTonnage = producerReportedTon,
                    ReportedPublicBinTonnage = publicBinTonnage,
                    HouseholdDrinksContainers = householdcontainers,
                    LateReportingTonnage = lateReportingTonnage.TotalLateReportingTonnage,

                    // TODO derived fields?
                    Total = total,
                    ProducerReportedTotalTonnage = producerReportedTotalTonnage,
                    CommsCostByMaterialPricePerTonne = producerReportedTotalTonnage != 0
                            ? total / producerReportedTotalTonnage : 0
                };
                list.Add(commsCost);
            }

            telemetryClient.TrackTrace("Generating total row...");
            var totalRow = GetTotalRow(list);

            list.Add(totalRow);
            result.CalcResultCommsCostCommsCostByMaterial = list;

            var commsCostByUk =
                allDefaultResults.Single(x =>
                    x.ParameterType == CommunicationCostByCountry && x.ParameterCategory == Uk);

            var ukCost = new CalcResultCommsCostOnePlusFourApportionment
            {
                Name = TwoBCommsCostUkWide,
                England         = (commsCostByUk.ParameterValue * apportionmentDetail.EnglandTotal) / 100,
                Wales           = (commsCostByUk.ParameterValue * apportionmentDetail.WalesTotal) / 100,
                Scotland        = (commsCostByUk.ParameterValue * apportionmentDetail.ScotlandTotal) / 100,
                NorthernIreland = (commsCostByUk.ParameterValue * apportionmentDetail.NorthernIrelandTotal) / 100,
                Total           = commsCostByUk.ParameterValue,
                OrderId         = 2
            };

            telemetryClient.TrackTrace("Getting comms cost by country...");
            var commsCostByCountryList = GetCommsCostByCountryList(ukCost, allDefaultResults);
            result.CommsCostByCountry = commsCostByCountryList;

            return result;
        }

        public async Task<List<ProducerReportedMaterialProjected>> GetProducerReportedMaterials(ApplicationDBContext context, int runId)
        {
            return await (
                from run in context.CalculatorRuns
                join pd in context.ProducerDetail on run.Id equals pd.CalculatorRunId
                join mat in context.ProducerReportedMaterialProjected on pd.Id equals mat.ProducerDetailId
                join material in context.Material on mat.MaterialId equals material.Id
                where run.Id == runId &&
                      // TODO need the following filter?
                      mat.PackagingType != null &&
                      (mat.PackagingType == PackagingTypes.Household ||
                       mat.PackagingType == PackagingTypes.PublicBin ||
                       (mat.PackagingType == PackagingTypes.HouseholdDrinksContainers &&
                        material.Code == MaterialCodes.Glass))
                select mat
            ).Distinct().ToListAsync();
        }

        private static CalcResultCommsCostCommsCostByMaterial GetTotalRow(
            IEnumerable<CalcResultCommsCostCommsCostByMaterial> list
        )
        {
            // TODO move total row to Exporter?
            return new CalcResultCommsCostCommsCostByMaterial
            {
                Name            = "Total",
                England         = list.Sum(x => x.England),
                Wales           = list.Sum(x => x.Wales),
                NorthernIreland = list.Sum(x => x.NorthernIreland),
                Scotland        = list.Sum(x => x.Scotland),
                Total           = list.Sum(x => x.Total),
                ProducerReportedHouseholdPackagingWasteTonnage = list.Sum(x => x.ProducerReportedHouseholdPackagingWasteTonnage),
                ProducerReportedTotalTonnage   = list.Sum(x => x.ProducerReportedTotalTonnage),
                LateReportingTonnage      = list.Sum(x => x.LateReportingTonnage),
                ReportedPublicBinTonnage  = list.Sum(x => x.ReportedPublicBinTonnage),
                HouseholdDrinksContainers = list.Sum(x => x.HouseholdDrinksContainers),
            };
        }

        private static List<CalcResultCommsCostOnePlusFourApportionment> GetCommsCostByCountryList(
            CalcResultCommsCostOnePlusFourApportionment ukCost,
            IEnumerable<CalcCommsBuilderResult> allDefaultResults
        )
        {
            var commsCostByCountryList = new List<CalcResultCommsCostOnePlusFourApportionment>();

            commsCostByCountryList.Add(ukCost);

            // TODO what is allDefaultResults and does it need Category String?
            var englandValue =
                allDefaultResults.Single(x =>
                    x.ParameterType == CommunicationCostByCountry &&
                    x.ParameterCategory == "England").ParameterValue;
            var walesValue =
                allDefaultResults.Single(x =>
                    x.ParameterType == CommunicationCostByCountry &&
                    x.ParameterCategory == "Wales").ParameterValue;
            var niValue =
                allDefaultResults.Single(x =>
                    x.ParameterType == CommunicationCostByCountry &&
                    x.ParameterCategory == "Northern Ireland").ParameterValue;
            var scotlandValue =
                allDefaultResults.Single(x =>
                    x.ParameterType == CommunicationCostByCountry &&
                    x.ParameterCategory == "Scotland").ParameterValue;

            var countryCost = new CalcResultCommsCostOnePlusFourApportionment
            {
                England         = englandValue,
                Wales           = walesValue,
                Scotland        = scotlandValue,
                NorthernIreland = niValue,
                Total           = englandValue + walesValue + scotlandValue + niValue,
                Name            = TwoCCommsCostByCountry,
                OrderId         = 3
            };

            commsCostByCountryList.Add(countryCost);

            return commsCostByCountryList;
        }

        private static List<CalcResultCommsCostOnePlusFourApportionment> GetApportionment(
            CalcResultOnePlusFourApportionmentDetail apportionmentDetail,
            CalcResultCommsCost result)
        {
            // TODO this does not need to be a list
            var commsApportionments = new List<CalcResultCommsCostOnePlusFourApportionment>();

            var commsApportionment = new CalcResultCommsCostOnePlusFourApportionment
            {
                Name            = OnePlusFourApportionment,
                England         = apportionmentDetail.EnglandTotal,
                Wales           = apportionmentDetail.WalesTotal,
                Scotland        = apportionmentDetail.ScotlandTotal,
                NorthernIreland = apportionmentDetail.NorthernIrelandTotal,
                Total           = apportionmentDetail.Total
            };

            commsApportionments.Add(commsApportionment);

            return commsApportionments;
        }
    }
}
