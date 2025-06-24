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
    public class CalcResult2ACommsDataByMaterialMapper : ICalcResult2ACommsDataByMaterialMapper
    {
        public CalcResult2ACommsDataByMaterial Map(List<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial)
        {
            return new CalcResult2ACommsDataByMaterial
            {
                CalcResult2ACommsDataDetails = GetMaterialBreakdown(commsCostByMaterial),
                CalcResult2ACommsDataDetailsTotal = GetTotalRow(commsCostByMaterial.Single(t => t.Name == CommonConstants.Total)),
            };
        }

        public calcResult2aCommsDataDetailsTotal GetTotalRow(CalcResultCommsCostCommsCostByMaterial commsCostByMaterial)
        {  
            return new calcResult2aCommsDataDetailsTotal
            {
                EnglandCommsCostTotal = CurrencyUtil.ConvertToCurrency(commsCostByMaterial.EnglandValue),
                HouseholdDrinksContainersTonnageTotal = commsCostByMaterial.HouseholdDrinksContainersValue,
                LateReportingTonnageTotal = commsCostByMaterial.LateReportingTonnageValue,
                NorthernIrelandCommsCostTotal = CurrencyUtil.ConvertToCurrency(commsCostByMaterial.NorthernIrelandValue),
                ScotlandCommsCostTotal = CurrencyUtil.ConvertToCurrency(commsCostByMaterial.ScotlandValue),
                WalesCommsCostTotal = CurrencyUtil.ConvertToCurrency(commsCostByMaterial.WalesValue),
                ProducerHouseholdPackagingWasteTonnageTotal = commsCostByMaterial.ProducerReportedHouseholdPackagingWasteTonnageValue,
                PublicBinTonnage = commsCostByMaterial.ReportedPublicBinTonnageValue,
                Total = commsCostByMaterial.Name,
                TotalCommsCostTotal = CurrencyUtil.ConvertToCurrency(commsCostByMaterial.TotalValue),
                TotalTonnageTotal = commsCostByMaterial.ProducerReportedTotalTonnage
            };
        }

        public IEnumerable<CalcResult2ACommsDataDetails> GetMaterialBreakdown(
           List<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial)
        {
            var commsByMaterialDataDetails = new List<CalcResult2ACommsDataDetails>();

            foreach (var item in commsCostByMaterial.Where(t=>t.Name != CommonConstants.Total))
            {
                commsByMaterialDataDetails.Add(new CalcResult2ACommsDataDetails
                {
                    MaterialName = item.Name,
                    ProducerHouseholdPackagingWasteTonnage = item.ProducerReportedHouseholdPackagingWasteTonnageValue,
                    PublicBinTonnage = item.ReportedPublicBinTonnageValue,
                    TotalTonnage = item.ProducerReportedTotalTonnage,
                    HouseholdDrinksContainersTonnage = item.HouseholdDrinksContainersValue,
                    CommsCostByMaterialPricePerTonne = CurrencyUtil.ConvertToCurrency(item.CommsCostByMaterialPricePerTonneValue),
                    EnglandCommsCost = CurrencyUtil.ConvertToCurrency(item.EnglandValue),
                    WalesCommsCost = CurrencyUtil.ConvertToCurrency(item.WalesValue),
                    ScotlandCommsCost = CurrencyUtil.ConvertToCurrency(item.ScotlandValue),
                    NorthernIrelandCommsCost = CurrencyUtil.ConvertToCurrency(item.NorthernIrelandValue),
                    TotalCommsCost = CurrencyUtil.ConvertToCurrency(item.TotalValue),
                    LateReportingTonnage = item.LateReportingTonnageValue
                });
            }

            return commsByMaterialDataDetails;
        }

    }
}
