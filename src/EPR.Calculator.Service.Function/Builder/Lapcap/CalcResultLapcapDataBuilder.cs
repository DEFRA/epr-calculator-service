namespace EPR.Calculator.Service.Function.Builder.Lapcap
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.EntityFrameworkCore;

    public class CalcResultLapcapDataBuilder : ICalcResultLapcapDataBuilder
    {
        private readonly ApplicationDBContext context;
        public const string LapcapHeader = "LAPCAP Data";
        public const string CountryApportionment = "1 Country Apportionment %s";
        public const string Total = "Total";
        public const int HundredPercent = 100;

        public CalcResultLapcapDataBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<CalcResultLapcapData> Construct(CalcResultsRequestDto resultsRequestDto)
        {
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            var orderId = 1;
            var data = new List<CalcResultLapcapDataDetails>();
            data.Add(new CalcResultLapcapDataDetails
            {
                Name = LapcapHeaderConstants.Name,
                EnglandDisposalCost = LapcapHeaderConstants.EnglandDisposalCost,
                WalesDisposalCost = LapcapHeaderConstants.WalesDisposalCost,
                ScotlandDisposalCost = LapcapHeaderConstants.ScotlandDisposalCost,
                NorthernIrelandDisposalCost = LapcapHeaderConstants.NorthernIrelandDisposalCost,
                OrderId = orderId,
                TotalDisposalCost = LapcapHeaderConstants.TotalDisposalCost,
            });

            var results = await (from run in this.context.CalculatorRuns
                           join lapcapMaster in this.context.LapcapDataMaster on run.LapcapDataMasterId equals lapcapMaster.Id
                           join lapcapDetail in this.context.LapcapDataDetail on lapcapMaster.Id equals lapcapDetail.LapcapDataMasterId
                           join lapcapTemplate in this.context.LapcapDataTemplateMaster on lapcapDetail.UniqueReference equals lapcapTemplate.UniqueReference
                           where run.Id == resultsRequestDto.RunId
                           select new ResultsClass
                           {
                               Material = lapcapTemplate.Material,
                               Country = lapcapTemplate.Country,
                               TotalCost = lapcapDetail.TotalCost,
                           }).ToListAsync();

            var materials = await this.context.Material.Select(x => x.Name).ToListAsync();

            var countries = await this.context.Country.ToListAsync();

            var costType = await this.context.CostType.SingleAsync(x => x.Name == "Fee for LA Disposal Costs");
            var costTypeId = costType.Id;

            foreach (var material in materials)
            {
                var detail = new CalcResultLapcapDataDetails
                {
                    Name = material,
                    EnglandCost = GetMaterialDisposalCostPerCountry(CountryConstants.England, material, results),
                    NorthernIrelandCost = GetMaterialDisposalCostPerCountry(CountryConstants.NI, material, results),
                    ScotlandCost = GetMaterialDisposalCostPerCountry(CountryConstants.Scotland, material, results),
                    WalesCost = GetMaterialDisposalCostPerCountry(CountryConstants.Wales, material, results),
                    OrderId = ++orderId,
                    TotalCost = GetTotalMaterialDisposalCost(material, results),
                };

                detail.EnglandDisposalCost = detail.EnglandCost.ToString("C", culture);
                detail.NorthernIrelandDisposalCost = detail.NorthernIrelandCost.ToString("C", culture);
                detail.ScotlandDisposalCost = detail.ScotlandCost.ToString("C", culture);
                detail.WalesDisposalCost = detail.WalesCost.ToString("C", culture);
                detail.TotalDisposalCost = detail.TotalCost.ToString("C", culture);

                data.Add(detail);
            }

            var totalDetail = new CalcResultLapcapDataDetails
            {
                Name = Total,
                EnglandCost = data.Sum(x => x.EnglandCost),
                NorthernIrelandCost = data.Sum(x => x.NorthernIrelandCost),
                ScotlandCost = data.Sum(x => x.ScotlandCost),
                WalesCost = data.Sum(x => x.WalesCost),
                TotalCost = data.Sum(x => x.TotalCost),
                OrderId = ++orderId,
            };
            totalDetail.EnglandDisposalCost = totalDetail.EnglandCost.ToString("C", culture);
            totalDetail.NorthernIrelandDisposalCost = totalDetail.NorthernIrelandCost.ToString("C", culture);
            totalDetail.ScotlandDisposalCost = totalDetail.ScotlandCost.ToString("C", culture);
            totalDetail.WalesDisposalCost = totalDetail.WalesCost.ToString("C", culture);
            totalDetail.TotalDisposalCost = totalDetail.TotalCost.ToString("C", culture);
            data.Add(totalDetail);


            var countryApportionment = new CalcResultLapcapDataDetails
            {
                Name = CountryApportionment,
                EnglandCost = CalculateApportionment(totalDetail.EnglandCost, totalDetail.TotalCost),
                NorthernIrelandCost = CalculateApportionment(totalDetail.NorthernIrelandCost, totalDetail.TotalCost),
                ScotlandCost = CalculateApportionment(totalDetail.ScotlandCost, totalDetail.TotalCost),
                WalesCost = CalculateApportionment(totalDetail.WalesCost, totalDetail.TotalCost),
                TotalCost = HundredPercent,
                OrderId = ++orderId,
            };
            countryApportionment.EnglandDisposalCost = $"{countryApportionment.EnglandCost.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%";
            countryApportionment.NorthernIrelandDisposalCost = $"{countryApportionment.NorthernIrelandCost.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%";
            countryApportionment.ScotlandDisposalCost = $"{countryApportionment.ScotlandCost.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%";
            countryApportionment.WalesDisposalCost = $"{countryApportionment.WalesCost.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%";
            countryApportionment.TotalDisposalCost = $"{countryApportionment.TotalCost.ToString("N", new NumberFormatInfo { NumberDecimalDigits = 8 })}%";
            data.Add(countryApportionment);

            this.context.CountryApportionment.Add(new CountryApportionment
            {
                CalculatorRunId = resultsRequestDto.RunId,
                CountryId = countries.Single(x => x.Name == "England").Id,
                CostTypeId = costTypeId,
                Apportionment = totalDetail.EnglandCost,
            });

            this.context.CountryApportionment.Add(new CountryApportionment
            {
                CalculatorRunId = resultsRequestDto.RunId,
                CountryId = countries.Single(x => x.Name == "Wales").Id,
                CostTypeId = costTypeId,
                Apportionment = totalDetail.WalesCost,
            });

            this.context.CountryApportionment.Add(new CountryApportionment
            {
                CalculatorRunId = resultsRequestDto.RunId,
                CountryId = countries.Single(x => x.Name == "Northern Ireland").Id,
                CostTypeId = costTypeId,
                Apportionment = totalDetail.NorthernIrelandCost,
            });

            this.context.CountryApportionment.Add(new CountryApportionment
            {
                CalculatorRunId = resultsRequestDto.RunId,
                CountryId = countries.Single(x => x.Name == "Scotland").Id,
                CostTypeId = costTypeId,
                Apportionment = totalDetail.ScotlandCost,
            });

            await this.context.SaveChangesAsync();

            return new CalcResultLapcapData { Name = LapcapHeader, CalcResultLapcapDataDetails = data };
        }

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
