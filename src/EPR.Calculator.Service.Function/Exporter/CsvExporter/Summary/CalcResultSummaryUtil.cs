using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function.Builder.Summary.TwoCCommsCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

[ExcludeFromCodeCoverage]
public static class CalcResultSummaryUtil
{
    private static IEnumerable<CalcResultSummaryHeader> CreateHeaders(params string[] names)
    {
        return names.Select((name, i) => new CalcResultSummaryHeader { Name = name});
    }

    internal static IEnumerable<CalcResultSummaryHeader> Section1Materials(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
    {
        return materials.SelectMany(material =>
        {
            var headers = CreateHeaders(CalcResultSummaryHeaders.PreviousInvoicedTonnage)
                .ToList();

            headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.HouseholdPackagingWasteTonnage));
            if (applyModulation)
            {
                headers.AddRange(CreateHeaders(
                    CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageRed,
                    CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageAmber,
                    CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageGreen,
                    CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageRedMedical,
                    CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageAmberMedical,
                    CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageGreenMedical));
            }

            headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.PublicBinTonnage));
            if (applyModulation)
            {
                headers.AddRange(CreateHeaders(
                    CalcResultSummaryHeaders.PublicBinTonnageRed,
                    CalcResultSummaryHeaders.PublicBinTonnageAmber,
                    CalcResultSummaryHeaders.PublicBinTonnageGreen,
                    CalcResultSummaryHeaders.PublicBinTonnageRedMedical,
                    CalcResultSummaryHeaders.PublicBinTonnageAmberMedical,
                    CalcResultSummaryHeaders.PublicBinTonnageGreenMedical));
            }

            if (material.Code == MaterialCodes.Glass)
            {
                headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.HouseholdDrinksContainersTonnage));
                if (applyModulation)
                {
                    headers.AddRange(CreateHeaders(
                        CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageRed,
                        CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageAmber,
                        CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageGreen,
                        CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageRedMedical,
                        CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageAmberMedical,
                        CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageGreenMedical));
                }
            }

            if (applyModulation) {
                headers.AddRange(CreateHeaders(
                    CalcResultSummaryHeaders.TotalTonnage,
                    CalcResultSummaryHeaders.RedTotalTonnage,
                    CalcResultSummaryHeaders.AmberTotalTonnage,
                    CalcResultSummaryHeaders.GreenTotalTonnage,
                    CalcResultSummaryHeaders.RedMedicalTotalTonnage,
                    CalcResultSummaryHeaders.AmberMedicalTotalTonnage,
                    CalcResultSummaryHeaders.GreenMedicalTotalTonnage,
                    CalcResultSummaryHeaders.RedPlusRedMedicalTotalTonnage,
                    CalcResultSummaryHeaders.AmberPlusAmberMedicalTotalTonnage,
                    CalcResultSummaryHeaders.GreenPlusGreenMedicalTotalTonnage,
                    CalcResultSummaryHeaders.SelfManagedConsumerWasteTonnage,
                    CalcResultSummaryHeaders.ActionedSelfManagedConsumerWasteTonnage,
                    CalcResultSummaryHeaders.RedActionedSelfManagedConsumerWasteTonnage,
                    CalcResultSummaryHeaders.AmberActionedSelfManagedConsumerWasteTonnage,
                    CalcResultSummaryHeaders.GreenActionedSelfManagedConsumerWasteTonnage,
                    CalcResultSummaryHeaders.NetTonnage,
                    CalcResultSummaryHeaders.RedPlusRedMedicalNetTonnage,
                    CalcResultSummaryHeaders.AmberPlusAmberMedicalNetTonnage,
                    CalcResultSummaryHeaders.GreenPlusGreenMedicalNetTonnage,
                    CalcResultSummaryHeaders.ResidualSelfManagedConsumerWasteTonnage
                ));
            } else {
                headers.AddRange(CreateHeaders(
                    CalcResultSummaryHeaders.TotalTonnage,
                    CalcResultSummaryHeaders.SelfManagedConsumerWasteTonnage,
                    CalcResultSummaryHeaders.NetTonnage
                ));
            }

            headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.TonnageChange));
            if (applyModulation) {
                headers.AddRange(CreateHeaders(
                    CalcResultSummaryHeaders.RedPlusRedMedicalMaterialPricePerTonne,
                    CalcResultSummaryHeaders.AmberPlusAmberMedicalMaterialPricePerTonne,
                    CalcResultSummaryHeaders.GreenPlusGreenMedicalMaterialPricePerTonne,
                    CalcResultSummaryHeaders.ProducerRedPlusRedMedicalMaterialDisposalCost,
                    CalcResultSummaryHeaders.ProducerAmberPlusAmberMedicalMaterialDisposalCost,
                    CalcResultSummaryHeaders.ProducerGreenPlusGreenMedicalMaterialDisposalCost));
            } else {
                headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.PricePerTonne));
            }

            headers.AddRange(CreateHeaders(
                CalcResultSummaryHeaders.ProducerDisposalFee,
                CalcResultSummaryHeaders.BadDebtProvision,
                CalcResultSummaryHeaders.ProducerDisposalFeeWithBadDebtProvision,
                CalcResultSummaryHeaders.EnglandWithBadDebtProvision,
                CalcResultSummaryHeaders.WalesWithBadDebtProvision,
                CalcResultSummaryHeaders.ScotlandWithBadDebtProvision,
                CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision));

            return headers;
        });
    }

    internal static IEnumerable<CalcResultSummaryHeader> Section1DisposalFee()
    {
        return CreateHeaders(
            CalcResultSummaryHeaders.TotalProducerDisposalFee,
            CalcResultSummaryHeaders.BadDebtProvision,
            CalcResultSummaryHeaders.TotalProducerDisposalFeeWithBadDebtProvision,
            CalcResultSummaryHeaders.EnglandTotal,
            CalcResultSummaryHeaders.WalesTotal,
            CalcResultSummaryHeaders.ScotlandTotal,
            CalcResultSummaryHeaders.NorthernIrelandTotal,
            CalcResultSummaryHeaders.TonnageChangeCount,
            CalcResultSummaryHeaders.TonnageChangeAdvice);
    }

    internal static IEnumerable<CalcResultSummaryHeader> Section2aMaterials(IReadOnlyList<MaterialDetail> materials)
    {
        return materials.SelectMany(material =>
        {
            var headers = new List<CalcResultSummaryHeader>();
            headers.AddRange(CreateHeaders(
                CalcResultSummaryHeaders.HouseholdPackagingWasteTonnage,
                CalcResultSummaryHeaders.PublicBinTonnage));

            if (material.Code == MaterialCodes.Glass)
            {
                headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.HouseholdDrinksContainersTonnage));
            }

            headers.AddRange(CreateHeaders(
                CalcResultSummaryHeaders.TotalTonnage,
                CalcResultSummaryHeaders.PricePerTonne,
                CalcResultSummaryHeaders.ProducerTotalCostWithoutBadDebtProvision,
                CalcResultSummaryHeaders.BadDebtProvision,
                CalcResultSummaryHeaders.ProducerTotalCostwithBadDebtProvision,
                CalcResultSummaryHeaders.EnglandWithBadDebtProvision,
                CalcResultSummaryHeaders.WalesWithBadDebtProvision,
                CalcResultSummaryHeaders.ScotlandWithBadDebtProvision,
                CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision
            ));

            return headers;
        });
    }

    internal static IEnumerable<CalcResultSummaryHeader> Section2aComms()
    {
        return CreateHeaders(
            CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision,
            CalcResultSummaryHeaders.TotalBadDebtProvision,
            CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision,
            CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision,
            CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision,
            CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision,
            CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision);
    }

    internal static IEnumerable<CalcResultSummaryHeader> Section1Disposal()
    {
        return CreateHeaders(
            CalcResultSummaryHeaders.TotalProducerFeeforLADisposalCostswoBadDebtprovision,
            CalcResultSummaryHeaders.BadDebtProvisionFor1,
            CalcResultSummaryHeaders.TotalProducerFeeforLADisposalCostswithBadDebtprovision,
            CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision,
            CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision,
            CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision,
            CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision);
    }

    internal static IEnumerable<CalcResultSummaryHeader> Section2aComms2a()
    {
        return CreateHeaders(
            CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision2A,
            CalcResultSummaryHeaders.BadDebtProvisionfor2A,
            CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision2A,
            CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision,
            CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision,
            CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision,
            CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision);
    }

    internal static IEnumerable<CalcResultSummaryHeader> CommsCost2aPercentage()
    {
        return CreateHeaders(CalcResultSummaryHeaders.PercentageofProducerTonnagevsAllProducers);
    }

    internal static IEnumerable<CalcResultSummaryHeader> CommsCost2b()
    {
        return CreateHeaders(
            CalcResultSummaryHeaders.ProducerFeeWithoutBadDebtForComms2b,
            CalcResultSummaryHeaders.BadDebtProvisionForComms2b,
            CalcResultSummaryHeaders.ProducerFeeForCommsCostsWithBadDebtForComms2b,
            CalcResultSummaryHeaders.EnglandTotalWithBadDebtProvisionForComms2b,
            CalcResultSummaryHeaders.WalesTotalWithBadDebtProvisionForComms2b,
            CalcResultSummaryHeaders.ScotlandTotalWithBadDebtProvisionForComms2b,
            CalcResultSummaryHeaders.NorthernIrelandTotalWithBadDebtProvisionForComms2b);
    }

    internal static IEnumerable<CalcResultSummaryHeader> CommsCost2c()
    {
        return CreateHeaders(
            TwoCCommsCostSubColumnHeader.TwoCCommsCostCountryInPropertionWithoutBadDebt,
            TwoCCommsCostSubColumnHeader.TwoCCommsCostBadDebtProvision,
            TwoCCommsCostSubColumnHeader.TwoCCommsCostCountryInPropertionWithBadDebt,
            TwoCCommsCostSubColumnHeader.TwoCCommsCostEnglandWithBadDebt,
            TwoCCommsCostSubColumnHeader.TwoCCommsCostWalesWithBadDebt,
            TwoCCommsCostSubColumnHeader.TwoCCommsCostScotlandWithBadDebt,
            TwoCCommsCostSubColumnHeader.TwoCCommsCostNIWithBadDebt);
    }

    public static decimal GetCommsCostHeaderWithoutBadDebtFor2bTitle(CalcResult calcResult)
    {
        return calcResult.CalcResultCommsCostReportDetail.CommsCostUkWide.Total;
    }

    public static decimal GetCommsCostHeaderBadDebtProvisionFor2bTitle(
        CalcResult calcResult,
        CalcResultSummary calcResultSummary)
    {
        var commsCost = calcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle;
        var badDebtProvision = calcResult.CalcResultParameterOtherCost.BadDebtValue / 100;
        return commsCost * badDebtProvision;
    }

    public static decimal GetCommsCostHeaderWithBadDebtFor2bTitle(CalcResultSummary calcResultSummary)
    {
        var commsCostHeaderWithoutBadDebt = calcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle;
        var commsCostHeaderBadDebtProvision = calcResultSummary.CommsCostHeaderBadDebtProvisionFor2bTitle;
        return commsCostHeaderWithoutBadDebt + commsCostHeaderBadDebtProvision;
    }
}
