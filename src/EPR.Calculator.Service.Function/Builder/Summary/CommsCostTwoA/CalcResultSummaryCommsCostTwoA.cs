using System.Globalization;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Builder.Summary.Common;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoA
{
    public static class CalcResultSummaryCommsCostTwoA
    {
        public static decimal GetEnglandWithBadDebtProvisionForCommsTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer =>
                GetEnglandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult)
            );
        }

        public static decimal GetWalesWithBadDebtProvisionForCommsTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetWalesWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult));
        }

        public static decimal GetScotlandWithBadDebtProvisionForCommsTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetScotlandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult));
        }

        public static decimal GetNorthernIrelandWithBadDebtProvisionForCommsTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetNorthernIrelandWithBadDebtProvisionForComms(projectedMaterialsLookup, producer, material, calcResult));
        }

        public static decimal GetProducerTotalCostWithoutBadDebtProvisionTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetProducerTotalCostWithoutBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult));
        }

        public static decimal GetBadDebtProvisionForCommsCostTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetBadDebtProvisionForCommsCost(projectedMaterialsLookup, producer, material, calcResult));
        }

        public static decimal GetEnglandWithBadDebtProvisionForComms(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            // TODO was getting 4th? Also `calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.Last()` - should be Last for Total?
            // Looks to be getting `CalcResultSummaryCommsCostTwoA.OnePlusFourApportionment` entry - should be in the model.
            // Also `%s` is a UI thing - the values are actually coefficients
            // TODO refactor like `CalcResultSummaryCommsCostTwoBTotalBill#GetRegionApportionment`
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Where(a => a.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).First().EnglandTotal;
        }

        public static decimal GetWalesWithBadDebtProvisionForComms(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Where(a => a.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).First().WalesTotal;
        }

        public static decimal GetScotlandWithBadDebtProvisionForComms(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Where(a => a.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).First().ScotlandTotal;
        }

        public static decimal GetNorthernIrelandWithBadDebtProvisionForComms(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var producerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult);
            return producerTotalCostwithBadDebtProvision * calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.Where(a => a.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment).First().NorthernIrelandTotal;
        }

        public static decimal GetPriceperTonneForComms(MaterialDetail material, CalcResult calcResult)
        {
            var commsCostDataDetail = calcResult.CalcResultCommsCostReportDetail.CalcResultCommsCostCommsCostByMaterial.FirstOrDefault(la => la.Name == material.Name);

            if (commsCostDataDetail == null)
            {
                return 0;
            }

            return commsCostDataDetail.CommsCostByMaterialPricePerTonne ?? 0m;
        }

        public static decimal GetProducerTotalCostwithBadDebtProvision(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue;
            var producerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult);
            return producerTotalCostWithoutBadDebtProvision * (1 + badDebtProvision / 100);
        }

        public static decimal GetTotalReportedTonnage(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material
        )
        {
            decimal hdcTonnage = 0;
            var hhPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.Household);
            var reportedPublicBinTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.PublicBin);

            if (material.Code == MaterialCodes.Glass)
            {
                hdcTonnage = CalcResultSummaryUtil.GetTonnage(projectedMaterialsLookup, producer, material, PackagingTypes.HouseholdDrinksContainers);
            }

            return material.Code == MaterialCodes.Glass
                ? hdcTonnage + reportedPublicBinTonnage + hhPackagingWasteTonnage
                : hhPackagingWasteTonnage + reportedPublicBinTonnage;
        }

        public static decimal GetProducerTotalCostWithoutBadDebtProvision(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var priceperTonne = GetPriceperTonneForComms(material, calcResult);
            return GetTotalReportedTonnage(projectedMaterialsLookup, producer, material) * priceperTonne;
        }

        public static decimal GetBadDebtProvisionForCommsCost(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            ProducerDetail producer,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            var badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue;
            var producerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult);
            return producerTotalCostWithoutBadDebtProvision * badDebtProvision / 100;
        }

        public static decimal GetProducerTotalCostwithBadDebtProvisionTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult
        )
        {
            return producers.Sum(producer => GetProducerTotalCostwithBadDebtProvision(projectedMaterialsLookup, producer, material, calcResult));
        }

        public static decimal GetTotalReportedTonnageTotal(
            ILookup<(int, string?), ProducerReportedMaterialProjected> projectedMaterialsLookup,
            IReadOnlyList<ProducerDetail> producers,
            MaterialDetail material
        )
        {
            return producers.Sum(producer => GetTotalReportedTonnage(projectedMaterialsLookup, producer, material));
        }
    }
}
