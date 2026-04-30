using System.Globalization;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoA
{
    public static class CalcResultSummaryCommsCostTwoA
    {
        public static decimal GetEnglandWithBadDebtProvisionForCommsTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetEnglandWithBadDebtProvisionForComms(producer, material, calcResult));
        }

        public static decimal GetWalesWithBadDebtProvisionForCommsTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetWalesWithBadDebtProvisionForComms(producer, material, calcResult));
        }

        public static decimal GetScotlandWithBadDebtProvisionForCommsTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetScotlandWithBadDebtProvisionForComms(producer, material, calcResult));
        }

        public static decimal GetNorthernIrelandWithBadDebtProvisionForCommsTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetNorthernIrelandWithBadDebtProvisionForComms(producer, material, calcResult));
        }

        public static decimal GetProducerTotalCostWithoutBadDebtProvisionTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult));
        }

        public static decimal GetBadDebtProvisionForCommsCostTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetBadDebtProvisionForCommsCost(producer, material, calcResult));
        }

        public static decimal GetEnglandWithBadDebtProvisionForComms(
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * (Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.EnglandDisposalTotal).ToList()[4].Trim('%')) / 100);
        }

        public static decimal GetWalesWithBadDebtProvisionForComms(
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * (Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.WalesDisposalTotal).ToList()[4].Trim('%')) / 100);
        }

        public static decimal GetScotlandWithBadDebtProvisionForComms(
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * (Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.ScotlandDisposalTotal).ToList()[4].Trim('%')) / 100);
        }

        public static decimal GetNorthernIrelandWithBadDebtProvisionForComms(
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * (Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Select(x => x.NorthernIrelandDisposalTotal).ToList()[4].Trim('%')) / 100);
        }

        public static decimal GetPriceperTonneForComms(MaterialDetail material, CalcResult calcResult)
        {
            var commsCostDataDetail = calcResult.CalcResultCommsCostReportDetail.CalcResultCommsCostCommsCostByMaterial.FirstOrDefault(la => la.Name == material.Name);

            if (commsCostDataDetail == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(commsCostDataDetail.CommsCostByMaterialPricePerTonne, NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out decimal value);

            return isParseSuccessful ? value : 0;
        }

        public static decimal GetProducerTotalCostwithBadDebtProvision(
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var badDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%'));
            var producerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult);
            return producerTotalCostWithoutBadDebtProvision * (1 + badDebtProvision / 100);
        }

        public static decimal GetTotalReportedTonnage(
            ProducerDetail producer,
            MaterialDetail material
        )
        {
            decimal hdcTonnage = 0;
            var hhPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.Household);
            var reportedPublicBinTonnage = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.PublicBin);

            if (material.Code == MaterialCodes.Glass)
            {
                hdcTonnage = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.HouseholdDrinksContainers);
            }

            return material.Code == MaterialCodes.Glass
                ? hdcTonnage + reportedPublicBinTonnage + hhPackagingWasteTonnage
                : hhPackagingWasteTonnage + reportedPublicBinTonnage;
        }

        public static decimal GetProducerTotalCostWithoutBadDebtProvision(
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var priceperTonne = GetPriceperTonneForComms(material, calcResult);
            return GetTotalReportedTonnage(producer, material) * priceperTonne;
        }

        public static decimal GetBadDebtProvisionForCommsCost(
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var badDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%'));
            var producerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult);
            return producerTotalCostWithoutBadDebtProvision * badDebtProvision / 100;
        }

        public static decimal GetProducerTotalCostwithBadDebtProvisionTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult));
        }

        public static decimal GetTotalReportedTonnageTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material
        )
        {
            return producers.Sum(producer => GetTotalReportedTonnage(producer, material));
        }
    }
}
