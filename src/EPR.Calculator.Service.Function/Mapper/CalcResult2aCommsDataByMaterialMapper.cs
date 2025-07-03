using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EPR.Calculator.Service.Common.Utils;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResult2ACommsDataByMaterialMapper : ICalcResult2ACommsDataByMaterialMapper
    {
        public CalcResult2ACommsDataByMaterial Map(IEnumerable<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial)
        {
            return new CalcResult2ACommsDataByMaterial
            {
                CalcResult2aCommsDataDetails = GetMaterialBreakdown(commsCostByMaterial),
                CalcResult2aCommsDataDetailsTotal = GetTotalRow(commsCostByMaterial.Single(t => t.Name == CommonConstants.Total)),
            };
        }

        public CalcResult2ACommsDataDetailsTotal GetTotalRow(CalcResultCommsCostCommsCostByMaterial commsCostByMaterial)
        {
            return new CalcResult2ACommsDataDetailsTotal
            {
                EnglandCommsCostTotal = CurrencyConverter.ConvertToCurrency(commsCostByMaterial.EnglandValue),
                HouseholdDrinksContainersTonnageTotal = Math.Round(commsCostByMaterial.HouseholdDrinksContainersValue, 3),
                LateReportingTonnageTotal = Math.Round(commsCostByMaterial.LateReportingTonnageValue, 3),
                NorthernIrelandCommsCostTotal = CurrencyConverter.ConvertToCurrency(commsCostByMaterial.NorthernIrelandValue),
                ScotlandCommsCostTotal = CurrencyConverter.ConvertToCurrency(commsCostByMaterial.ScotlandValue),
                WalesCommsCostTotal = CurrencyConverter.ConvertToCurrency(commsCostByMaterial.WalesValue),
                ProducerHouseholdPackagingWasteTonnageTotal = Math.Round(commsCostByMaterial.ProducerReportedHouseholdPackagingWasteTonnageValue, 3),
                PublicBinTonnage = Math.Round(commsCostByMaterial.ReportedPublicBinTonnageValue, 3),
                Total = commsCostByMaterial.Name,
                TotalCommsCostTotal = CurrencyConverter.ConvertToCurrency(commsCostByMaterial.TotalValue),
                TotalTonnageTotal = Math.Round(commsCostByMaterial.ProducerReportedTotalTonnage, 3)
            };
        }

        public IEnumerable<CalcResult2ACommsDataDetails> GetMaterialBreakdown(
           IEnumerable<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial)
        {
            var commsByMaterialDataDetails = new List<CalcResult2ACommsDataDetails>();

            foreach (var item in commsCostByMaterial.Where(t => t.Name != CommonConstants.Total && t.Name != CommonConstants.TwoACommsCostsbyMaterial))
            {
                commsByMaterialDataDetails.Add(new CalcResult2ACommsDataDetails
                {
                    MaterialName = item.Name,
                    ProducerHouseholdPackagingWasteTonnage = Math.Round(item.ProducerReportedHouseholdPackagingWasteTonnageValue, 3),
                    PublicBinTonnage = Math.Round(item.ReportedPublicBinTonnageValue, 3),
                    TotalTonnage = Math.Round(item.ProducerReportedTotalTonnage, 3),
                    HouseholdDrinksContainersTonnage = Math.Round(item.HouseholdDrinksContainersValue, 3),
                    CommsCostByMaterialPricePerTonne = $"£{item.CommsCostByMaterialPricePerTonneValue.ToString("N4", CultureInfo.CreateSpecificCulture("en-GB"))}",
                    EnglandCommsCost = CurrencyConverter.ConvertToCurrency(item.EnglandValue),
                    WalesCommsCost = CurrencyConverter.ConvertToCurrency(item.WalesValue),
                    ScotlandCommsCost = CurrencyConverter.ConvertToCurrency(item.ScotlandValue),
                    NorthernIrelandCommsCost = CurrencyConverter.ConvertToCurrency(item.NorthernIrelandValue),
                    TotalCommsCost = CurrencyConverter.ConvertToCurrency(item.TotalValue),
                    LateReportingTonnage = Math.Round(item.LateReportingTonnageValue, 3)
                });
            }

            return commsByMaterialDataDetails;
        }

    }
}
