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
            public required string MaterialName { get; set; }
            public required string PackagingType { get; set; }
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

            await SetProducerData(resultsRequestDto);

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
                    ReportedPublicBinTonnage = GetReportedPublicBinTonnage(details.Name),
                    HouseholdDrinkContainers = GetReportedHouseholdDrinksContainerTonnage(details.Name),
                    OrderId = ++OrderId
                };
                laDiposalDetail.LateReportingTonnage = GetLateReportingTonnageDataByMaterial(laDiposalDetail.Name, calcResult.CalcResultLateReportingTonnageData.CalcResultLateReportingTonnageDetails.ToList());
                laDiposalDetail.ProducerReportedTotalTonnage = GetProducerReportedTotalTonnage(laDiposalDetail);
                laDiposalDetail.DisposalCostPricePerTonne = laDiposalDetail.Name == CommonConstants.Total
                    ? string.Empty
                    : CalculateDisposalCostPricePerTonne(laDiposalDetail);
                laDisposalCostDetails.Add(laDiposalDetail);

            }

            var header = GetHeader();
            laDisposalCostDetails.Insert(0, header);

            return new CalcResultLaDisposalCostData()
            {
                Name = CommonConstants.LADisposalCostData,
                CalcResultLaDisposalCostDetails = laDisposalCostDetails.AsEnumerable()
            };
            }

        private string GetReportedHouseholdDrinksContainerTonnage(string materialName)
        {
            if (materialName == CommonConstants.Total)
            {
                var householdDrinksContainerData = producerData
                    .Where(p => p.PackagingType == PackagingTypes.HouseholdDrinksContainers);

                return householdDrinksContainerData.Any()
                    ? householdDrinksContainerData.Sum(p => p.Tonnage).ToString()
                    : "0";
            }
            else
            {
                var householdDrinksContainerData = producerData
                    .Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.HouseholdDrinksContainers);

                return householdDrinksContainerData.Any()
                    ? householdDrinksContainerData.Sum(p => p.Tonnage).ToString()
                    : string.Empty;
            }
        }

        private string GetReportedPublicBinTonnage(string materialName)
        {
            return materialName == CommonConstants.Total
                ? producerData
                    .Where(p => p.PackagingType == PackagingTypes.PublicBin)
                    .Sum(p => p.Tonnage).ToString()
                : producerData
                    .Where(p => p.MaterialName == materialName && p.PackagingType == PackagingTypes.PublicBin)
                    .Sum(p => p.Tonnage).ToString();
        }

        private string GetTonnageDataByMaterial(string materialName)
        {
            return materialName == CommonConstants.Total
                ? producerData.Where(t => t.PackagingType == PackagingTypes.Household).Sum(t => t.Tonnage).ToString()
                : producerData.Where(t => t.MaterialName == materialName && t.PackagingType == PackagingTypes.Household).Sum(t => t.Tonnage).ToString();
        }

        private static string GetLateReportingTonnageDataByMaterial(string materialName, List<CalcResultLateReportingTonnageDetail> details)
        {
            return details.Where(t => t.Name == materialName).Sum(t => t.TotalLateReportingTonnage).ToString();
        }

        private static string GetProducerReportedTotalTonnage(CalcResultLaDisposalCostDataDetail detail)
        {
            var householdDrinkContainersValue = detail.HouseholdDrinkContainers == null
                ? 0
                : GetDecimalValue(detail.HouseholdDrinkContainers);

            var value = GetDecimalValue(detail.LateReportingTonnage)
                + GetDecimalValue(detail.ProducerReportedHouseholdPackagingWasteTonnage)
                + GetDecimalValue(detail.ReportedPublicBinTonnage)
                + householdDrinkContainersValue;

            return value.ToString();
        }

        private static string CalculateDisposalCostPricePerTonne(CalcResultLaDisposalCostDataDetail detail)
        {
            var HouseholdTonnagePlusLateReportingTonnage = GetDecimalValue(detail.ProducerReportedTotalTonnage);
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
                ReportedPublicBinTonnage = CommonConstants.ReportedPublicBinTonnage,
                HouseholdDrinkContainers = CommonConstants.HouseholdDrinkContainers,
                LateReportingTonnage = CommonConstants.LateReportingTonnage,
                ProducerReportedTotalTonnage = CommonConstants.ProducerReportedTotalTonnage,
                DisposalCostPricePerTonne = CommonConstants.DisposalCostPricePerTonne,
                OrderId = 1
            };
        }

        private static decimal GetDecimalValue(string value)
        {
            var isParseSuccessful = decimal.TryParse(value, CultureInfo.InvariantCulture, out decimal result);
            return isParseSuccessful ? result : 0;
        }

        private static decimal ConvertCurrencyToDecimal(string currency)
        {
            decimal amount;
            decimal.TryParse(currency, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-GB"), out amount);
            return amount;
        }

        private async Task SetProducerData(CalcResultsRequestDto resultsRequestDto)
        {
            producerData = await (from run in context.CalculatorRuns
                                  join producerDetail in context.ProducerDetail on run.Id equals producerDetail.CalculatorRunId
                                  join producerMaterial in context.ProducerReportedMaterial on producerDetail.Id equals producerMaterial
                                      .ProducerDetailId
                                  join material in context.Material on producerMaterial.MaterialId equals material.Id
                                  where run.Id == resultsRequestDto.RunId &&
                                      producerMaterial.PackagingType != null &&
                                      (
                                          producerMaterial.PackagingType == PackagingTypes.Household ||
                                          producerMaterial.PackagingType == PackagingTypes.PublicBin ||
                                          (
                                              producerMaterial.PackagingType == PackagingTypes.HouseholdDrinksContainers &&
                                              material.Code == MaterialCodes.Glass
                                          )
                                      )
                                  select new ProducerData
                                  {
                                      MaterialName = material.Name,
                                      PackagingType = producerMaterial.PackagingType,
                                      Tonnage = producerMaterial.PackagingTonnage
                                  }).ToListAsync();
        }
    }
}
