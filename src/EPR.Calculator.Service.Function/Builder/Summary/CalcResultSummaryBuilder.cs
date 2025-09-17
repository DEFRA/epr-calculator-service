using System.Runtime.CompilerServices;
using EPR.Calculator.Service.Function.Builder.ParametersOther;

[assembly: InternalsVisibleTo("EPR.Calculator.Service.Function.UnitTests")]
namespace EPR.Calculator.Service.Function.Builder.Summary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
    using EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions;
    using EPR.Calculator.Service.Function.Builder.Summary.Common;
    using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoA;
    using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoBTotalBill;
    using EPR.Calculator.Service.Function.Builder.Summary.LaDataPrepCosts;
    using EPR.Calculator.Service.Function.Builder.Summary.OneAndTwoA;
    using EPR.Calculator.Service.Function.Builder.Summary.OnePlus2A2B2C;
    using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
    using EPR.Calculator.Service.Function.Builder.Summary.ThreeSA;
    using EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer.cs;
    using EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown;
    using EPR.Calculator.Service.Function.Builder.Summary.TwoCCommsCost;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.EntityFrameworkCore;

    public class CalcResultSummaryBuilder : ICalcResultSummaryBuilder
    {
        private readonly ApplicationDBContext context;

        public IEnumerable<CalcResultScaledupProducer> ScaledupProducers { get; set; } = [];

        public IEnumerable<ScaledupOrganisation> ParentOrganisations { get; set; } = [];

        public CalcResultSummaryBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<CalcResultSummary> Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
            // Get and map materials from DB
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await this.context.Material.ToListAsync();
            var materials = Mappers.MaterialMapper.Map(materialsFromDb);

            ScaledupProducers = calcResult.CalcResultScaledupProducers.ScaledupProducers ?? [];

            var runProducerMaterialDetails = await (from pd in this.context.ProducerDetail
                                                    join prm in this.context.ProducerReportedMaterial on pd.Id equals prm.ProducerDetailId
                                                    where pd.CalculatorRunId == runId
                                                    select new CalcResultsProducerAndReportMaterialDetail
                                                    {
                                                        ProducerDetail = pd,
                                                        ProducerReportedMaterial = prm,
                                                    }).ToListAsync();
            var producerDetails = runProducerMaterialDetails.Select(x => x.ProducerDetail).Distinct().ToList();

            var orderedProducerDetails = GetOrderedListOfProducersAssociatedRunId(
                runId, producerDetails);

            var producerInvoicedMaterialNetTonnage = GetPreviousInvoicedTonnageFromDb(resultsRequestDto.FinancialYear);

            var defaultParams = await GetDefaultParamsAsync(resultsRequestDto.RunId);

            // Household + PublicBin + HDC
            var totalPackagingTonnage = GetTotalPackagingTonnagePerRun(runProducerMaterialDetails, materials, runId, ScaledupProducers.ToList());

            // Get parent organisations
            ParentOrganisations = await (from run in this.context.CalculatorRuns
                                         join crodm in this.context.CalculatorRunOrganisationDataMaster on run.CalculatorRunOrganisationDataMasterId equals crodm.Id
                                         join crodd in this.context.CalculatorRunOrganisationDataDetails on crodm.Id equals crodd.CalculatorRunOrganisationDataMasterId
                                         where run.Id == runId && crodd.SubsidaryId == null
                                         select new ScaledupOrganisation
                                         {
                                             OrganisationId = crodd.OrganisationId ?? 0,
                                             OrganisationName = crodd.OrganisationName,
                                             TradingName = crodd.TradingName,
                                         }).Distinct().ToListAsync();

            var result = GetCalcResultSummary(
                orderedProducerDetails,
                materials,
                calcResult,
                totalPackagingTonnage,
                producerInvoicedMaterialNetTonnage,
                defaultParams);

            return result;
        }

        private Task<List<DefaultParamResultsClass>> GetDefaultParamsAsync(int runId)
        {
            return (from run in this.context.CalculatorRuns.AsNoTracking()
                    join defaultMaster in this.context.DefaultParameterSettings.AsNoTracking() on run.DefaultParameterSettingMasterId equals defaultMaster.Id
                    join defaultDetail in this.context.DefaultParameterSettingDetail.AsNoTracking() on defaultMaster.Id equals defaultDetail.DefaultParameterSettingMasterId
                    join defaultTemplate in this.context.DefaultParameterTemplateMasterList.AsNoTracking() on defaultDetail.ParameterUniqueReferenceId equals defaultTemplate.ParameterUniqueReferenceId
                    where run.Id == runId
                    select new DefaultParamResultsClass
                    {
                        ParameterValue = defaultDetail.ParameterValue,
                        ParameterCategory = defaultTemplate.ParameterCategory,
                        ParameterType = defaultTemplate.ParameterType,
                        ParameterUniqueReference = defaultDetail.ParameterUniqueReferenceId
                    }).ToListAsync();
        }


        public CalcResultSummary GetCalcResultSummary(IEnumerable<ProducerDetail> orderedProducerDetails,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            IEnumerable<TotalPackagingTonnagePerRun> TotalPackagingTonnage,
            IEnumerable<ProducerInvoicedDto> ProducerInvoicedMaterialNetTonnage,
            IEnumerable<DefaultParamResultsClass> defaultParams)
        {
            var result = new CalcResultSummary();
            if (orderedProducerDetails.Any())
            {
                var producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();
                foreach (var producer in orderedProducerDetails)
                {
                    // We have to write an additional row if a producer have at least one subsidiary
                    // This additional row will be the total of this producer and its subsidiaries
                    var producersAndSubsidiaries = orderedProducerDetails.Where(pd => pd.ProducerId == producer.ProducerId);

                    // Make sure the total row is written only once
                    if (CanAddTotalRow(producer, producersAndSubsidiaries, producerDisposalFees))
                    {
                        var totalRow = this.GetProducerTotalRow(producersAndSubsidiaries.ToList(), materials, calcResult, producerDisposalFees, false, TotalPackagingTonnage, ProducerInvoicedMaterialNetTonnage);
                        producerDisposalFees.Add(totalRow);
                    }

                    // Calculate the values for the producer
                    producerDisposalFees.Add(this.GetProducerRow(producerDisposalFees, producersAndSubsidiaries.ToList(), producer, materials, calcResult, TotalPackagingTonnage, ProducerInvoicedMaterialNetTonnage));
                }

                // Calculate the total for all the producers

                var allTotalRow = this.GetProducerTotalRow(orderedProducerDetails.ToList(), materials, calcResult, producerDisposalFees, true, TotalPackagingTonnage, ProducerInvoicedMaterialNetTonnage);
                producerDisposalFees.Add(allTotalRow);

                result.ProducerDisposalFees = producerDisposalFees;

                // Section-(1) & (2a)
                result.TotalFeeforLADisposalCostswoBadDebtprovision1 = CalcResultOneAndTwoAUtil.GetTotalDisposalCostswoBadDebtprovision1(producerDisposalFees);
                result.BadDebtProvisionFor1 = CalcResultOneAndTwoAUtil.GetTotalBadDebtprovision1(producerDisposalFees);
                result.TotalFeeforLADisposalCostswithBadDebtprovision1 = CalcResultOneAndTwoAUtil.GetTotalDisposalCostswithBadDebtprovision1(producerDisposalFees);

                result.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A = CalcResultOneAndTwoAUtil.GetTotalCommsCostswoBadDebtprovision2A(producerDisposalFees);
                result.BadDebtProvisionFor2A = CalcResultOneAndTwoAUtil.GetTotalBadDebtprovision2A(producerDisposalFees);
                result.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = CalcResultOneAndTwoAUtil.GetTotalCommsCostswithBadDebtprovision2A(producerDisposalFees);

                // 2b comms total
                result.CommsCostHeaderWithoutBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithoutBadDebtFor2bTitle(calcResult);
                result.CommsCostHeaderBadDebtProvisionFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderBadDebtProvisionFor2bTitle(calcResult, result);
                result.CommsCostHeaderWithBadDebtFor2bTitle = CalcResultSummaryUtil.GetCommsCostHeaderWithBadDebtFor2bTitle(result);

                TwoCCommsCostUtil.UpdateHeaderTotal(calcResult, result);

                // Section Total bill (1 + 2a + 2b + 2c)
                OnePlus2A2B2CProducer.SetValues(result);

                // Section-3 SA Operating costs section
                ThreeSaCostsProducer.GetProducerSetUpCostsSection3(calcResult, result);

                // Section-4 LA data prep costs
                LaDataPrepCostsProducer.SetValues(calcResult, result);

                // Section-5 SA setup costs
                SaSetupCostsProducer.GetProducerSetUpCosts(calcResult, result);

                // Total bill section
                TotalBillBreakdownProducer.SetValues(result);

                // Billing instructions section
                BillingInstructionsProducer.SetValues(result, ProducerInvoicedMaterialNetTonnage, defaultParams, this.context, calcResult.CalcResultDetail.RunId);
            }

            // Set headers with calculated column index
            CalcResultSummaryUtil.SetHeaders(result, materials);

            return result;
        }

        public static IEnumerable<ProducerDetail> GetOrderedListOfProducersAssociatedRunId(
            int runId,
            IEnumerable<ProducerDetail> producerDetails)
        {
            return producerDetails.Where(pd => pd.CalculatorRunId == runId).OrderBy(pd => pd.ProducerId).ThenBy(pd => pd.SubsidiaryId).ToList();
        }

        public static IEnumerable<CalcResultsProducerAndReportMaterialDetail> GetProducerRunMaterialDetails(
            IEnumerable<ProducerDetail> producerDetails,
            IEnumerable<ProducerReportedMaterial> producerReportedmaterials,
            int runId)
        {
            return (from p in producerDetails
                    join m in producerReportedmaterials
                    on p.Id equals m.ProducerDetailId
                    where p.CalculatorRunId == runId
                    select new CalcResultsProducerAndReportMaterialDetail
                    {
                        ProducerDetail = p,
                        ProducerReportedMaterial = m,
                    }).ToList();
        }

        public CalcResultSummaryProducerDisposalFees GetProducerTotalRow(
            List<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            bool isOverAllTotalRow,
            IEnumerable<TotalPackagingTonnagePerRun> totalPackagingTonnage,
            IEnumerable<ProducerInvoicedDto> ProducerInvoicedMaterialNetTonnage)
        {
            var materialCosts = GetMaterialCosts(producersAndSubsidiaries, producerDisposalFees, materials, calcResult, isOverAllTotalRow, ProducerInvoicedMaterialNetTonnage);
            var communicationCosts = GetCommunicationCosts(producersAndSubsidiaries, materials, calcResult);

            // Compute Count/Advice for the producer-total (Level 1) row
            string? tonnageChangeCount = null;
            string? tonnageChangeAdvice = null;
            if (!isOverAllTotalRow)
            {
                (tonnageChangeCount, tonnageChangeAdvice) =
                    TonnageChangeUtil.ComputeCountAndAdvice(
                        CommonConstants.LevelOne.ToString(),
                        materialCosts);
            }

            var producerForTotalRow = GetProducerDetailsForTotalRow(producersAndSubsidiaries[0].ProducerId, isOverAllTotalRow);
            const int overallTotalId = 0;

            var localAuthorityDisposalCostsSectionOne = GetLocalAuthorityDisposalCostsSectionOne(materialCosts);

            var totalRow = new CalcResultSummaryProducerDisposalFees
            {
                ProducerIdInt = isOverAllTotalRow ? overallTotalId : producersAndSubsidiaries[0].ProducerId,
                ProducerId = isOverAllTotalRow ? string.Empty : producersAndSubsidiaries[0].ProducerId.ToString(),
                ProducerName = producerForTotalRow?.OrganisationName ?? string.Empty,
                SubsidiaryId = string.Empty,
                TradingName = producerForTotalRow?.TradingName ?? string.Empty,
                Level = isOverAllTotalRow ? string.Empty : CommonConstants.LevelOne.ToString(),
                IsProducerScaledup = GetScaledupProducerStatusTotalRow(producersAndSubsidiaries[0], ScaledupProducers, isOverAllTotalRow),
                ProducerDisposalFeesByMaterial = materialCosts,

                // Disposal fee summary
                TotalProducerDisposalFee = CalcResultSummaryUtil.GetTotalProducerDisposalFee(materialCosts),
                BadDebtProvision = CalcResultSummaryUtil.GetTotalBadDebtProvision(materialCosts),
                TotalProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCosts),
                EnglandTotal = CalcResultSummaryUtil.GetEnglandTotal(materialCosts),
                WalesTotal = CalcResultSummaryUtil.GetWalesTotal(materialCosts),
                ScotlandTotal = CalcResultSummaryUtil.GetScotlandTotal(materialCosts),
                NorthernIrelandTotal = CalcResultSummaryUtil.GetNorthernIrelandTotal(materialCosts),

                // For Comms Start
                TotalProducerCommsFee = CalcResultSummaryUtil.GetTotalProducerCommsFee(communicationCosts),
                BadDebtProvisionComms = CalcResultSummaryUtil.GetCommsTotalBadDebtProvision(communicationCosts),
                TotalProducerCommsFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(communicationCosts),
                EnglandTotalComms = CalcResultSummaryUtil.GetEnglandCommsTotal(communicationCosts),
                WalesTotalComms = CalcResultSummaryUtil.GetWalesCommsTotal(communicationCosts),
                ScotlandTotalComms = CalcResultSummaryUtil.GetScotlandCommsTotal(communicationCosts),
                NorthernIrelandTotalComms = CalcResultSummaryUtil.GetNorthernIrelandCommsTotal(communicationCosts),
                ProducerCommsFeesByMaterial = communicationCosts,

                // Set Count/Advice on the producer-total row
                TonnageChangeCount = tonnageChangeCount,
                TonnageChangeAdvice = tonnageChangeAdvice,

                // Section 1
                LocalAuthorityDisposalCostsSectionOne = GetLocalAuthorityDisposalCostsSectionOne(materialCosts),

                // Section 2a
                CommunicationCostsSectionTwoA = GetCommunicationCostsSectionTwoA(communicationCosts),

                // Section 2b
                CommunicationCostsSectionTwoB = GetCommunicationCostsSectionTwoB(calcResult, producersAndSubsidiaries, totalPackagingTonnage),

                // Section-3
                // Percentage of Producer Reported Tonnage vs All Producers
                PercentageofProducerReportedTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducersTotal(producersAndSubsidiaries, totalPackagingTonnage),

                isTotalRow = true,
                isOverallTotalRow = isOverAllTotalRow,
            };

            TwoCCommsCostUtil.UpdateTwoCTotals(calcResult, producerDisposalFees, isOverAllTotalRow, totalRow, producersAndSubsidiaries, totalPackagingTonnage);

            return totalRow;
        }

        public CalcResultSummaryProducerDisposalFees GetProducerRow(
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFeesLookup,
            List<ProducerDetail> producerAndSubsidiaries,
            ProducerDetail producer,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            IEnumerable<TotalPackagingTonnagePerRun> TotalPackagingTonnage,
            IEnumerable<ProducerInvoicedDto> ProducerInvoicedMaterialNetTonnage)
        {
            var materialCostSummary = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>();
            var commsCostSummary = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>();
            var level = CalcResultSummaryUtil.GetLevelIndex(producerDisposalFeesLookup, producer);

            var result = new CalcResultSummaryProducerDisposalFees
            {
                ProducerIdInt = producer.ProducerId,
                ProducerId = producer.ProducerId.ToString(),
                ProducerName = producer.ProducerName ?? string.Empty,
                SubsidiaryId = producer.SubsidiaryId ?? string.Empty,
                TradingName = producer.TradingName ?? string.Empty,
                Level = level.ToString(),
                IsProducerScaledup = CalcResultSummaryUtil.IsProducerScaledup(producer, ScaledupProducers)
                    ? CommonConstants.ScaledupProducersYes
                    : CommonConstants.ScaledupProducersNo,
            };
            foreach (var material in materials)
            {
                var householdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.Household, ScaledupProducers);
                var publicBinTonnage = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.PublicBin, ScaledupProducers);
                var previousInvoicedNetTonnage = ProducerInvoicedMaterialNetTonnage
                                                    .Where(x => x.InvoicedTonnage?.MaterialId == material.Id && x.InvoicedTonnage.ProducerId == result.ProducerIdInt)
                                                    .Select(x => x.InvoicedTonnage?.InvoicedNetTonnage)
                                                    .FirstOrDefault();

                var calcResultSummaryProducerDisposalFeesByMaterial = new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdPackagingWasteTonnage = householdPackagingWasteTonnage,
                    PublicBinTonnage = publicBinTonnage,
                    TotalReportedTonnage = CalcResultSummaryUtil.GetReportedTonnage(producer, material, ScaledupProducers),
                    ManagedConsumerWasteTonnage = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.ConsumerWaste, ScaledupProducers),
                    NetReportedTonnage = CalcResultSummaryUtil.GetNetReportedTonnage(producer, material, ScaledupProducers, level),
                    PricePerTonne = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
                    ProducerDisposalFee = CalcResultSummaryUtil.GetProducerDisposalFee(producer, producerAndSubsidiaries, material, calcResult, ScaledupProducers),
                    BadDebtProvision = CalcResultSummaryUtil.GetBadDebtProvision(producer, producerAndSubsidiaries, material, calcResult, ScaledupProducers),
                    ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvision(producer, producerAndSubsidiaries, material, calcResult, ScaledupProducers),
                    EnglandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(producer, producerAndSubsidiaries, material, calcResult, Countries.England, ScaledupProducers),
                    WalesWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(producer, producerAndSubsidiaries, material, calcResult, Countries.Wales, ScaledupProducers),
                    ScotlandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(producer, producerAndSubsidiaries, material, calcResult, Countries.Scotland, ScaledupProducers),
                    NorthernIrelandWithBadDebtProvision = CalcResultSummaryUtil.GetCountryBadDebtProvision(producer, producerAndSubsidiaries, material, calcResult, Countries.NorthernIreland, ScaledupProducers),
                    PreviousInvoicedTonnage = previousInvoicedNetTonnage.HasValue ? previousInvoicedNetTonnage.Value : null,
                };

                calcResultSummaryProducerDisposalFeesByMaterial.TonnageChange =
                    TonnageChangeUtil.ComputePerMaterialChange(
                        result.Level,
                        calcResultSummaryProducerDisposalFeesByMaterial.NetReportedTonnage,
                        calcResultSummaryProducerDisposalFeesByMaterial.PreviousInvoicedTonnage);

                materialCostSummary.Add(material.Code, calcResultSummaryProducerDisposalFeesByMaterial);

                if (material.Code == MaterialCodes.Glass && materialCostSummary.TryGetValue(material.Code, out var producerDisposalFees))
                {
                    producerDisposalFees.HouseholdDrinksContainersTonnage = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.HouseholdDrinksContainers, ScaledupProducers);
                }

                result.TotalProducerDisposalFee += calcResultSummaryProducerDisposalFeesByMaterial.ProducerDisposalFee;
                result.BadDebtProvision += calcResultSummaryProducerDisposalFeesByMaterial.BadDebtProvision;
                result.TotalProducerDisposalFeeWithBadDebtProvision += calcResultSummaryProducerDisposalFeesByMaterial.ProducerDisposalFeeWithBadDebtProvision;
                result.EnglandTotal += calcResultSummaryProducerDisposalFeesByMaterial.EnglandWithBadDebtProvision;
                result.WalesTotal += calcResultSummaryProducerDisposalFeesByMaterial.WalesWithBadDebtProvision;
                result.ScotlandTotal += calcResultSummaryProducerDisposalFeesByMaterial.ScotlandWithBadDebtProvision;
                result.NorthernIrelandTotal += calcResultSummaryProducerDisposalFeesByMaterial.NorthernIrelandWithBadDebtProvision;

                var calcResultSummaryProducerCommsFeesCostByMaterial = new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = householdPackagingWasteTonnage,
                    ReportedPublicBinTonnage = publicBinTonnage,
                    TotalReportedTonnage = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnage(producer, material, ScaledupProducers),
                    PriceperTonne = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
                    ProducerTotalCostWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvision(producer, material, calcResult, ScaledupProducers),
                    BadDebtProvision = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCost(producer, material, calcResult, ScaledupProducers),
                    ProducerTotalCostwithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvision(producer, material, calcResult, ScaledupProducers),
                    EnglandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForComms(producer, material, calcResult, ScaledupProducers),
                    WalesWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForComms(producer, material, calcResult, ScaledupProducers),
                    ScotlandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForComms(producer, material, calcResult, ScaledupProducers),
                    NorthernIrelandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForComms(producer, material, calcResult, ScaledupProducers),
                };

                commsCostSummary.Add(material.Code, calcResultSummaryProducerCommsFeesCostByMaterial);

                if (material.Code == MaterialCodes.Glass && commsCostSummary.TryGetValue(material.Code, out var comm))
                {
                    comm.HouseholdDrinksContainers = CalcResultSummaryUtil.GetTonnage(producer, material, PackagingTypes.HouseholdDrinksContainers, ScaledupProducers);
                }

                result.TotalProducerCommsFee += calcResultSummaryProducerCommsFeesCostByMaterial.ProducerTotalCostWithoutBadDebtProvision;
                result.BadDebtProvisionComms += calcResultSummaryProducerCommsFeesCostByMaterial.BadDebtProvision;
                result.TotalProducerCommsFeeWithBadDebtProvision += calcResultSummaryProducerCommsFeesCostByMaterial.ProducerTotalCostwithBadDebtProvision;
                result.EnglandTotalComms += calcResultSummaryProducerCommsFeesCostByMaterial.EnglandWithBadDebtProvision;
                result.WalesTotalComms += calcResultSummaryProducerCommsFeesCostByMaterial.WalesWithBadDebtProvision;
                result.ScotlandTotalComms += calcResultSummaryProducerCommsFeesCostByMaterial.ScotlandWithBadDebtProvision;
                result.NorthernIrelandTotalComms += calcResultSummaryProducerCommsFeesCostByMaterial.NorthernIrelandWithBadDebtProvision;
            }

            result.ProducerDisposalFeesByMaterial = materialCostSummary;
            result.ProducerCommsFeesByMaterial = commsCostSummary;

            // Section 1
            result.LocalAuthorityDisposalCostsSectionOne = new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision = result.TotalProducerDisposalFee,
                BadDebtProvision = result.BadDebtProvision,
                TotalProducerFeeWithBadDebtProvision = result.TotalProducerDisposalFeeWithBadDebtProvision,
                EnglandTotalWithBadDebtProvision = result.EnglandTotal,
                WalesTotalWithBadDebtProvision = result.WalesTotal,
                ScotlandTotalWithBadDebtProvision = result.ScotlandTotal,
                NorthernIrelandTotalWithBadDebtProvision = result.NorthernIrelandTotal
            };

            // Section 2a
            result.CommunicationCostsSectionTwoA = new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision = result.TotalProducerCommsFee,
                BadDebtProvision = result.BadDebtProvisionComms,
                TotalProducerFeeWithBadDebtProvision = result.TotalProducerCommsFeeWithBadDebtProvision,
                EnglandTotalWithBadDebtProvision = result.EnglandTotalComms,
                WalesTotalWithBadDebtProvision = result.WalesTotalComms,
                ScotlandTotalWithBadDebtProvision = result.ScotlandTotalComms,
                NorthernIrelandTotalWithBadDebtProvision = result.NorthernIrelandTotalComms
            };

            // Section 2b
            result.CommunicationCostsSectionTwoB = new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2b(calcResult, producer, TotalPackagingTonnage),
                BadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2b(calcResult, producer, TotalPackagingTonnage),
                TotalProducerFeeWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2b(calcResult, producer, TotalPackagingTonnage),
                EnglandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebt(calcResult, producer, TotalPackagingTonnage),
                WalesTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebt(calcResult, producer, TotalPackagingTonnage),
                ScotlandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebt(calcResult, producer, TotalPackagingTonnage),
                NorthernIrelandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebt(calcResult, producer, TotalPackagingTonnage)
            };

            var (countStr, advice) = TonnageChangeUtil.ComputeCountAndAdvice(result.Level, materialCostSummary);
            result.TonnageChangeCount = countStr;
            result.TonnageChangeAdvice = advice;

            // Section-3
            // Percentage of Producer Reported Tonnage vs All Producers
            result.PercentageofProducerReportedTonnagevsAllProducers = TonnageVsAllProducerUtil.GetPercentageofProducerReportedTonnagevsAllProducers(producer, TotalPackagingTonnage);

            TwoCCommsCostUtil.UpdateTwoCRows(calcResult, result, producer, TotalPackagingTonnage);

            return result;
        }

        public IEnumerable<ProducerInvoicedDto> GetPreviousInvoicedTonnageFromDb(string financialYear)
        {
            var validClassificationStatuses = new[]
            {
                RunClassificationStatusIds.INITIALRUNCOMPLETEDID,
                RunClassificationStatusIds.INTERMRECALCULATIONRUNCOMPID,
                RunClassificationStatusIds.FINALRECALCULATIONRUNCOMPID,
                RunClassificationStatusIds.FINALRUNCOMPLETEDID
            };

            var previousInvoicedNetTonnage =
                (from calc in context.CalculatorRuns.AsNoTracking()
                 join b in context.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                     on calc.Id equals b.CalculatorRunId
                 join p in context.ProducerDesignatedRunInvoiceInstruction.AsNoTracking()
                     on new { b.CalculatorRunId, b.ProducerId }
                     equals new { p.CalculatorRunId, p.ProducerId }
                 join t in context.ProducerInvoicedMaterialNetTonnage.AsNoTracking()
                     on new { calc.Id, p.ProducerId } equals new { Id = t.CalculatorRunId, t.ProducerId }
                 where validClassificationStatuses.Contains(calc.CalculatorRunClassificationId)
                       && calc.FinancialYearId == financialYear
                       && b.BillingInstructionAcceptReject == PrepareBillingFileConstants.BillingInstructionAccepted
                       && b.SuggestedBillingInstruction != PrepareBillingFileConstants.SuggestedBillingInstructionCancelBill
                       //not exists clause -- to exclude previous "net tonnage" and "current year invoice total to date" values if cancel bill has been accepted since.
                       && !(from calc2 in context.CalculatorRuns.AsNoTracking()
                           join b2 in context.ProducerResultFileSuggestedBillingInstruction.AsNoTracking()
                               on calc2.Id equals b2.CalculatorRunId
                           where b2.ProducerId == p.ProducerId
                                 && b2.BillingInstructionAcceptReject == PrepareBillingFileConstants.BillingInstructionAccepted
                                 && b2.SuggestedBillingInstruction == PrepareBillingFileConstants.SuggestedBillingInstructionCancelBill
                                 && calc2.FinancialYearId == financialYear
                                 && validClassificationStatuses.Contains(calc2.CalculatorRunClassificationId)
                                 && calc2.Id > calc.Id select 1).Any() 
                 select new { calc, p, t })
                .AsEnumerable()
                .GroupBy(x => new { x.p.ProducerId, x.t.MaterialId })
                .Select(g =>
                {
                    var latest = g.OrderByDescending(x => x.calc.Id).First();
                    return new ProducerInvoicedDto
                    {
                        InvoicedTonnage = latest.t,
                        CalculatorRunId = latest.calc.Id,
                        InvoiceInstruction = latest.p
                    };
                })
                .OrderBy(x => x.InvoicedTonnage?.ProducerId)
                .ThenBy(x => x.InvoicedTonnage?.MaterialId)
                .ToList();
            

            return previousInvoicedNetTonnage;
        }

        public static IEnumerable<TotalPackagingTonnagePerRun> GetTotalPackagingTonnagePerRun(
            IEnumerable<CalcResultsProducerAndReportMaterialDetail> allResults,
            IEnumerable<MaterialDetail> materials, int runId, IEnumerable<CalcResultScaledupProducer> scaledupProducers)
        {
            var allProducerDetails = allResults.Select(x => x.ProducerDetail).Distinct();
            var filteredProducers = allProducerDetails.Where(t => scaledupProducers.All(i => i.ProducerId != t.ProducerId)).ToList();
            var allProducerReportedMaterials = allResults.Select(x => x.ProducerReportedMaterial);

            var result =
                (from p in filteredProducers
                 join pm in allProducerReportedMaterials on p.Id equals pm.ProducerDetailId
                 join m in materials on pm.MaterialId equals m.Id
                 where p.CalculatorRunId == runId &&
           (
               pm.PackagingType == PackagingTypes.Household || pm.PackagingType == PackagingTypes.PublicBin ||
               (
                   pm.PackagingType == PackagingTypes.HouseholdDrinksContainers && m.Code == MaterialCodes.Glass)
           )
                 group new { m = pm, p } by new { p.ProducerId, p.SubsidiaryId } into g
                 select new TotalPackagingTonnagePerRun
                 {
                     ProducerId = g.Key.ProducerId,
                     SubsidiaryId = g.Key.SubsidiaryId,
                     TotalPackagingTonnage = g.Sum(x => x.m.PackagingTonnage),
                 }).ToList();

            var distinctSProducers = scaledupProducers.Where(t => !t.IsTotalRow).Where(t => !t.IsSubtotalRow).Select(t => new { t.ProducerId, t.SubsidiaryId }).Distinct();

            foreach (var item in distinctSProducers)
            {
                decimal total = 0;

                var sProducers = scaledupProducers.Where(t => t.ProducerId == item.ProducerId && t.SubsidiaryId == item.SubsidiaryId);
                foreach (var scaledupProducer in sProducers)
                {
                    total += scaledupProducer.ScaledupProducerTonnageByMaterial.Sum(x => x.Value.ScaledupTotalReportedTonnage);
                }

                result.Add(new TotalPackagingTonnagePerRun() { ProducerId = item.ProducerId, SubsidiaryId = item.SubsidiaryId, TotalPackagingTonnage = total });
            }

            return result;
        }

        public bool CanAddTotalRow(ProducerDetail producer,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFees)
        {
            var parentProducer = ParentOrganisations.FirstOrDefault(po => po.OrganisationId == producer.ProducerId);
            if (parentProducer == null)
            {
                return false;
            }

            var pomDataExistsForParentProducer = producersAndSubsidiaries.Any(ps => ps.ProducerId == parentProducer.OrganisationId && ps.SubsidiaryId == null);
            if (producersAndSubsidiaries.Count() > 1 || !pomDataExistsForParentProducer)
            {
                if (producerDisposalFees.Find(pdf => pdf.ProducerId == producer.ProducerId.ToString()) == null)
                {
                    return true;
                }
            }

            return false;
        }

        public ScaledupOrganisation? GetProducerDetailsForTotalRow(int producerId, bool isOverAllTotalRow)
        {
            if (isOverAllTotalRow)
            {
                return null;
            }

            var parentProducer = ParentOrganisations.FirstOrDefault(po => po.OrganisationId == producerId);
            return parentProducer;
        }

        internal static string GetScaledupProducerStatusTotalRow(
            ProducerDetail producer,
            IEnumerable<CalcResultScaledupProducer> scaledupProducers,
            bool isOverAllTotalRow)
        {
            if (isOverAllTotalRow)
            {
                return CommonConstants.Totals;
            }

            return CalcResultSummaryUtil.IsProducerScaledup(producer, scaledupProducers)
                ? CommonConstants.ScaledupProducersYes
                : CommonConstants.ScaledupProducersNo;
        }

        private Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> GetMaterialCosts(
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            bool isOverAllTotalRow,
            IEnumerable<ProducerInvoicedDto> ProducerInvoicedMaterialNetTonnage)
        {
            var materialCosts = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>();

            foreach (var material in materials)
            {
                var householdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnageTotal(producersAndSubsidiaries, material, PackagingTypes.Household, ScaledupProducers);
                var publicBinTonnage = CalcResultSummaryUtil.GetTonnageTotal(producersAndSubsidiaries, material, PackagingTypes.PublicBin, ScaledupProducers);
                var previousInvoicedNetTonnage = ProducerInvoicedMaterialNetTonnage
                                                   .Where(x => x.InvoicedTonnage?.MaterialId == material.Id
                                                            && x.InvoicedTonnage?.ProducerId == producersAndSubsidiaries.FirstOrDefault()?.ProducerId)
                                                   .Select(x => x.InvoicedTonnage?.InvoicedNetTonnage)
                                                   .FirstOrDefault();

                // Net reported for this totals context (producer total or overall total)
                var netReportedTonnage = MaterialCostsUtil.GetNetReportedTonnage(
                    producerDisposalFees, producersAndSubsidiaries, ScaledupProducers, material, isOverAllTotalRow);

                // Compute TonnageChange per the story:
                // - Overall totals row: sum of Level-1 values
                // - Producer totals row (Level 1): per-material logic using net - previous, with null/zero handling
                decimal? tonnageChange = isOverAllTotalRow
                    ? TonnageChangeUtil.GetOverallChangeTotal(producerDisposalFees, material.Code)
                    : TonnageChangeUtil.ComputePerMaterialChange(
                          CommonConstants.LevelOne.ToString(), // producer “total” row is Level 1 for this column
                          netReportedTonnage,
                          previousInvoicedNetTonnage);

                materialCosts.Add(material.Code, new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdPackagingWasteTonnage = householdPackagingWasteTonnage,
                    PublicBinTonnage = publicBinTonnage,
                    TotalReportedTonnage = CalcResultSummaryUtil.GetReportedTonnageTotal(producersAndSubsidiaries, material, ScaledupProducers),
                    ManagedConsumerWasteTonnage = CalcResultSummaryUtil.GetTonnageTotal(producersAndSubsidiaries, material, PackagingTypes.ConsumerWaste, ScaledupProducers),
                    NetReportedTonnage = netReportedTonnage,
                    PricePerTonne = CalcResultSummaryUtil.GetPricePerTonne(material, calcResult),
                    ProducerDisposalFee = MaterialCostsUtil.GetProducerDisposalFee(producerDisposalFees, producersAndSubsidiaries, ScaledupProducers, material, calcResult, isOverAllTotalRow),
                    BadDebtProvision = MaterialCostsUtil.GetBadDebtProvision(producerDisposalFees, producersAndSubsidiaries, ScaledupProducers, material, calcResult, isOverAllTotalRow),
                    ProducerDisposalFeeWithBadDebtProvision = MaterialCostsUtil.GetProducerDisposalFeeWithBadDebtProvision(producerDisposalFees, producersAndSubsidiaries, ScaledupProducers, material, calcResult, isOverAllTotalRow),
                    EnglandWithBadDebtProvision = MaterialCostsUtil.GetCountryDisposalFeeWithBadDebtProvision(producerDisposalFees, producersAndSubsidiaries, ScaledupProducers, material, calcResult, Countries.England, isOverAllTotalRow),
                    WalesWithBadDebtProvision = MaterialCostsUtil.GetCountryDisposalFeeWithBadDebtProvision(producerDisposalFees, producersAndSubsidiaries, ScaledupProducers, material, calcResult, Countries.Wales, isOverAllTotalRow),
                    ScotlandWithBadDebtProvision = MaterialCostsUtil.GetCountryDisposalFeeWithBadDebtProvision(producerDisposalFees, producersAndSubsidiaries, ScaledupProducers, material, calcResult, Countries.Scotland, isOverAllTotalRow),
                    NorthernIrelandWithBadDebtProvision = MaterialCostsUtil.GetCountryDisposalFeeWithBadDebtProvision(producerDisposalFees, producersAndSubsidiaries, ScaledupProducers, material, calcResult, Countries.NorthernIreland, isOverAllTotalRow),
                    PreviousInvoicedTonnage = MaterialCostsUtil.GetPreviousInvoicedTonnage(producerDisposalFees, producersAndSubsidiaries, ScaledupProducers, material, isOverAllTotalRow, previousInvoicedNetTonnage),
                    TonnageChange = tonnageChange
                });

                if (material.Code == MaterialCodes.Glass && materialCosts.TryGetValue(material.Code, out var materialCost))
                {
                    materialCost.HouseholdDrinksContainersTonnage = CalcResultSummaryUtil.GetTonnageTotal(
                        producersAndSubsidiaries, material, PackagingTypes.HouseholdDrinksContainers, ScaledupProducers);
                }

            }

            return materialCosts;
        }

        private Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> GetCommunicationCosts(
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult)
        {
            var communicationCosts = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            foreach (var material in materials)
            {
                var householdPackagingWasteTonnage = CalcResultSummaryUtil.GetTonnageTotal(producersAndSubsidiaries, material, PackagingTypes.Household, ScaledupProducers);
                var publicBinTonnage = CalcResultSummaryUtil.GetTonnageTotal(producersAndSubsidiaries, material, PackagingTypes.PublicBin, ScaledupProducers);

                communicationCosts.Add(material.Code, new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = householdPackagingWasteTonnage,
                    ReportedPublicBinTonnage = publicBinTonnage,
                    TotalReportedTonnage = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnageTotal(producersAndSubsidiaries, material, ScaledupProducers),
                    PriceperTonne = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(material, calcResult),
                    ProducerTotalCostWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvisionTotal(producersAndSubsidiaries, material,
                            calcResult, ScaledupProducers),
                    BadDebtProvision = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCostTotal(producersAndSubsidiaries, material, calcResult, ScaledupProducers),
                    ProducerTotalCostwithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvisionTotal(producersAndSubsidiaries, material, calcResult, ScaledupProducers),
                    EnglandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult, ScaledupProducers),
                    WalesWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult, ScaledupProducers),
                    ScotlandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material, calcResult, ScaledupProducers),
                    NorthernIrelandWithBadDebtProvision = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForCommsTotal(producersAndSubsidiaries, material,
                            calcResult, ScaledupProducers),
                });

                if (material.Code == MaterialCodes.Glass && communicationCosts.TryGetValue(material.Code, out var comm))
                {
                    comm.HouseholdDrinksContainers = CalcResultSummaryUtil.GetTonnageTotal(producersAndSubsidiaries, material, PackagingTypes.HouseholdDrinksContainers, ScaledupProducers);
                }
            }

            return communicationCosts;
        }

        private CalcResultSummaryBadDebtProvision GetLocalAuthorityDisposalCostsSectionOne(
            Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            return new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFee(materialCostSummary),
                BadDebtProvision = CalcResultSummaryUtil.GetTotalBadDebtProvision(materialCostSummary),
                TotalProducerFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerDisposalFeeWithBadDebtProvision(materialCostSummary),
                EnglandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetEnglandTotal(materialCostSummary),
                WalesTotalWithBadDebtProvision = CalcResultSummaryUtil.GetWalesTotal(materialCostSummary),
                ScotlandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetScotlandTotal(materialCostSummary),
                NorthernIrelandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetNorthernIrelandTotal(materialCostSummary)
            };
        }

        private CalcResultSummaryBadDebtProvision GetCommunicationCostsSectionTwoA(
            Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            return new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFee(commsCostSummary),
                BadDebtProvision = CalcResultSummaryUtil.GetCommsTotalBadDebtProvision(commsCostSummary),
                TotalProducerFeeWithBadDebtProvision = CalcResultSummaryUtil.GetTotalProducerCommsFeeWithBadDebtProvision(commsCostSummary),
                EnglandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetEnglandCommsTotal(commsCostSummary),
                WalesTotalWithBadDebtProvision = CalcResultSummaryUtil.GetWalesCommsTotal(commsCostSummary),
                ScotlandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetScotlandCommsTotal(commsCostSummary),
                NorthernIrelandTotalWithBadDebtProvision = CalcResultSummaryUtil.GetNorthernIrelandCommsTotal(commsCostSummary),
            };
        }

        private CalcResultSummaryBadDebtProvision GetCommunicationCostsSectionTwoB(
            CalcResult calcResult, IEnumerable<ProducerDetail> producersAndSubsidiaries, IEnumerable<TotalPackagingTonnagePerRun> totalPackagingTonnage)
        {
            return new CalcResultSummaryBadDebtProvision
            {
                TotalProducerFeeWithoutBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithoutBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
                BadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsBadDebtProvisionFor2bTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
                TotalProducerFeeWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsProducerFeeWithBadDebtFor2bTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
                EnglandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsEnglandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
                WalesTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsWalesWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
                ScotlandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsScotlandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage),
                NorthernIrelandTotalWithBadDebtProvision = CalcResultSummaryCommsCostTwoBTotalBill.GetCommsNorthernIrelandWithBadDebtTotalsRow(calcResult, producersAndSubsidiaries, totalPackagingTonnage)
            };
        }
    }
}