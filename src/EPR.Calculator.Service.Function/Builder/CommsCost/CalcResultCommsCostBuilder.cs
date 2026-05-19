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

            var apportionmentDetail = apportionment.OnePlusFourApportionment;

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

            telemetryClient.TrackTrace($"Generating comms costs for {materialDetails.Count} materials...");
            var commsCostByMaterial = materialDetails.Select(material =>
            {
                telemetryClient.TrackTrace($"Generating comms cost for {material.Name}...");

                var producerReportedTon = producerReportedMaterials.Where(x => x.MaterialId == material.Id && x.PackagingType != PackagingTypes.PublicBin && x.PackagingType != PackagingTypes.HouseholdDrinksContainers)
                    .Sum(x => x.PackagingTonnage);

                var lateReportingTonnage = calcResultLateReportingTonnage.LateReportingTonnageByMaterial[material.Code];
                var publicBinTonnage = producerReportedMaterials.Where(p => p.MaterialId == material.Id && p.PackagingType == PackagingTypes.PublicBin).Sum(p => p.PackagingTonnage);
                var householdcontainers = producerReportedMaterials.Where(p => p.MaterialId == material.Id && p.PackagingType == PackagingTypes.HouseholdDrinksContainers).Sum(p => p.PackagingTonnage);


                var materialDefault = materialDefaults.Single(m => m.ParameterCategory == material.Name);
                var total = Math.Round(materialDefault.ParameterValue, 2);
                var producerReportedTotalTonnage =
                        producerReportedTon
                        + lateReportingTonnage.Total
                        + publicBinTonnage
                        + householdcontainers;
                var commsCost = new CalcResultCommsCostCommsCostByMaterial{
                    EnglandCost         = total * apportionmentDetail.England         / 100,
                    WalesCost           = total * apportionmentDetail.Wales           / 100,
                    ScotlandCost        = total * apportionmentDetail.Scotland        / 100,
                    NorthernIrelandCost = total * apportionmentDetail.NorthernIreland / 100,

                    HouseholdPackagingWasteTonnage = producerReportedTon,
                    PublicBinTonnage = publicBinTonnage,
                    HouseholdDrinksContainersTonnage = householdcontainers,
                    LateReportingTonnage = lateReportingTonnage.Total,
                };
                return (material.Code, commsCost);
            }).ToDictionary();

            telemetryClient.TrackTrace("Generating total row...");
            var commsCostByMaterialTotal = GetTotalRow(commsCostByMaterial.Values);

            var commsCostByUk =
                allDefaultResults.Single(x =>
                    x.ParameterType == CommunicationCostByCountry && x.ParameterCategory == Uk);

            var ukCost = new ByCountryCost
            {
                England         = commsCostByUk.ParameterValue * apportionmentDetail.England         / 100,
                Wales           = commsCostByUk.ParameterValue * apportionmentDetail.Wales           / 100,
                Scotland        = commsCostByUk.ParameterValue * apportionmentDetail.Scotland        / 100,
                NorthernIreland = commsCostByUk.ParameterValue * apportionmentDetail.NorthernIreland / 100
            };

            telemetryClient.TrackTrace("Getting comms cost by country...");
            var commsCostByCountryList = GetCommsCostByCountry(allDefaultResults);

            return new CalcResultCommsCost()
            {
                CalcResultCommsCostOnePlusFourApportionment = apportionmentDetail,
                CommsCostByMaterial                         = commsCostByMaterial,
                CommsCostByMaterialTotal                    = commsCostByMaterialTotal,
                CommsCostUkWide                             = ukCost,
                CommsCostByCountry                          = commsCostByCountryList
            };
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
            return new CalcResultCommsCostCommsCostByMaterial
            {
                EnglandCost         = list.Sum(x => x.EnglandCost),
                WalesCost           = list.Sum(x => x.WalesCost),
                NorthernIrelandCost = list.Sum(x => x.NorthernIrelandCost),
                ScotlandCost        = list.Sum(x => x.ScotlandCost),
                HouseholdPackagingWasteTonnage    = list.Sum(x => x.HouseholdPackagingWasteTonnage),
                PublicBinTonnage                  = list.Sum(x => x.PublicBinTonnage),
                HouseholdDrinksContainersTonnage  = list.Sum(x => x.HouseholdDrinksContainersTonnage),
                LateReportingTonnage              = list.Sum(x => x.LateReportingTonnage)
            };
        }

        private static ByCountryCost GetCommsCostByCountry(
            IEnumerable<CalcCommsBuilderResult> allDefaultResults
        )
        {
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

            return new ByCountryCost
            {
                England         = englandValue,
                Wales           = walesValue,
                Scotland        = scotlandValue,
                NorthernIreland = niValue
            };
        }
    }
}
