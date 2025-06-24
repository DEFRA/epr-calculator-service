using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResultLaDisposalCostDataMapper : ICalcResultLaDisposalCostDataMapper
    {
        public CalcResultLaDisposalCostDataJson Map(List<CalcResultLaDisposalCostDataDetail> laDisposalCostDataDetail)
        {
            return new CalcResultLaDisposalCostDataJson
            {
                Name = CommonConstants.LADisposalCostData,
                CalcResultLaDisposalCostDetails = GetMaterialBreakdown(laDisposalCostDataDetail),
                CalcResultLaDisposalCostDataDetailsTotal = GetTotalRow(laDisposalCostDataDetail),
            };
        }

        public CalcResultLaDisposalCostDataDetailsTotal GetTotalRow(List<CalcResultLaDisposalCostDataDetail> laDisposalCostDataDetail)
        {
            var laDisposalCostDetailTotal = laDisposalCostDataDetail
    .SingleOrDefault(t => t.Name == CommonConstants.Total);
            if (laDisposalCostDetailTotal == null)
            {
                return new CalcResultLaDisposalCostDataDetailsTotal()
                {
                    EnglandLaDisposalCostTotal = string.Empty,
                    HouseholdDrinksContainersTonnageTotal = 0,
                    LateReportingTonnageTotal = 0,
                    NorthernIrelandLaDisposalCostTotal = string.Empty,
                    ProducerHouseholdPackagingWasteTonnageTotal = 0,
                    PublicBinTonnage = 0,
                    ScotlandLaDisposalCostTotal = string.Empty,
                    Total = string.Empty,
                    TotalLaDisposalCostTotal = string.Empty,
                    TotalTonnageTotal = 0,
                    WalesLaDisposalCostTotal = string.Empty
                };
            }
            return new CalcResultLaDisposalCostDataDetailsTotal
            {
                Total = laDisposalCostDetailTotal.Name,
                EnglandLaDisposalCostTotal = CurrencyConverter.FormatAsGbpCurrency(laDisposalCostDetailTotal.England),
                WalesLaDisposalCostTotal = CurrencyConverter.FormatAsGbpCurrency(laDisposalCostDetailTotal.Wales),
                ScotlandLaDisposalCostTotal = CurrencyConverter.FormatAsGbpCurrency(laDisposalCostDetailTotal.Scotland),
                NorthernIrelandLaDisposalCostTotal = CurrencyConverter.FormatAsGbpCurrency(laDisposalCostDetailTotal.NorthernIreland),
                TotalLaDisposalCostTotal = CurrencyConverter.FormatAsGbpCurrency(laDisposalCostDetailTotal.Total),
                ProducerHouseholdPackagingWasteTonnageTotal = CurrencyConverter.GetDecimalValue(laDisposalCostDetailTotal.ProducerReportedHouseholdPackagingWasteTonnage),
                PublicBinTonnage = CurrencyConverter.GetDecimalValue(laDisposalCostDetailTotal.ReportedPublicBinTonnage),
                HouseholdDrinksContainersTonnageTotal = CurrencyConverter.GetDecimalValue(laDisposalCostDetailTotal.HouseholdDrinkContainers),
                LateReportingTonnageTotal = CurrencyConverter.GetDecimalValue(laDisposalCostDetailTotal.LateReportingTonnage),
                TotalTonnageTotal = laDisposalCostDetailTotal.TotalReportedTonnage != null ? CurrencyConverter.GetDecimalValue(laDisposalCostDetailTotal.TotalReportedTonnage) : 0.00M,
            };

        }

        public IEnumerable<CalcResultLaDisposalCostDetails> GetMaterialBreakdown(
           List<CalcResultLaDisposalCostDataDetail> laDisposalCostDataDetail)
        {
            var commsByMaterialDataDetails = new List<CalcResultLaDisposalCostDetails>();

            foreach (var item in laDisposalCostDataDetail.Where(t => t.Name != CommonConstants.Total))
            {
                commsByMaterialDataDetails.Add(new CalcResultLaDisposalCostDetails
                {
                    MaterialName = item.Name,
                    EnglandLaDisposalCost = CurrencyConverter.FormatAsGbpCurrency(item.England),
                    WalesLaDisposalCost = CurrencyConverter.FormatAsGbpCurrency(item.Wales),
                    ScotlandLaDisposalCost = CurrencyConverter.FormatAsGbpCurrency(item.Scotland),
                    NorthernIrelandLaDisposalCost = CurrencyConverter.FormatAsGbpCurrency(item.NorthernIreland),
                    TotalLaDisposalCost = CurrencyConverter.FormatAsGbpCurrency(item.Total),
                    ProducerHouseholdPackagingWasteTonnage = CurrencyConverter.GetDecimalValue(item.ProducerReportedHouseholdPackagingWasteTonnage),
                    PublicBinTonnage = CurrencyConverter.GetDecimalValue(item.ReportedPublicBinTonnage),
                    HouseholdDrinksContainersTonnage = CurrencyConverter.GetDecimalValue(item.HouseholdDrinkContainers),
                    LateReportingTonnage = CurrencyConverter.GetDecimalValue(item.LateReportingTonnage),
                    TotalTonnage = CurrencyConverter.GetDecimalValue(item.ProducerReportedTotalTonnage),
                    DisposalCostPricePerTonne = item.DisposalCostPricePerTonne != null ? CurrencyConverter.FormatAsGbpCurrency(item.DisposalCostPricePerTonne) : "£0.00",
                });
            }

            return commsByMaterialDataDetails;
        }
    }
}
