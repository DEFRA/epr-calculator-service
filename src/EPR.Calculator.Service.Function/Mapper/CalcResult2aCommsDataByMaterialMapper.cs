using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CalcResult2aCommsDataByMaterialMapper : ICalcResult2aCommsDataByMaterialMapper
    {
        public CalcResult2aCommsDataByMaterial Map(List<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial)
        {
            return new CalcResult2aCommsDataByMaterial
            {
                CalcResult2aCommsDataDetails = GetMaterialBreakdown(commsCostByMaterial)
            };
        }

        public IEnumerable<CalcResult2aCommsDataDetails> GetMaterialBreakdown(
           List<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial)
        {
            var commsByMaterialDataDetails = new List<CalcResult2aCommsDataDetails>();

            foreach (var item in commsCostByMaterial)
            {
                commsByMaterialDataDetails.Add(new CalcResult2aCommsDataDetails
                {
                    MaterialName = item.Name,
                    ProducerHouseholdPackagingWasteTonnage = item.ProducerReportedHouseholdPackagingWasteTonnageValue,
                    PublicBinTonnage = item.ReportedPublicBinTonnageValue,
                    TotalTonnage = item.ProducerReportedTotalTonnage,
                    HouseholdDrinksContainersTonnage = item.HouseholdDrinksContainersValue,
                    CommsCostByMaterialPricePerTonne = item.CommsCostByMaterialPricePerTonneValue,
                    EnglandCommsCost = item.EnglandValue,
                    WalesCommsCost = item.WalesValue,
                    ScotlandCommsCost = item.ScotlandValue,
                    NorthernIrelandCommsCost = item.NorthernIrelandValue,
                    TotalCommsCost = item.TotalValue,
                    LateReportingTonnage = item.LateReportingTonnageValue
                });
            }

            return commsByMaterialDataDetails;
        }
    }
}
