using System.Globalization;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions;
using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoA;
using EPR.Calculator.Service.Function.Builder.Summary.LaDataPrepCosts;
using EPR.Calculator.Service.Function.Builder.Summary.OnePlus2A2B2C;
using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
using EPR.Calculator.Service.Function.Builder.Summary.ThreeSa;
using EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown;
using EPR.Calculator.Service.Function.Builder.Summary.TwoCCommsCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary.Common
{
    public static class CalcResultSummaryUtil
    {
        private const int decimalRoundUp = 2;

        public static int GetLevelIndex(
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFeesLookup,
            ProducerDetail producer)
        {
            var totalRow = producerDisposalFeesLookup.Find(pdf => pdf.ProducerId == producer.ProducerId.ToString() && pdf.isTotalRow);

            return totalRow == null ? (int)CalcResultSummaryLevelIndex.One : (int)CalcResultSummaryLevelIndex.Two;
        }

        public static bool IsProducerScaledup(
            ProducerDetail producer,
            IEnumerable<CalcResultScaledupProducer> scaledupProducers)
        {
            var scaledupProducer = scaledupProducers.FirstOrDefault(p => p.ProducerId == producer.ProducerId);
            return scaledupProducer != null;
        }

        public static bool IsProducerPartiallyObligated(
            ProducerDetail producer,
            IEnumerable<CalcResultPartialObligation> partialObligations,
            bool isTotalRow)
        {
            var partialObligation = isTotalRow ? partialObligations.FirstOrDefault(p => p.ProducerId == producer.ProducerId) : partialObligations.FirstOrDefault(p => p.ProducerId == producer.ProducerId && p.SubsidiaryId == producer.SubsidiaryId);
            return partialObligation != null;
        }

        public static decimal? GetScaledUpTonnage(ProducerDetail producer, MaterialDetail material, string packagingType, IEnumerable<CalcResultScaledupProducer> scaledUpProducers)
        {
            var scaledupProducerForAllSubmissionPeriods = scaledUpProducers.Where(p => p.ProducerId == producer.ProducerId
                && p.SubsidiaryId == producer.SubsidiaryId
                && !p.IsSubtotalRow
                && !p.IsTotalRow);

            if (scaledupProducerForAllSubmissionPeriods.Any())
            {
                decimal tonnage = 0;
                foreach (var scaledupProducerTonnageByMaterial in scaledupProducerForAllSubmissionPeriods.Select(x => x.ScaledupProducerTonnageByMaterial))
                {
                    switch (packagingType)
                    {
                        case PackagingTypes.Household:
                            tonnage += scaledupProducerTonnageByMaterial.GetValueOrDefault(material.Code)?.ScaledupReportedHouseholdPackagingWasteTonnage ?? 0;
                            break;
                        case PackagingTypes.PublicBin:
                            tonnage += scaledupProducerTonnageByMaterial.GetValueOrDefault(material.Code)?.ScaledupReportedPublicBinTonnage ?? 0;
                            break;
                        case PackagingTypes.ConsumerWaste:
                            tonnage += scaledupProducerTonnageByMaterial.GetValueOrDefault(material.Code)?.ScaledupReportedSelfManagedConsumerWasteTonnage ?? 0;
                            break;
                        case PackagingTypes.HouseholdDrinksContainers:
                            tonnage += scaledupProducerTonnageByMaterial.GetValueOrDefault(material.Code)?.ScaledupHouseholdDrinksContainersTonnageGlass ?? 0;
                            break;
                        default:
                            tonnage += 0;
                            break;
                    }
                }

                return tonnage;
            }
            return null;
        }

        public static decimal? GetPartialTonnage(ProducerDetail producer, MaterialDetail material, string packagingType, IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            var maybePartialObligation = partialObligations.FirstOrDefault(p => p.ProducerId == producer.ProducerId && p.SubsidiaryId == producer.SubsidiaryId);

            if (maybePartialObligation != null)
            {
                switch (packagingType)
                {
                    case PackagingTypes.Household:
                        return maybePartialObligation.PartialObligationTonnageByMaterial.GetValueOrDefault(material.Code)?.PartialReportedHouseholdPackagingWasteTonnage ?? 0;
                    case PackagingTypes.PublicBin:
                        return maybePartialObligation.PartialObligationTonnageByMaterial.GetValueOrDefault(material.Code)?.PartialReportedPublicBinTonnage ?? 0;
                    case PackagingTypes.ConsumerWaste:
                        return maybePartialObligation.PartialObligationTonnageByMaterial.GetValueOrDefault(material.Code)?.PartialReportedSelfManagedConsumerWasteTonnage ?? 0;
                    case PackagingTypes.HouseholdDrinksContainers:
                        return maybePartialObligation.PartialObligationTonnageByMaterial.GetValueOrDefault(material.Code)?.PartialHouseholdDrinksContainersTonnageGlass ?? 0;
                    default:
                        return 0;
                }
            }
            return null;
        }

        public static decimal GetTonnage(
            ProducerDetail producer,
            MaterialDetail material,
            string packagingType,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations,
            RagRating? ragRating = null)
        {

            var maybePartialScaledUpTonnage = GetPartialTonnage(producer, material, packagingType, partialObligations);

            if (maybePartialScaledUpTonnage != null) {
                return (decimal)maybePartialScaledUpTonnage;
            }

            var maybeScaledUpTonnage = GetScaledUpTonnage(producer, material, packagingType, scaledUpProducers);

            if (maybeScaledUpTonnage != null) {
                return (decimal)maybeScaledUpTonnage;
            }

            var prms = producer.ProducerReportedMaterials.Where(p => p.Material?.Code == material.Code && p.PackagingType == packagingType);

            return ragRating switch
            {
                null                   => prms.Sum(p => p.PackagingTonnage),
                RagRating.Red          => prms.Sum(p => p.PackagingTonnageRed ?? 0),
                RagRating.Amber        => prms.Sum(p => p.PackagingTonnageAmber ?? 0),
                RagRating.Green        => prms.Sum(p => p.PackagingTonnageGreen ?? 0),
                RagRating.RedMedical   => prms.Sum(p => p.PackagingTonnageRedMedical ?? 0),
                RagRating.AmberMedical => prms.Sum(p => p.PackagingTonnageAmberMedical ?? 0),
                RagRating.GreenMedical => prms.Sum(p => p.PackagingTonnageGreenMedical ?? 0),
                _                      => 0m
            };
        }

        public static decimal GetTonnageTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            string packagingType,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations,
            RagRating? ragRating = null)
        {
            return producers.Sum(producer => GetTonnage(producer, material, packagingType, scaledUpProducers, partialObligations, ragRating));
        }

        public static decimal GetReportedTonnage(
            ProducerDetail producer,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations,
            RagRating? ragRating = null)
        {
            var householdTonnage = GetTonnage(producer, material, PackagingTypes.Household, scaledUpProducers, partialObligations, ragRating);
            var publicBinTonnage = GetTonnage(producer, material, PackagingTypes.PublicBin, scaledUpProducers, partialObligations, ragRating);
            var glassTonnage = material.Code == MaterialCodes.Glass
                ? GetTonnage(producer, material, PackagingTypes.HouseholdDrinksContainers, scaledUpProducers, partialObligations, ragRating)
                : 0;

            return householdTonnage + publicBinTonnage + glassTonnage;
        }

        public static decimal GetReportedTonnageTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations,
            RagRating? ragRating = null)
        {
            return producers.Sum(producer => GetReportedTonnage(producer, material, scaledUpProducers, partialObligations, ragRating));
        }

        public static decimal GetNetReportedTonnageCanBeNegative(
            ProducerDetail producer,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            var reportedTonnage = GetReportedTonnage(producer, material, scaledUpProducers, partialObligations);
            var toSubtract = GetTonnage(producer, material, PackagingTypes.ConsumerWaste, scaledUpProducers, partialObligations);
            var total = reportedTonnage - toSubtract;
            return total;
        }

        public static decimal? GetPreviousInvoicedTonnageOverallTotal(
          IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
          MaterialDetail material)
        {
            var levelOneRows = producerDisposalFees.Where(fee => fee.Level == CommonConstants.LevelOne.ToString());
            return levelOneRows.Sum(row => row.ProducerDisposalFeesByMaterial?[material.Code].PreviousInvoicedTonnage);
        }

        public static decimal GetPricePerTonne(
            MaterialDetail material,
            CalcResult calcResult)
        {
            var laDisposalCostDataDetail = calcResult.CalcResultLaDisposalCostData.CalcResultLaDisposalCostDetails.FirstOrDefault(la => la.Name == material.Name);

            if (laDisposalCostDataDetail == null)
            {
                return 0;
            }

            var isParseSuccessful = decimal.TryParse(laDisposalCostDataDetail.DisposalCostPricePerTonne, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-GB"), out decimal value);

            return isParseSuccessful ? value : 0;
        }

        public static decimal GetProducerDisposalFee(
            ProducerDetail producer,
            IEnumerable<ProducerDetail> producerAndSubsidiaries,
            MaterialDetail material,
            CalcResult calcResult,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            var totalReportedTonnage = producerAndSubsidiaries.Sum(producer => GetReportedTonnage(producer, material, scaledUpProducers, partialObligations));
            var totalManagedConsumerWasteTonnage = producerAndSubsidiaries.Sum(producer => GetTonnage(producer, material, PackagingTypes.ConsumerWaste, scaledUpProducers, partialObligations));

            if (totalManagedConsumerWasteTonnage > totalReportedTonnage)
            {
                return 0;
            }

            var netReportedTonnage = GetNetReportedTonnageCanBeNegative(producer, material, scaledUpProducers, partialObligations);
            var pricePerTonne = GetPricePerTonne(material, calcResult);

            return netReportedTonnage * pricePerTonne;
        }

        public static decimal GetProducerDisposalFeeProducerTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            var totalReportedTonnage = producers.Sum(producer => GetReportedTonnage(producer, material, scaledUpProducers, partialObligations));
            var totalManagedConsumerWasteTonnage = producers.Sum(producer => GetTonnage(producer, material, PackagingTypes.ConsumerWaste, scaledUpProducers, partialObligations));

            if (totalManagedConsumerWasteTonnage > totalReportedTonnage)
            {
                return 0;
            }

            var totalProducerDisposalFees = 0m;
            foreach(var producer in producers)
            {
                var netReportedTonnage = GetNetReportedTonnageCanBeNegative(producer, material, scaledUpProducers, partialObligations);
                var pricePerTonne = GetPricePerTonne(material, calcResult);

                totalProducerDisposalFees += netReportedTonnage * pricePerTonne;
            }

            return totalProducerDisposalFees;
        }

        public static decimal GetProducerDisposalFeeOverallTotal(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            MaterialDetail material)
        {
            var levelOneRows = producerDisposalFees.Where(fee => fee.Level == CommonConstants.LevelOne.ToString());
            return levelOneRows.Sum(row => row?.ProducerDisposalFeesByMaterial?[material.Code].ProducerDisposalFee) ?? 0;
        }

        public static decimal GetBadDebtProvisionOverallTotal(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            MaterialDetail material)
        {
            var levelOneRows = producerDisposalFees.Where(fee => fee.Level == CommonConstants.LevelOne.ToString());
            return levelOneRows.Sum(row => row?.ProducerDisposalFeesByMaterial?[material.Code].BadDebtProvision) ?? 0;
        }

        public static decimal GetBadDebtProvision(
            ProducerDetail producer,
            IEnumerable<ProducerDetail> producerAndSubsidiaries,
            MaterialDetail material,
            CalcResult calcResult,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            var producerDisposalFee = GetProducerDisposalFee(producer, producerAndSubsidiaries, material, calcResult, scaledUpProducers, partialObligations);

            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                return producerDisposalFee * value / 100;
            }

            return 0;
        }

        public static decimal GetBadDebtProvisionProducerTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            var totalProducerDisposalFees = GetProducerDisposalFeeProducerTotal(producers, material, calcResult, scaledUpProducers, partialObligations);

            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                return totalProducerDisposalFees * value / 100;
            }

            return 0;
        }

        public static decimal GetProducerDisposalFeeWithBadDebtProvision(
            ProducerDetail producer,
            IEnumerable<ProducerDetail> producerAndSubsidiaries,
            MaterialDetail material,
            CalcResult calcResult,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            var producerDisposalFee = GetProducerDisposalFee(producer, producerAndSubsidiaries, material, calcResult, scaledUpProducers, partialObligations);

            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                return producerDisposalFee * (1 + (value / 100));
            }

            return 0;
        }

        public static decimal GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            var totalProducerDisposalFees = GetProducerDisposalFeeProducerTotal(producers, material, calcResult, scaledUpProducers, partialObligations);

            var isParseSuccessful = decimal.TryParse(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Replace("%", string.Empty), out decimal value);

            if (isParseSuccessful)
            {
                return totalProducerDisposalFees * (1 + (value / 100));
            }

            return 0;
        }

        public static decimal GetProducerDisposalFeeWithBadDebtProvisionOverallTotal(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            MaterialDetail material)
        {
            var levelOneRows = producerDisposalFees.Where(fee => fee.Level == CommonConstants.LevelOne.ToString());
            return levelOneRows.Sum(row => row?.ProducerDisposalFeesByMaterial?[material.Code].ProducerDisposalFeeWithBadDebtProvision) ?? 0;
        }

        public static decimal GetCountryBadDebtProvision(
            ProducerDetail producer,
            IEnumerable<ProducerDetail> producerAndSubsidiaries,
            MaterialDetail material,
            CalcResult calcResult,
            Countries country,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, producerAndSubsidiaries, material, calcResult, scaledUpProducers, partialObligations);

            var countryApportionmentPercentage = GetCountryApportionmentPercentage(calcResult);
            if (countryApportionmentPercentage == null)
            {
                return 0;
            }

            string? disposalCost;
            switch (country)
            {
                case Countries.England:
                    disposalCost = countryApportionmentPercentage.EnglandDisposalCost;
                    break;
                case Countries.Wales:
                    disposalCost = countryApportionmentPercentage.WalesDisposalCost;
                    break;
                case Countries.Scotland:
                    disposalCost = countryApportionmentPercentage.ScotlandDisposalCost;
                    break;
                case Countries.NorthernIreland:
                    disposalCost = countryApportionmentPercentage.NorthernIrelandDisposalCost;
                    break;
                default:
                    disposalCost = "0%";
                    break;
            }

            var isParseSuccessful = decimal.TryParse(disposalCost.Replace("%", string.Empty), out decimal value);

            return isParseSuccessful ? producerDisposalFeeWithBadDebtProvision * value / 100 : 0;
        }

        public static decimal GetCountryBadDebtProvisionTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            CalcResult calcResult,
            Countries country,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            return producers.Sum(producer => GetCountryBadDebtProvision(producer, producers, material, calcResult, country, scaledUpProducers, partialObligations));
        }

        public static decimal GetCountryBadDebtProvisionOverallTotal(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            MaterialDetail material,
            Countries country)
        {
            var levelOneRows = producerDisposalFees.Where(fee => fee.Level == CommonConstants.LevelOne.ToString());

            switch (country)
            {
                case Countries.England:
                    return levelOneRows.Sum(row => row?.ProducerDisposalFeesByMaterial?[material.Code].EnglandWithBadDebtProvision) ?? 0;
                case Countries.Wales:
                    return levelOneRows.Sum(row => row?.ProducerDisposalFeesByMaterial?[material.Code].WalesWithBadDebtProvision) ?? 0;
                case Countries.Scotland:
                    return levelOneRows.Sum(row => row?.ProducerDisposalFeesByMaterial?[material.Code].ScotlandWithBadDebtProvision) ?? 0;
                case Countries.NorthernIreland:
                    return levelOneRows.Sum(row => row?.ProducerDisposalFeesByMaterial?[material.Code].NorthernIrelandWithBadDebtProvision) ?? 0;
                default:
                    return 0m;
            }
        }

        public static CalcResultLapcapDataDetail? GetCountryApportionmentPercentage(CalcResult calcResult)
        {
            return calcResult.CalcResultLapcapData.CalcResultLapcapDataDetail?.FirstOrDefault(la => la.Name == CalcResultSummaryHeaders.OneCountryApportionment);
        }

        public static decimal GetTotal1Plus2ABadDebt(
            IEnumerable<ProducerDetail> producers,
            IEnumerable<MaterialDetail> materials,
            CalcResult calcResult,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            decimal total = 0m;

            foreach (var material in materials)
            {
                var laDisposalTotal = GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(producers, material, calcResult, scaledUpProducers, partialObligations);
                var twoAcommsDisposal = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvisionTotal(producers, material, calcResult, scaledUpProducers, partialObligations);
                total += laDisposalTotal + twoAcommsDisposal;
            }

            return total;
        }

        public static void SetHeaders(CalcResultSummary result, IEnumerable<MaterialDetail> materials, bool showModulations)
        {
            result.ResultSummaryHeader = new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CalculationResult, ColumnIndex = 1 };
            result.NotesHeader         = new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.Notes, ColumnIndex = 1 };

            int section1MaterialsIdx           = 1                             + startingHeaders().Count();
            int section1DisposalFeeIdx         = section1MaterialsIdx          + section1Materials(materials, showModulations).Count();
            int section2aMaterialsIdx          = section1DisposalFeeIdx        + section1DisposalFee().Count();
            int section2aCommsIdx              = section2aMaterialsIdx         + section2aMaterials(materials, showModulations).Count();
            int section1DisposalIdx            = section2aCommsIdx             + section1Disposal().Count();
            int section2aComms2aIdx            = section1DisposalIdx           + section2aComms().Count();
            int commsCost2aPercentageIdx       = section2aComms2aIdx           + commsCost2aPercentage().Count();
            int commsCost2bIdx                 = commsCost2aPercentageIdx      + commsCost2b().Count();
            int commsCost2cIdx                 = commsCost2bIdx                + commsCost2c().Count();
            int onePlus2A2B2CProducerIdx       = commsCost2cIdx                + commsCost2c().Count();
            int threeSaCostsSummaryIdx         = onePlus2A2B2CProducerIdx      + OnePlus2A2B2CProducer.GetHeaders().Count();
            int laDataPrepCostsProducerIdx     = threeSaCostsSummaryIdx        + ThreeSaCostsProducer.GetHeaders().Count();
            int saSetupCostsSummaryIdx         = laDataPrepCostsProducerIdx    + LaDataPrepCostsProducer.GetHeaders().Count();
            int totalBillBreakdownProducerIdx  = saSetupCostsSummaryIdx        + SaSetupCostsProducer.GetHeaders().Count();
            int billingInstructionsProducerIdx = totalBillBreakdownProducerIdx + TotalBillBreakdownProducer.GetHeaders().Count();

            var resultSummaryHeaders = new List<CalcResultSummaryHeader>();
            resultSummaryHeaders.AddRange([
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
            resultSummaryHeaders.AddRange(OnePlus2A2B2CProducer.GetSummaryHeaders(onePlus2A2B2CProducerIdx));
            resultSummaryHeaders.AddRange(ThreeSaCostsProducer.GetSummaryHeaders(threeSaCostsSummaryIdx));
            resultSummaryHeaders.AddRange(LaDataPrepCostsProducer.GetSummaryHeaders(laDataPrepCostsProducerIdx));
            resultSummaryHeaders.AddRange(SaSetupCostsProducer.GetSummaryHeaders(saSetupCostsSummaryIdx));
            resultSummaryHeaders.AddRange(TotalBillBreakdownProducer.GetSummaryHeaders(totalBillBreakdownProducerIdx));
            resultSummaryHeaders.AddRange(BillingInstructionsProducer.GetSummaryHeaders(billingInstructionsProducerIdx));
            result.ProducerDisposalFeesHeaders = resultSummaryHeaders;

            var materialsBreakdownHeaders = new List<CalcResultSummaryHeader>();
            var columnIndex = section1MaterialsIdx;
            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultSummaryHeader { Name = $"{material.Name} Breakdown", ColumnIndex = columnIndex});
                columnIndex += section1Materials([material], showModulations).Count();
            }
            materialsBreakdownHeaders.Add(new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.DisposalFeeSummary, ColumnIndex = section1DisposalFeeIdx });

            var commsCostColumnIndex = section2aMaterialsIdx;
            foreach (var material in materials)
            {
                materialsBreakdownHeaders.Add(new CalcResultSummaryHeader { Name = $"{material.Name} Breakdown", ColumnIndex = commsCostColumnIndex });
                commsCostColumnIndex += section2aMaterials([material], showModulations).Count();
            }
            materialsBreakdownHeaders.Add(new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.CommsCostSummaryHeader, ColumnIndex = commsCostColumnIndex});
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(section1DisposalIdx, result.TotalFeeforLADisposalCostswoBadDebtprovision1, result.BadDebtProvisionFor1, result.TotalFeeforLADisposalCostswithBadDebtprovision1));
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(section2aComms2aIdx, result.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A, result.BadDebtProvisionFor2A, result.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A));
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(commsCost2bIdx, result.CommsCostHeaderWithoutBadDebtFor2bTitle, result.CommsCostHeaderBadDebtProvisionFor2bTitle, result.CommsCostHeaderWithBadDebtFor2bTitle));
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(commsCost2cIdx, result.TwoCCommsCostsByCountryWithoutBadDebtProvision, result.TwoCBadDebtProvision, result.TwoCCommsCostsByCountryWithBadDebtProvision));
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(onePlus2A2B2CProducerIdx, result.TotalOnePlus2A2B2CFeeWithBadDebtProvision));
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(threeSaCostsSummaryIdx, result.SaOperatingCostsWoTitleSection3, result.BadDebtProvisionTitleSection3, result.SaOperatingCostsWithTitleSection3));
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(laDataPrepCostsProducerIdx, result.LaDataPrepCostsTitleSection4, result.LaDataPrepCostsBadDebtProvisionTitleSection4, result.LaDataPrepCostsWithBadDebtProvisionTitleSection4));
            materialsBreakdownHeaders.AddRange(CreateMoneyHeaders(saSetupCostsSummaryIdx, result.SaSetupCostsTitleSection5, result.SaSetupCostsBadDebtProvisionTitleSection5, result.SaSetupCostsWithBadDebtProvisionTitleSection5));
            result.MaterialBreakdownHeaders = materialsBreakdownHeaders;

            var columnHeaders = new List<CalcResultSummaryHeader>();
            columnHeaders.AddRange(startingHeaders());
            columnHeaders.AddRange(section1Materials(materials, showModulations));
            columnHeaders.AddRange(section1DisposalFee());
            columnHeaders.AddRange(section2aMaterials(materials, showModulations));
            columnHeaders.AddRange(section2aComms());
            columnHeaders.AddRange(section1Disposal());
            columnHeaders.AddRange(section2aComms2a());
            columnHeaders.AddRange(commsCost2aPercentage());
            columnHeaders.AddRange(commsCost2b());
            columnHeaders.AddRange(commsCost2c());
            columnHeaders.AddRange(OnePlus2A2B2CProducer.GetHeaders());
            columnHeaders.AddRange(ThreeSaCostsProducer.GetHeaders());
            columnHeaders.AddRange(LaDataPrepCostsProducer.GetHeaders());
            columnHeaders.AddRange(SaSetupCostsProducer.GetHeaders());
            columnHeaders.AddRange(TotalBillBreakdownProducer.GetHeaders());
            columnHeaders.AddRange(BillingInstructionsProducer.GetHeaders());
            result.ColumnHeaders = columnHeaders;
        }

        private static IEnumerable<CalcResultSummaryHeader> CreateMoneyHeaders(int columnIndex, params decimal[] values)
        {
            return values.Select((value, i) => new CalcResultSummaryHeader { Name = $"£{Math.Round(value, decimalRoundUp)}", ColumnIndex = columnIndex + i});
        }

        private static IEnumerable<CalcResultSummaryHeader> CreateHeaders(params string[] names)
        {
            return names.Select((name, i) => new CalcResultSummaryHeader { Name = name});
        }

        private static IEnumerable<CalcResultSummaryHeader> startingHeaders()
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

        private static IEnumerable<CalcResultSummaryHeader> section1Materials(IEnumerable<MaterialDetail> materials, bool showModulations)
        {
            return materials.SelectMany(material =>
            {
                var headers = CreateHeaders(CalcResultSummaryHeaders.PreviousInvoicedTonnage)
                    .ToList();

                headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.HouseholdPackagingWasteTonnage));
                if (showModulations)
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
                if (showModulations)
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
                    if (showModulations)
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

                if (showModulations) {
                    headers.AddRange(CreateHeaders(
                        CalcResultSummaryHeaders.TotalTonnage,
                        CalcResultSummaryHeaders.RedPlusRedMedicalTotalTonnage,
                        CalcResultSummaryHeaders.AmberPlusAmberMedicalTotalTonnage,
                        CalcResultSummaryHeaders.GreenPlusGreenMedicalTotalTonnage,
                        CalcResultSummaryHeaders.SelfManagedConsumerWasteTonnage,
                        CalcResultSummaryHeaders.ActionedSelfManagedConsumerWasteTonnage,
                        CalcResultSummaryHeaders.NetTonnage,
                        CalcResultSummaryHeaders.RedPlusRedMedicalNetTonnage,
                        CalcResultSummaryHeaders.AmberPlusAmberMedicalNetTonnage,
                        CalcResultSummaryHeaders.GreenPlusGreenMedicalNetTonnage
                    ));
                } else {
                    headers.AddRange(CreateHeaders(
                        CalcResultSummaryHeaders.TotalTonnage,
                        CalcResultSummaryHeaders.SelfManagedConsumerWasteTonnage,
                        CalcResultSummaryHeaders.NetTonnage
                    ));
                }

                headers.AddRange(CreateHeaders(
                    CalcResultSummaryHeaders.TonnageChange,
                    CalcResultSummaryHeaders.PricePerTonne,
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

        private static IEnumerable<CalcResultSummaryHeader> section1DisposalFee()
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

        private static IEnumerable<CalcResultSummaryHeader> section2aMaterials(IEnumerable<MaterialDetail> materials, bool showModulations)
        {
            return materials.SelectMany(material =>
            {
                var headers = new List<CalcResultSummaryHeader>();

                headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.HouseholdPackagingWasteTonnage));
                if (showModulations)
                {
                    headers.AddRange(CreateHeaders(
                        CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageRed,
                        CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageAmber,
                        CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageGreen,
                        CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageRedMedical,
                        CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageAmberMedical,
                        CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageGreenMedical
                    ));
                }

                headers.AddRange(CreateHeaders(CalcResultSummaryHeaders.PublicBinTonnage));
                if (showModulations)
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
                    if (showModulations)
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

        private static IEnumerable<CalcResultSummaryHeader> section2aComms()
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

        private static IEnumerable<CalcResultSummaryHeader> section1Disposal()
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

        private static IEnumerable<CalcResultSummaryHeader> section2aComms2a()
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

        private static IEnumerable<CalcResultSummaryHeader> commsCost2aPercentage()
        {
            return CreateHeaders(CalcResultSummaryHeaders.PercentageofProducerTonnagevsAllProducers);
        }

        private static IEnumerable<CalcResultSummaryHeader> commsCost2b()
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

        private static IEnumerable<CalcResultSummaryHeader> commsCost2c()
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
            return calcResult.CalcResultCommsCostReportDetail.CommsCostByCountry.ToList()[1].TotalValue;
        }

        public static decimal GetCommsCostHeaderBadDebtProvisionFor2bTitle(
            CalcResult calcResult,
            CalcResultSummary calcResultSummary)
        {
            var commsCost = calcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle;
            var badDebtProvision = Convert.ToDecimal(calcResult.CalcResultParameterOtherCost.BadDebtProvision.Value.Trim('%')) / 100;
            return commsCost * badDebtProvision;
        }

        public static decimal GetCommsCostHeaderWithBadDebtFor2bTitle(CalcResultSummary calcResultSummary)
        {
            var commsCostHeaderWithoutBadDebt = calcResultSummary.CommsCostHeaderWithoutBadDebtFor2bTitle;
            var commsCostHeaderBadDebtProvision = calcResultSummary.CommsCostHeaderBadDebtProvisionFor2bTitle;
            return commsCostHeaderWithoutBadDebt + commsCostHeaderBadDebtProvision;
        }

        public static decimal GetCountryOnePlusFourApportionment(
            CalcResult calcResult,
            Countries country)
        {
            var onePlusFourApportionment = calcResult.CalcResultOnePlusFourApportionment
                .CalcResultOnePlusFourApportionmentDetails
                .Single(x => x.Name == CalcResultCommsCostBuilder.OnePlusFourApportionment);

            switch (country)
            {
                case Countries.England:
                    return onePlusFourApportionment.EnglandTotal;
                case Countries.Wales:
                    return onePlusFourApportionment.WalesTotal;
                case Countries.Scotland:
                    return onePlusFourApportionment.ScotlandTotal;
                case Countries.NorthernIreland:
                    return onePlusFourApportionment.NorthernIrelandTotal;
                default:
                    return 0;
            }
        }

        public static decimal GetParamsOtherFourCountryApportionmentPercentage(
            CalcResult calcResult,
            Countries country)
        {
            var fourCountryApportionment = calcResult.CalcResultParameterOtherCost.Details
                .SingleOrDefault(x => x.Name == CalcResultParameterOtherCostBuilder.FourCountryApportionmentPercentage);

            if (fourCountryApportionment == null)
            {
                return 0;
            }

            switch (country)
            {
                case Countries.England:
                    return fourCountryApportionment.EnglandValue;
                case Countries.Wales:
                    return fourCountryApportionment.WalesValue;
                case Countries.Scotland:
                    return fourCountryApportionment.ScotlandValue;
                case Countries.NorthernIreland:
                    return fourCountryApportionment.NorthernIrelandValue;
                default:
                    return 0;
            }
        }
    }
}