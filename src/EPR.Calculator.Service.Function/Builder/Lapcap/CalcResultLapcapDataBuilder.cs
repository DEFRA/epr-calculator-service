using System.Globalization;
using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Builder.Lapcap
{
    public interface ICalcResultLapcapDataBuilder
    {
        Task<CalcResultLapcapData> ConstructAsync(
            IEnumerable<MaterialDetail> materialDetails,
            CalcResultsRequestDto resultsRequestDto
        );
    }

    public class CalcResultLapcapDataBuilder : ICalcResultLapcapDataBuilder
    {
        private readonly ICalcCountryApportionmentService calcCountryApportionmentService;
        private readonly ApplicationDBContext dbContext;

        //public const string CountryApportionment = "1 Country Apportionment %s";
        public const string Total = "Total";

        public CalcResultLapcapDataBuilder(ApplicationDBContext dbContext,
            ICalcCountryApportionmentService calcCountryApportionmentService)
        {
            this.dbContext = dbContext;
            this.calcCountryApportionmentService = calcCountryApportionmentService;
        }

#pragma warning disable S1854
        public async Task<CalcResultLapcapData> ConstructAsync(
            IEnumerable<MaterialDetail> materialDetails,
            CalcResultsRequestDto resultsRequestDto
        )
        {
            var orderId = 1;
            var data = new List<CalcResultLapcapDataDetail>();

            var results = await (
                from run in dbContext.CalculatorRuns
                join lapcapMaster in dbContext.LapcapDataMaster on run.LapcapDataMasterId equals lapcapMaster.Id
                join lapcapDetail in dbContext.LapcapDataDetail on lapcapMaster.Id equals lapcapDetail.LapcapDataMasterId
                join lapcapTemplate in dbContext.LapcapDataTemplateMaster on lapcapDetail.UniqueReference equals lapcapTemplate.UniqueReference
                where run.Id == resultsRequestDto.RunId
                select new ResultsClass
                {
                    Material  = lapcapTemplate.Material,
                    Country   = lapcapTemplate.Country,
                    TotalCost = lapcapDetail.TotalCost,
                }
            ).ToListAsync();

            var countries = await dbContext.Country.ToListAsync();

            var costType = await dbContext.CostType.SingleAsync(x => x.Name == "Fee for LA Disposal Costs");
            var costTypeId = costType.Id;

            foreach (var material in materialDetails.Select(m => m.Name))
            {
                var detail = new CalcResultLapcapDataDetail
                {
                    Name                = material,
                    EnglandCost         = GetMaterialDisposalCostPerCountry(CountryConstants.England, material, results),
                    NorthernIrelandCost = GetMaterialDisposalCostPerCountry(CountryConstants.NI, material, results),
                    ScotlandCost        = GetMaterialDisposalCostPerCountry(CountryConstants.Scotland, material, results),
                    WalesCost           = GetMaterialDisposalCostPerCountry(CountryConstants.Wales, material, results),
                    TotalCost           = GetTotalMaterialDisposalCost(material, results),
                    OrderId             = ++orderId
                };

                data.Add(detail);
            }

            var totalDetail = new CalcResultLapcapDataDetail
            {
                Name                = Total,
                EnglandCost         = data.Sum(x => x.EnglandCost),
                NorthernIrelandCost = data.Sum(x => x.NorthernIrelandCost),
                ScotlandCost        = data.Sum(x => x.ScotlandCost),
                WalesCost           = data.Sum(x => x.WalesCost),
                TotalCost           = data.Sum(x => x.TotalCost),
                OrderId             = ++orderId,
            };
            data.Add(totalDetail);

            var countryApportionment = new CountryApportionmentData
            {
                England         = CalculateApportionment(totalDetail.EnglandCost, totalDetail.TotalCost),
                NorthernIreland = CalculateApportionment(totalDetail.NorthernIrelandCost, totalDetail.TotalCost),
                Scotland        = CalculateApportionment(totalDetail.ScotlandCost, totalDetail.TotalCost),
                Wales           = CalculateApportionment(totalDetail.WalesCost, totalDetail.TotalCost)
            };

            if (!resultsRequestDto.IsBillingFile)
            {
                await calcCountryApportionmentService.SaveChangesAsync(new CalcCountryApportionmentServiceDto
                {
                    RunId               = resultsRequestDto.RunId,
                    Countries           = countries,
                    CostTypeId          = costTypeId,
                    EnglandCost         = countryApportionment.England,
                    NorthernIrelandCost = countryApportionment.NorthernIreland,
                    ScotlandCost        = countryApportionment.Scotland,
                    WalesCost           = countryApportionment.Wales
                });
            }

            return new CalcResultLapcapData { CalcResultLapcapDataDetails = data, CountryApportionment = countryApportionment };
        }
#pragma warning restore

        internal static decimal CalculateApportionment(decimal countryCost, decimal totalCost)
        {
            if (totalCost != 0)
            {
                var total = (countryCost / totalCost);
                return total * 100;
            }

            return 0;
        }

        internal static decimal GetMaterialDisposalCostPerCountry(string country, string material, IEnumerable<ResultsClass> results)
        {
            var totalSum = results.Where(x => x.Country.Equals(country, StringComparison.OrdinalIgnoreCase) &&
                x.Material.Equals(material, StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.TotalCost);
            return totalSum;
        }

        internal static decimal GetTotalMaterialDisposalCost(string material, IEnumerable<ResultsClass> results)
        {
            var totalSum = results.Where(x => x.Material.Equals(material, StringComparison.OrdinalIgnoreCase)).Sum(x => x.TotalCost);
            return totalSum;
        }
    }
}
