using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;


namespace EPR.Calculator.Service.Function.Builder.ParametersOther
{
    public interface ICalcResultParameterOtherCostBuilder
    {
        Task<CalcResultParameterOtherCost> ConstructAsync(RunContext runContext);
    }

    public class CalcResultParameterOtherCostBuilder(
        ApplicationDBContext dbContext,
        ICalcCountryApportionmentService calcCountryApportionmentService)
        : ICalcResultParameterOtherCostBuilder
    {
        public const string SchemeAdminOperatingCost = "Scheme administrator operating costs";
        public const string LaPrepCharge = "Local authority data preparation costs";
        public const string SchemeSetupCost = "Scheme setup costs";

        public async Task<CalcResultParameterOtherCost> ConstructAsync(RunContext runContext)
        {
            //TODO inject params
            var results = await (
                from run in dbContext.CalculatorRuns
                join defaultMaster in dbContext.DefaultParameterSettings on run.DefaultParameterSettingMasterId equals defaultMaster.Id
                join defaultDetail in dbContext.DefaultParameterSettingDetail on defaultMaster.Id equals defaultDetail.DefaultParameterSettingMasterId
                join defaultTemplate in dbContext.DefaultParameterTemplateMasterList on defaultDetail.ParameterUniqueReferenceId equals defaultTemplate.ParameterUniqueReferenceId
                where run.Id == runContext.RunId
                select new DefaultParamResultsClass
                {
                    ParameterValue           = defaultDetail.ParameterValue,
                    ParameterCategory        = defaultTemplate.ParameterCategory,
                    ParameterType            = defaultTemplate.ParameterType,
                    ParameterUniqueReference = defaultDetail.ParameterUniqueReferenceId
                }
            ).ToListAsync();

            var schemeAdminCosts = results
                .Where(x => x.ParameterType == SchemeAdminOperatingCost)
                .ToImmutableArray();

            var saOperatingCost = GetPrepCharge(schemeAdminCosts);

            var lapPrepCharges = results
                .Where(x => x.ParameterType == LaPrepCharge)
                .ToImmutableArray();

            var laDataPrep = GetPrepCharge(lapPrepCharges);
            var countryApportionment = GetCountryApportionment(laDataPrep);

            var schemeSetUpCharges = results
                .Where(x => x.ParameterType == SchemeSetupCost)
                .ToImmutableArray();

            var schemeSetupCharge = GetPrepCharge(schemeSetUpCharges);

            var materialityResults = results
                .Where(x => x.ParameterType == "Materiality threshold")
                .ToImmutableArray();

            var tonnageResults = results
                .Where(x => x.ParameterType == "Tonnage change threshold")
                .ToImmutableArray();

            var countries = await dbContext.Country.ToListAsync();

            var costType = await dbContext.CostType.SingleAsync(x => x.Name == "LA Data Prep Charge");

            if (runContext.RunType == RunType.Calculator)
            {
                await calcCountryApportionmentService.SaveChangesAsync(new CalcCountryApportionmentServiceDto
                {
                    RunId               = runContext.RunId,
                    Countries           = countries,
                    CostTypeId          = costType.Id,
                    EnglandCost         = laDataPrep.England,
                    NorthernIrelandCost = laDataPrep.NorthernIreland,
                    ScotlandCost        = laDataPrep.Scotland,
                    WalesCost           = laDataPrep.Wales
                });
            }

            return new CalcResultParameterOtherCost
            {
                LaDataPrepCharge      = laDataPrep,
                CountryApportionment  = countryApportionment,
                SaOperatingCost       = saOperatingCost,
                SchemeSetupCost       = schemeSetupCharge,
                BadDebtValue          = Value(results.Where(x => x.ParameterType == "Bad debt provision").ToList(), "Percentage"),
                MaterialityIncrease   = new() { Amount = Value(materialityResults, "Amount Increase"), Percentage = Value(materialityResults, "Percent Increase") },
                MaterialityDecrease   = new() { Amount = Value(materialityResults, "Amount Decrease"), Percentage = Value(materialityResults, "Percent Decrease") },
                TonnageChangeIncrease = new() { Amount = Value(tonnageResults    , "Amount Increase"), Percentage = Value(tonnageResults    , "Percent Increase") },
                TonnageChangeDecrease = new() { Amount = Value(tonnageResults    , "Amount Decrease"), Percentage = Value(tonnageResults    , "Percent Decrease") }
            };
        }

        private static ByCountryApportionment GetCountryApportionment(ByCountryCost laDataPrep)
        {
            var total = laDataPrep.England + laDataPrep.NorthernIreland + laDataPrep.Wales + laDataPrep.Scotland;
            return new ByCountryApportionment
            {
                England         = total != 0 ? (laDataPrep.England         / total) * 100 : 0M,
                NorthernIreland = total != 0 ? (laDataPrep.NorthernIreland / total) * 100 : 0M,
                Scotland        = total != 0 ? (laDataPrep.Scotland        / total) * 100 : 0M,
                Wales           = total != 0 ? (laDataPrep.Wales           / total) * 100 : 0M
            };
        }

        private static ByCountryCost GetPrepCharge(IReadOnlyCollection<DefaultParamResultsClass> lapPrepCharges) =>
            new ()
            {
                England         = Value(lapPrepCharges, "England"),
                Wales           = Value(lapPrepCharges, "Wales"),
                Scotland        = Value(lapPrepCharges, "Scotland"),
                NorthernIreland = Value(lapPrepCharges, "Northern Ireland")
            };

        private static decimal Value(IReadOnlyCollection<DefaultParamResultsClass> defaultParams, string category) =>
            defaultParams
                .Single(x => x.ParameterCategory == category)
                .ParameterValue
                .ToDecimal();
    }
}
