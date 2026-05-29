using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions;
using EPR.Calculator.Service.Function.Builder.Summary.LaDataPrepCosts;
using EPR.Calculator.Service.Function.Builder.Summary.OnePlus2A2B2C;
using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
using EPR.Calculator.Service.Function.Builder.Summary.ThreeSa;
using EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown;
using EPR.Calculator.Service.Function.Builder.Summary.TwoCCommsCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;

[ExcludeFromCodeCoverage]
public static class CalcResultSummaryUtil
{
    private const int decimalRoundUp = 2;

    public static (List<CalcResultSummaryHeader>, List<CalcResultSummaryHeader>, List<CalcResultSummaryHeader>) GetHeaders(
        CalcResultSummary resultSummary,
        IReadOnlyList<MaterialDetail> materials,
        bool applyModulation
    )
    {
        int section1MaterialsIdx           = 1                             + StartingHeaders().Count();
        int section1DisposalFeeIdx         = section1MaterialsIdx          + Section1Materials(materials, applyModulation).Count();
        int section2aMaterialsIdx          = section1DisposalFeeIdx        + Section1DisposalFee().Count();
        int section2aCommsIdx              = section2aMaterialsIdx         + Section2aMaterials(materials).Count();
        int section1DisposalIdx            = section2aCommsIdx             + Section1Disposal().Count();
        int section2aComms2aIdx            = section1DisposalIdx           + Section2aComms().Count();
        int commsCost2aPercentageIdx       = section2aComms2aIdx           + CommsCost2aPercentage().Count();
        int commsCost2bIdx                 = commsCost2aPercentageIdx      + CommsCost2b().Count();
        int commsCost2cIdx                 = commsCost2bIdx                + CommsCost2c().Count();
        int onePlus2A2B2CProducerIdx       = commsCost2cIdx                + CommsCost2c().Count();
        int threeSaCostsSummaryIdx         = onePlus2A2B2CProducerIdx      + OnePlus2A2B2CProducer.GetHeaders().Count;
        int laDataPrepCostsProducerIdx     = threeSaCostsSummaryIdx        + ThreeSaCostsProducer.GetHeaders().Count;
        int saSetupCostsSummaryIdx         = laDataPrepCostsProducerIdx    + LaDataPrepCostsProducer.GetHeaders().Count;
        int totalBillBreakdownProducerIdx  = saSetupCostsSummaryIdx        + SaSetupCostsProducer.GetHeaders().Count;
        int billingInstructionsProducerIdx = totalBillBreakdownProducerIdx + TotalBillBreakdownProducer.GetHeaders().Count;

        var producerDisposalFeesHeaders = new List<CalcResultSummaryHeader>();
        producerDisposalFeesHeaders.AddRange([
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.OneProducerDisposalFeesWithBadDebtProvision, ColumnIndex = section1MaterialsIdx },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeader, ColumnIndex = section2aMaterialsIdx },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforLADisposalCostswoBadDebtprovision1, ColumnIndex = section1DisposalIdx },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision, ColumnIndex = section1DisposalIdx + 1 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforLADisposalCostswithBadDebtprovision1, ColumnIndex = section1DisposalIdx + 2 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforCommsCostsbyMaterialwoBadDebtprovision2A, ColumnIndex = section2aComms2aIdx },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision, ColumnIndex = section2aComms2aIdx + 1 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.FeeforCommsCostsbyMaterialwithBadDebtprovision2A, ColumnIndex = section2aComms2aIdx + 2 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeaderWithoutBadDebtFor2bTitle, ColumnIndex = commsCost2bIdx },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeaderBadDebtProvisionFor2bTitle, ColumnIndex = commsCost2bIdx + 1 },
            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostHeaderWithBadDebtFor2bTitle,ColumnIndex = commsCost2bIdx + 2 },
            new CalcResultSummaryHeader { Name = TwoCCommsConstantsHeader.TwoCCommsCostByCountryWithout, ColumnIndex = commsCost2cIdx },
            new CalcResultSummaryHeader { Name = TwoCCommsConstantsHeader.TwoCCommsCostBadBebtProvision, ColumnIndex = commsCost2cIdx + 1 },
            new CalcResultSummaryHeader { Name = TwoCCommsConstantsHeader.TwoCCommsCostByCountryWithBadDebt, ColumnIndex = commsCost2cIdx + 2 },
        ]);
        producerDisposalFeesHeaders.AddRange(OnePlus2A2B2CProducer.GetSummaryHeaders(onePlus2A2B2CProducerIdx));
        producerDisposalFeesHeaders.AddRange(ThreeSaCostsProducer.GetSummaryHeaders(threeSaCostsSummaryIdx));
        producerDisposalFeesHeaders.AddRange(LaDataPrepCostsProducer.GetSummaryHeaders(laDataPrepCostsProducerIdx));
        producerDisposalFeesHeaders.AddRange(SaSetupCostsProducer.GetSummaryHeaders(saSetupCostsSummaryIdx));
        producerDisposalFeesHeaders.AddRange(TotalBillBreakdownProducer.GetSummaryHeaders(totalBillBreakdownProducerIdx));
        producerDisposalFeesHeaders.AddRange(BillingInstructionsProducer.GetSummaryHeaders(billingInstructionsProducerIdx));

        var materialsBreakdownHeaders = new List<CalcResultSummaryHeader>();
        var columnIndex = section1MaterialsIdx;
        foreach (var material in materials)
        {
            materialsBreakdownHeaders.Add(new CalcResultSummaryHeader { Name = $"{material.Name} Breakdown", ColumnIndex = columnIndex});
            columnIndex += Section1Materials([material], applyModulation).Count();
        }
        materialsBreakdownHeaders.Add(new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.DisposalFeeSummary, ColumnIndex = section1DisposalFeeIdx });

        var commsCostColumnIndex = section2aMaterialsIdx;
        foreach (var material in materials)
        {
            materialsBreakdownHeaders.Add(new CalcResultSummaryHeader { Name = $"{material.Name} Breakdown", ColumnIndex = commsCostColumnIndex });
            commsCostColumnIndex += Section2aMaterials([material]).Count();
        }
        materialsBreakdownHeaders.Add(new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostSummaryHeader, ColumnIndex = commsCostColumnIndex});
        materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(section1DisposalIdx       , resultSummary.TotalFeeforLADisposalCostswoBadDebtprovision1      , resultSummary.BadDebtProvisionFor1                        , resultSummary.TotalFeeforLADisposalCostswithBadDebtprovision1));
        materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(section2aComms2aIdx       , resultSummary.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A, resultSummary.BadDebtProvisionFor2A                       , resultSummary.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A));
        materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(commsCost2bIdx            , resultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle            , resultSummary.CommsCostHeaderBadDebtProvisionFor2bTitle   , resultSummary.CommsCostHeaderWithBadDebtFor2bTitle));
        materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(commsCost2cIdx            , resultSummary.TwoCCommsCostsByCountryWithoutBadDebtProvision     , resultSummary.TwoCBadDebtProvision                        , resultSummary.TwoCCommsCostsByCountryWithBadDebtProvision));
        materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(onePlus2A2B2CProducerIdx  , resultSummary.TotalOnePlus2A2B2CFeeWithBadDebtProvision));
        materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(threeSaCostsSummaryIdx    , resultSummary.SaOperatingCostsWoTitleSection3                    , resultSummary.BadDebtProvisionTitleSection3               , resultSummary.SaOperatingCostsWithTitleSection3));
        materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(laDataPrepCostsProducerIdx, resultSummary.LaDataPrepCostsTitleSection4                       , resultSummary.LaDataPrepCostsBadDebtProvisionTitleSection4, resultSummary.LaDataPrepCostsWithBadDebtProvisionTitleSection4));
        materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(saSetupCostsSummaryIdx    , resultSummary.SaSetupCostsTitleSection5                          , resultSummary.SaSetupCostsBadDebtProvisionTitleSection5   , resultSummary.SaSetupCostsWithBadDebtProvisionTitleSection5));

        var columnHeaders = new List<CalcResultSummaryHeader>();
        columnHeaders.AddRange(StartingHeaders());
        columnHeaders.AddRange(Section1Materials(materials, applyModulation));
        columnHeaders.AddRange(Section1DisposalFee());
        columnHeaders.AddRange(Section2aMaterials(materials));
        columnHeaders.AddRange(Section2aComms());
        columnHeaders.AddRange(Section1Disposal());
        columnHeaders.AddRange(Section2aComms2a());
        columnHeaders.AddRange(CommsCost2aPercentage());
        columnHeaders.AddRange(CommsCost2b());
        columnHeaders.AddRange(CommsCost2c());
        columnHeaders.AddRange(OnePlus2A2B2CProducer.GetHeaders());
        columnHeaders.AddRange(ThreeSaCostsProducer.GetHeaders());
        columnHeaders.AddRange(LaDataPrepCostsProducer.GetHeaders());
        columnHeaders.AddRange(SaSetupCostsProducer.GetHeaders());
        columnHeaders.AddRange(TotalBillBreakdownProducer.GetHeaders());
        columnHeaders.AddRange(BillingInstructionsProducer.GetHeaders());

        return (producerDisposalFeesHeaders, materialsBreakdownHeaders, columnHeaders);
    }

    private static IEnumerable<CalcResultSummaryHeader> CreateMoneyHeaders(int columnIndex, params decimal[] values)
    {
        return values.Select((value, i) => new CalcResultSummaryHeader { Name = $"£{Math.Round(value, decimalRoundUp)}", ColumnIndex = columnIndex + i});
    }

    private static IEnumerable<CalcResultSummaryHeader> CreateHeaders(params string[] names)
    {
        return names.Select((name, i) => new CalcResultSummaryHeader { Name = name});
    }

    private static IEnumerable<CalcResultSummaryHeader> StartingHeaders()
    {
        return CreateHeaders(
            CalcResultSummaryHeaders.ProducerId,
            CalcResultSummaryHeaders.SubsidiaryId,
            CalcResultSummaryHeaders.ProducerOrSubsidiaryName,
            CalcResultSummaryHeaders.TradingName,
            CalcResultSummaryHeaders.Level,
            CalcResultSummaryHeaders.ScaledupTonnages,
            CalcResultSummaryHeaders.PartialCalculation,
            CalcResultSummaryHeaders.StatusCode,
            CalcResultSummaryHeaders.JoinersDate,
            CalcResultSummaryHeaders.LeaversDate);
    }

    private static IEnumerable<CalcResultSummaryHeader> Section1Materials(IReadOnlyList<MaterialDetail> materials, bool applyModulation)
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

    private static IEnumerable<CalcResultSummaryHeader> Section1DisposalFee()
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

    private static IEnumerable<CalcResultSummaryHeader> Section2aMaterials(IReadOnlyList<MaterialDetail> materials)
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

    private static IEnumerable<CalcResultSummaryHeader> Section2aComms()
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

    private static IEnumerable<CalcResultSummaryHeader> Section1Disposal()
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

    private static IEnumerable<CalcResultSummaryHeader> Section2aComms2a()
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

    private static IEnumerable<CalcResultSummaryHeader> CommsCost2aPercentage()
    {
        return CreateHeaders(CalcResultSummaryHeaders.PercentageofProducerTonnagevsAllProducers);
    }

    private static IEnumerable<CalcResultSummaryHeader> CommsCost2b()
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

    private static IEnumerable<CalcResultSummaryHeader> CommsCost2c()
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
