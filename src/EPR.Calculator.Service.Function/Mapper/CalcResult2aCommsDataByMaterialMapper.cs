using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                CalcResult2ACommsDataDetails = GetMaterialBreakdown(commsCostByMaterial)
            };
        }

        public IEnumerable<CalcResult2ACommsDataDetails> GetMaterialBreakdown(
           IEnumerable<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial)
        {
            var commsByMaterialDataDetails = new List<CalcResult2ACommsDataDetails>();

            foreach (var item in commsCostByMaterial)
            {
                commsByMaterialDataDetails.Add(new CalcResult2ACommsDataDetails
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
