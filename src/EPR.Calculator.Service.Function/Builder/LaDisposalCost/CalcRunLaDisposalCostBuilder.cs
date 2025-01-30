﻿using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Data;
using EPR.Calculator.Service.Function.Dtos;
using EPR.Calculator.Service.Function.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace EPR.Calculator.Service.Function.Builder.LaDisposalCost
{
    public class CalcRunLaDisposalCostBuilder : ICalcRunLaDisposalCostBuilder
    {
        internal class ProducerData
        {
            public required string Material { get; set; }
            public decimal Tonnage { get; set; }
        }


        private readonly ApplicationDBContext context;
        private List<ProducerData> producerData;

        public CalcRunLaDisposalCostBuilder(ApplicationDBContext context)
        {
            this.context = context;
            producerData = new List<ProducerData>();
        }


        public async Task<CalcResultLaDisposalCostData> Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
            var laDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>();
            var OrderId = 1;

            producerData = await (from run in context.CalculatorRuns
                join producerDetail in context.ProducerDetail on run.Id equals producerDetail.CalculatorRunId
                join producerMaterial in context.ProducerReportedMaterial on producerDetail.Id equals producerMaterial
                    .ProducerDetailId
                join material in context.Material on producerMaterial.MaterialId equals material.Id
                where run.Id == resultsRequestDto.RunId && producerMaterial.PackagingType != null &&
                      producerMaterial.PackagingType == CommonConstants.Household
                select new ProducerData
                {
                    Material = material.Name,
                    Tonnage = producerMaterial.PackagingTonnage
                }).ToListAsync();

            var lapcapDetails = calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails
                .Where(t => t.OrderId != 1 && t.Name != CalcResultLapcapDataBuilder.CountryApportionment).ToList();


            foreach (var details in lapcapDetails)
            {
                var laDiposalDetail = new CalcResultLaDisposalCostDataDetail()
                {
                    Name = details.Name,
                    England = details.EnglandDisposalCost,
                    Wales = details.WalesDisposalCost,
                    Scotland = details.ScotlandDisposalCost,
                    NorthernIreland = details.NorthernIrelandDisposalCost,
                    Total = details.TotalDisposalCost,
                    ProducerReportedHouseholdPackagingWasteTonnage = GetTonnageDataByMaterial(details.Name),
                    OrderId = ++OrderId
                };
                laDisposalCostDetails.Add(laDiposalDetail);

            }


            foreach (var details in laDisposalCostDetails)
            {
                details.LateReportingTonnage = GetLateReportingTonnageDataByMaterial(details.Name, calcResult.CalcResultLateReportingTonnageData.CalcResultLateReportingTonnageDetails.ToList());

                details.ProducerReportedHouseholdTonnagePlusLateReportingTonnage = GetProducerReportedHouseholdTonnagePlusLateReportingTonnage(details);
                if (details.Name == CommonConstants.Total) continue;
                details.DisposalCostPricePerTonne = CalculateDisposalCostPricePerTonne(details);
            }


            var header = GetHeader();
            laDisposalCostDetails.Insert(0, header);

            return new CalcResultLaDisposalCostData() { Name = CommonConstants.LADisposalCostData, CalcResultLaDisposalCostDetails = laDisposalCostDetails.AsEnumerable() };

        }

        private string GetTonnageDataByMaterial(string material)
        {
            return material == "Total"? producerData.Sum(t=>t.Tonnage).ToString()  : producerData.Where(t => t.Material == material).Sum(t => t.Tonnage).ToString();
        }

        private static string GetLateReportingTonnageDataByMaterial(string material, List<CalcResultLateReportingTonnageDetail> details)
        {
            return details.Where(t => t.Name == material).Sum(t => t.TotalLateReportingTonnage).ToString();
        }

        private static string GetProducerReportedHouseholdTonnagePlusLateReportingTonnage(CalcResultLaDisposalCostDataDetail detail)
        {
            var value = GetDecimalValue(detail.LateReportingTonnage) + GetDecimalValue(detail.ProducerReportedHouseholdPackagingWasteTonnage);
            return value.ToString();
        }

        private static string CalculateDisposalCostPricePerTonne(CalcResultLaDisposalCostDataDetail detail)
        {
            var HouseholdTonnagePlusLateReportingTonnage = GetDecimalValue(detail.ProducerReportedHouseholdTonnagePlusLateReportingTonnage);
            if (HouseholdTonnagePlusLateReportingTonnage == 0) return "0";
            var value = Math.Round(ConvertCurrencyToDecimal(detail.Total) / HouseholdTonnagePlusLateReportingTonnage, 4);
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            return value.ToString("C4", culture);
        }

        private static CalcResultLaDisposalCostDataDetail GetHeader()
        {
            return new CalcResultLaDisposalCostDataDetail()
            {
                Name = CommonConstants.Material,
                England = CommonConstants.England,
                Wales = CommonConstants.Wales,
                Scotland = CommonConstants.Scotland,
                NorthernIreland = CommonConstants.NorthernIreland,
                Total = CommonConstants.Total,
                ProducerReportedHouseholdPackagingWasteTonnage = CommonConstants.ProducerReportedHouseholdPackagingWasteTonnage,
                LateReportingTonnage = CommonConstants.LateReportingTonnage,
                ProducerReportedHouseholdTonnagePlusLateReportingTonnage = CommonConstants.ProduceLateTonnage,
                DisposalCostPricePerTonne = CommonConstants.DisposalCostPricePerTonne,
                OrderId = 1
            };
        }

        private static decimal GetDecimalValue(string value)
        {
            return decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        private static decimal ConvertCurrencyToDecimal(string currency)
        {
            decimal amount;
            decimal.TryParse(currency, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-GB"), out amount);
            return amount;
        }
    }
}
