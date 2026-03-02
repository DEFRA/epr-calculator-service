namespace EPR.Calculator.Service.Function.Builder.Summary.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Data.Enums;
    using EPR.Calculator.Service.Function.Builder.CommsCost;
    using EPR.Calculator.Service.Function.Builder.ParametersOther;
    using EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions;
    using EPR.Calculator.Service.Function.Builder.Summary.CommsCostTwoA;
    using EPR.Calculator.Service.Function.Builder.Summary.LaDataPrepCosts;
    using EPR.Calculator.Service.Function.Builder.Summary.OnePlus2A2B2C;
    using EPR.Calculator.Service.Function.Builder.Summary.SaSetupCosts;
    using EPR.Calculator.Service.Function.Builder.Summary.ThreeSA;
    using EPR.Calculator.Service.Function.Builder.Summary.TotalBillBreakdown;
    using EPR.Calculator.Service.Function.Builder.Summary.TwoCCommsCost;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Models;

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

            if (scaledupProducerForAllSubmissionPeriods!.Any())
            {
                decimal tonnage = 0;
                foreach (var scaledupProducerTonnageByMaterial in scaledupProducerForAllSubmissionPeriods!.Select(x => x.ScaledupProducerTonnageByMaterial))
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
                        return maybePartialObligation!.PartialObligationTonnageByMaterial.GetValueOrDefault(material.Code)?.PartialReportedHouseholdPackagingWasteTonnage ?? 0;
                    case PackagingTypes.PublicBin:
                        return maybePartialObligation!.PartialObligationTonnageByMaterial.GetValueOrDefault(material.Code)?.PartialReportedPublicBinTonnage ?? 0;
                    case PackagingTypes.ConsumerWaste:
                        return maybePartialObligation!.PartialObligationTonnageByMaterial.GetValueOrDefault(material.Code)?.PartialReportedSelfManagedConsumerWasteTonnage ?? 0;
                    case PackagingTypes.HouseholdDrinksContainers:
                        return maybePartialObligation!.PartialObligationTonnageByMaterial.GetValueOrDefault(material.Code)?.PartialHouseholdDrinksContainersTonnageGlass ?? 0;
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
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {

            var maybePartialScaledUpTonnage = GetPartialTonnage(producer, material, packagingType, partialObligations);

            if (maybePartialScaledUpTonnage != null) {
                return (decimal)maybePartialScaledUpTonnage!;
            }

            var maybeScaledUpTonnage = GetScaledUpTonnage(producer, material, packagingType, scaledUpProducers);

            if (maybeScaledUpTonnage != null) {
                return (decimal)maybeScaledUpTonnage!;
            }

            var reportedMaterials = producer.ProducerReportedMaterials
                .FirstOrDefault(p => p.Material?.Code == material.Code && p.PackagingType == packagingType);

            return reportedMaterials?.PackagingTonnage ?? 0;
        }

        public static decimal GetTonnageTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            string packagingType,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            return producers.Sum(producer => GetTonnage(producer, material, packagingType, scaledUpProducers, partialObligations));
        }

        public static decimal GetReportedTonnage(
            ProducerDetail producer,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            var householdPackagingWasteTonnage = GetTonnage(producer, material, PackagingTypes.Household, scaledUpProducers, partialObligations);
            var publicBinTonnage = GetTonnage(producer, material, PackagingTypes.PublicBin, scaledUpProducers, partialObligations);

            if (material.Code != MaterialCodes.Glass)
            {
                return householdPackagingWasteTonnage + publicBinTonnage;
            }

            var householdDrinksContainersTonnage = GetTonnage(producer, material, PackagingTypes.HouseholdDrinksContainers, scaledUpProducers, partialObligations);

            return householdPackagingWasteTonnage + publicBinTonnage + householdDrinksContainersTonnage;
        }

        public static decimal GetReportedTonnageTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            return producers.Sum(producer => GetReportedTonnage(producer, material, scaledUpProducers, partialObligations));
        }

        private static decimal GetRagValue(ProducerReportedMaterial? prm, RagRating ragRating) =>
            ragRating switch
            {
                RagRating.Red => prm?.RedRamRagRating ?? 0m,
                RagRating.Amber => prm?.AmberRamRagRating ?? 0m,
                RagRating.Green => prm?.GreenRamRagRating ?? 0m,
                RagRating.RedMedical => prm?.RedMedicalRamRagRating ?? 0m,
                RagRating.AmberMedical => prm?.AmberMedicalRamRagRating ?? 0m,
                RagRating.GreenMedical => prm?.GreenMedicalRamRagRating ?? 0m,
                _ => 0m
            };

        public static decimal GetRagReportedTonnage(
            ProducerDetail producer,
            MaterialDetail material,
            RagRating ragRating,
            String packagingType)
        {
            var prm = producer.ProducerReportedMaterials.FirstOrDefault(p => p.Material?.Code == material.Code && p.PackagingType == packagingType);
            return GetRagValue(prm, ragRating);
        }

        public static decimal GetRagReportedTonnageTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            RagRating ragRating,
            String packagingType)
        {
            return producers.Sum(producer => GetRagReportedTonnage(producer, material, ragRating, packagingType));
        }

        public static decimal GetNetReportedTonnageWithoutNegativeTonnages(
            ProducerDetail producer,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            var reportedTonnage = GetReportedTonnage(producer, material, scaledUpProducers, partialObligations);
            var managedConsumerWasteTonnage = GetTonnage(producer, material, PackagingTypes.ConsumerWaste, scaledUpProducers, partialObligations);

            return reportedTonnage - managedConsumerWasteTonnage;
        }

        public static decimal GetNetReportedTonnage(
            ProducerDetail producer,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations,
            int level = CommonConstants.LevelOne)
        {
            var reportedTonnage = GetReportedTonnage(producer, material, scaledUpProducers, partialObligations);
            var managedConsumerWasteTonnage = GetTonnage(producer, material, PackagingTypes.ConsumerWaste, scaledUpProducers, partialObligations);

            if (level == CommonConstants.LevelTwo)
            {
                return reportedTonnage - managedConsumerWasteTonnage;
            }

            return managedConsumerWasteTonnage > reportedTonnage
                ? 0
                : reportedTonnage - managedConsumerWasteTonnage;
        }

        public static decimal GetNetReportedTonnageTotal(
            IEnumerable<ProducerDetail> producers,
            MaterialDetail material,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            IEnumerable<CalcResultPartialObligation> partialObligations)
        {
            var totalReportedTonnage = producers.Sum(producer => GetReportedTonnage(producer, material, scaledUpProducers, partialObligations));
            var totalManagedConsumerWasteTonnage = producers.Sum(producer => GetTonnage(producer, material, PackagingTypes.ConsumerWaste, scaledUpProducers, partialObligations));

            return totalManagedConsumerWasteTonnage > totalReportedTonnage
                ? 0
                : totalReportedTonnage - totalManagedConsumerWasteTonnage;
        }

        public static decimal GetNetReportedTonnageOverallTotal(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            MaterialDetail material)
        {
            var levelOneRows = producerDisposalFees.Where(fee => fee.Level == CommonConstants.LevelOne.ToString());
            return levelOneRows.Sum(row => row?.ProducerDisposalFeesByMaterial?[material.Code].NetReportedTonnage) ?? 0;
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

            var netReportedTonnage = GetNetReportedTonnageWithoutNegativeTonnages(producer, material, scaledUpProducers, partialObligations);

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
                var netReportedTonnage = GetNetReportedTonnageWithoutNegativeTonnages(producer, material, scaledUpProducers, partialObligations);

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

        public static CalcResultLapcapDataDetails? GetCountryApportionmentPercentage(CalcResult calcResult)
        {
            return calcResult.CalcResultLapcapData.CalcResultLapcapDataDetails?.FirstOrDefault(la => la.Name == CalcResultSummaryHeaders.OneCountryApportionment);
        }

        public static decimal GetTotalProducerDisposalFee(Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            return materialCostSummary.Values.Sum(m => m.ProducerDisposalFee);
        }

        public static decimal GetTotalBadDebtProvision(Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            return materialCostSummary.Values.Sum(m => m.BadDebtProvision);
        }

        public static decimal GetTotalProducerDisposalFeeWithBadDebtProvision(Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            return materialCostSummary.Values.Sum(m => m.ProducerDisposalFeeWithBadDebtProvision);
        }

        public static decimal GetEnglandTotal(Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            return materialCostSummary.Values.Sum(m => m.EnglandWithBadDebtProvision);
        }

        public static decimal GetWalesTotal(Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            return materialCostSummary.Values.Sum(m => m.WalesWithBadDebtProvision);
        }

        public static decimal GetScotlandTotal(Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            return materialCostSummary.Values.Sum(m => m.ScotlandWithBadDebtProvision);
        }

        public static decimal GetNorthernIrelandTotal(Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> materialCostSummary)
        {
            return materialCostSummary.Values.Sum(m => m.NorthernIrelandWithBadDebtProvision);
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

            result.MaterialBreakdownHeaders = GetMaterialsBreakdownHeader(
                section1MaterialsIdx: section1MaterialsIdx,
                section1DisposalFeeIdx: section1DisposalFeeIdx,
                section2aMaterialsIdx: section2aMaterialsIdx,
                section1DisposalIdx: section1DisposalIdx,
                section2aComms2aIdx: section2aComms2aIdx,
                commsCost2bIdx: commsCost2bIdx,
                commsCost2cIdx: commsCost2cIdx,
                onePlus2A2B2CProducerIdx: onePlus2A2B2CProducerIdx,
                threeSaCostsSummaryIdx: threeSaCostsSummaryIdx,
                laDataPrepCostsProducerIdx: laDataPrepCostsProducerIdx,
                saSetupCostsSummaryIdx: saSetupCostsSummaryIdx,
                totalBillBreakdownProducerIdx: totalBillBreakdownProducerIdx,
                billingInstructionsProducerIdx: billingInstructionsProducerIdx,
                result,
                materials,
                showModulations
            );

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Index section headers are needed - to be dynamic")]
        private static List<CalcResultSummaryHeader> GetMaterialsBreakdownHeader(
            int section1MaterialsIdx,
            int section1DisposalFeeIdx,
            int section2aMaterialsIdx,
            int section1DisposalIdx,
            int section2aComms2aIdx,
            int commsCost2bIdx,
            int commsCost2cIdx,
            int onePlus2A2B2CProducerIdx,
            int threeSaCostsSummaryIdx,
            int laDataPrepCostsProducerIdx,
            int saSetupCostsSummaryIdx,
            int totalBillBreakdownProducerIdx,
            int billingInstructionsProducerIdx,
            CalcResultSummary result,
            IEnumerable<MaterialDetail> materials,
            bool showModulations)
        {
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

            // Section-(1) & (2a)
            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforLADisposalCostswoBadDebtprovision1, decimalRoundUp)}", ColumnIndex = section1DisposalIdx },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.BadDebtProvisionFor1, decimalRoundUp)}", ColumnIndex = section1DisposalIdx + 1 },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforLADisposalCostswithBadDebtprovision1, decimalRoundUp)}", ColumnIndex = section1DisposalIdx + 2}
            ]);

            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A, decimalRoundUp)}", ColumnIndex = section2aComms2aIdx },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.BadDebtProvisionFor2A, decimalRoundUp)}", ColumnIndex = section2aComms2aIdx + 1 },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A, decimalRoundUp)}", ColumnIndex = section2aComms2aIdx + 2 }
            ]);

            // 2b comms total bill
            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.CommsCostHeaderWithoutBadDebtFor2bTitle, decimalRoundUp)}", ColumnIndex = commsCost2bIdx },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.CommsCostHeaderBadDebtProvisionFor2bTitle,decimalRoundUp)}", ColumnIndex = commsCost2bIdx + 1 },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.CommsCostHeaderWithBadDebtFor2bTitle, decimalRoundUp)}", ColumnIndex = commsCost2bIdx + 2 },
             ]);

            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TwoCCommsCostsByCountryWithoutBadDebtProvision, CalcResultSummaryUtil.decimalRoundUp)}", ColumnIndex = commsCost2cIdx },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TwoCBadDebtProvision, CalcResultSummaryUtil.decimalRoundUp)}", ColumnIndex = commsCost2cIdx + 1} ,
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TwoCCommsCostsByCountryWithBadDebtProvision, CalcResultSummaryUtil.decimalRoundUp)}", ColumnIndex = commsCost2cIdx + 2}
            ]);

            // Section Total bill (1 + 2a + 2b + 2c)
            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.TotalOnePlus2A2B2CFeeWithBadDebtProvision, decimalRoundUp)}", ColumnIndex = onePlus2A2B2CProducerIdx },
            ]);

            // Section-3 -first header
            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.SaOperatingCostsWoTitleSection3, decimalRoundUp)}", ColumnIndex = threeSaCostsSummaryIdx },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.BadDebtProvisionTitleSection3, decimalRoundUp)}", ColumnIndex = threeSaCostsSummaryIdx + 1 },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.SaOperatingCostsWithTitleSection3, decimalRoundUp)}", ColumnIndex = threeSaCostsSummaryIdx + 2 }
             ]);

            // LA data prep costs section 4
            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.LaDataPrepCostsTitleSection4, decimalRoundUp)}", ColumnIndex = laDataPrepCostsProducerIdx },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.LaDataPrepCostsBadDebtProvisionTitleSection4, decimalRoundUp)}", ColumnIndex = laDataPrepCostsProducerIdx + 1 },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.LaDataPrepCostsWithBadDebtProvisionTitleSection4, decimalRoundUp)}", ColumnIndex = laDataPrepCostsProducerIdx + 2 }
            ]);

            // Scheme administrator setup costs section 5
            materialsBreakdownHeaders.AddRange([
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.SaSetupCostsTitleSection5, decimalRoundUp)}", ColumnIndex = saSetupCostsSummaryIdx },
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.SaSetupCostsBadDebtProvisionTitleSection5, decimalRoundUp)}", ColumnIndex = saSetupCostsSummaryIdx + 1},
                new CalcResultSummaryHeader { Name = $"£{Math.Round(result.SaSetupCostsWithBadDebtProvisionTitleSection5, decimalRoundUp)}", ColumnIndex = saSetupCostsSummaryIdx + 2 }
            ]);

            return materialsBreakdownHeaders;
        }

        private static IEnumerable<CalcResultSummaryHeader> startingHeaders()
        {
            return [
                new () { Name = CalcResultSummaryHeaders.ProducerId },
                new () { Name = CalcResultSummaryHeaders.SubsidiaryId },
                new () { Name = CalcResultSummaryHeaders.ProducerOrSubsidiaryName },
                new () { Name = CalcResultSummaryHeaders.TradingName },
                new () { Name = CalcResultSummaryHeaders.Level },
                new () { Name = CalcResultSummaryHeaders.ScaledupTonnages },
                new () { Name = CalcResultSummaryHeaders.PartialCalculation },
                new () { Name = CalcResultSummaryHeaders.StatusCode },
                new () { Name = CalcResultSummaryHeaders.JoinersDate },
                new () { Name = CalcResultSummaryHeaders.LeaversDate }
            ];
        }

        private static IEnumerable<CalcResultSummaryHeader> section1Materials(
            IEnumerable<MaterialDetail> materials, bool showModulations)
        {
            return materials.SelectMany(material =>
            {
                var columnHeadersList = new List<CalcResultSummaryHeader>
                {
                    new() { Name = CalcResultSummaryHeaders.PreviousInvoicedTonnage },
                };

                columnHeadersList.Add(new() { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnage });
                if (showModulations)
                {
                    columnHeadersList.AddRange([
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageRed },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageAmber },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageGreen },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageRedMedical },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageAmberMedical },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageGreenMedical }
                    ]);
                }

                columnHeadersList.Add(new() { Name = CalcResultSummaryHeaders.PublicBinTonnage });
                if (showModulations)
                {
                    columnHeadersList.AddRange([
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PublicBinTonnageRed },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PublicBinTonnageAmber },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PublicBinTonnageGreen },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PublicBinTonnageRedMedical },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PublicBinTonnageAmberMedical },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PublicBinTonnageGreenMedical }
                    ]);
                }

                if (material.Code == MaterialCodes.Glass)
                {
                    columnHeadersList.Add(new() { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnage });
                    if (showModulations)
                    {
                        columnHeadersList.AddRange([
                            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageRed },
                            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageAmber },
                            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageGreen },
                            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageRedMedical },
                            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageAmberMedical },
                            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageGreenMedical }
                        ]);
                    }
                }

                columnHeadersList.AddRange([
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.SelfManagedConsumerWasteTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NetTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TonnageChange },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PricePerTonne },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerDisposalFee },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerDisposalFeeWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision },
                ]);

                return columnHeadersList;
            });
        }

        private static IEnumerable<CalcResultSummaryHeader> section1DisposalFee()
        {
            return [
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerDisposalFee },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerDisposalFeeWithBadDebtProvision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotal },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotal },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotal },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotal },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TonnageChangeCount },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TonnageChangeAdvice }
            ];
        }

        private static IEnumerable<CalcResultSummaryHeader> section2aMaterials(IEnumerable<MaterialDetail> materials, bool showModulations)
        {
            return materials.SelectMany(material =>
            {
                var columnHeadersList = new List<CalcResultSummaryHeader>{};

                columnHeadersList.Add(new() { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnage });
                if (showModulations)
                {
                    columnHeadersList.AddRange([
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageRed },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageAmber },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageGreen },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageRedMedical },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageAmberMedical },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdPackagingWasteTonnageGreenMedical }
                    ]);
                }

                columnHeadersList.Add(new() { Name = CalcResultSummaryHeaders.PublicBinTonnage });
                if (showModulations)
                {
                    columnHeadersList.AddRange([
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PublicBinTonnageRed },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PublicBinTonnageAmber },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PublicBinTonnageGreen },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PublicBinTonnageRedMedical },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PublicBinTonnageAmberMedical },
                        new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PublicBinTonnageGreenMedical }
                    ]);
                }

                if (material.Code == MaterialCodes.Glass)
                {
                    columnHeadersList.Add(new() { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnage });
                    if (showModulations)
                    {
                        columnHeadersList.AddRange([
                            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageRed },
                            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageAmber },
                            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageGreen },
                            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageRedMedical },
                            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageAmberMedical },
                            new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.HouseholdDrinksContainersTonnageGreenMedical }
                        ]);
                    }
                }

                columnHeadersList.AddRange([
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalTonnage },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PricePerTonne },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerTotalCostWithoutBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerTotalCostwithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandWithBadDebtProvision },
                    new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision }
                ]);

                return columnHeadersList;
            });
        }

        private static IEnumerable<CalcResultSummaryHeader> section2aComms()
        {
            return [
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalBadDebtProvision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision }
            ];
        }

        private static IEnumerable<CalcResultSummaryHeader> section1Disposal()
        {
            return [
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforLADisposalCostswoBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionFor1 },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforLADisposalCostswithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision }
            ];
        }

        private static IEnumerable<CalcResultSummaryHeader> section2aComms2a()
        {
            return [
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwoBadDebtprovision2A },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionfor2A },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.TotalProducerFeeforCommsCostsbyMaterialwithBadDebtprovision2A },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalwithBadDebtprovision },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalwithBadDebtprovision }
            ];
        }

        private static IEnumerable<CalcResultSummaryHeader> commsCost2aPercentage()
        {
            return [
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.PercentageofProducerTonnagevsAllProducers },
            ];
        }

        private static IEnumerable<CalcResultSummaryHeader> commsCost2b()
        {
            return [
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerFeeWithoutBadDebtForComms2b },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.BadDebtProvisionForComms2b },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ProducerFeeForCommsCostsWithBadDebtForComms2b },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.EnglandTotalWithBadDebtProvisionForComms2b },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.WalesTotalWithBadDebtProvisionForComms2b },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.ScotlandTotalWithBadDebtProvisionForComms2b },
                new CalcResultSummaryHeader { Name = CalcResultSummaryHeaders.NorthernIrelandTotalWithBadDebtProvisionForComms2b }
            ];
        }

        private static IEnumerable<CalcResultSummaryHeader> commsCost2c()
        {
            return [
                new CalcResultSummaryHeader { Name = TwoCCommsCostSubColumnHeader.TwoCCommsCostCountryInPropertionWithoutBadDebt },
                new CalcResultSummaryHeader { Name = TwoCCommsCostSubColumnHeader.TwoCCommsCostBadDebtProvision },
                new CalcResultSummaryHeader { Name = TwoCCommsCostSubColumnHeader.TwoCCommsCostCountryInPropertionWithBadDebt },
                new CalcResultSummaryHeader { Name = TwoCCommsCostSubColumnHeader.TwoCCommsCostEnglandWithBadDebt },
                new CalcResultSummaryHeader { Name = TwoCCommsCostSubColumnHeader.TwoCCommsCostWalesWithBadDebt },
                new CalcResultSummaryHeader { Name = TwoCCommsCostSubColumnHeader.TwoCCommsCostScotlandWithBadDebt },
                new CalcResultSummaryHeader { Name = TwoCCommsCostSubColumnHeader.TwoCCommsCostNIWithBadDebt }
            ];
        }

        public static decimal GetTotalProducerCommsFee(Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal producerTotalCostWithoutBadDebtProvision = 0;

            foreach (var material in commsCostSummary)
            {
                producerTotalCostWithoutBadDebtProvision += material.Value.ProducerTotalCostWithoutBadDebtProvision;
            }

            return producerTotalCostWithoutBadDebtProvision;
        }

        public static decimal GetCommsTotalBadDebtProvision(Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> materialCostSummary)
        {
            decimal totalBadDebtProvision = 0;

            foreach (var material in materialCostSummary)
            {
                totalBadDebtProvision += material.Value.BadDebtProvision;
            }

            return totalBadDebtProvision;
        }

        public static decimal GetTotalProducerCommsFeeWithBadDebtProvision(Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal totalCommsCostsbyMaterialwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                totalCommsCostsbyMaterialwithBadDebtprovision += material.Value.ProducerTotalCostwithBadDebtProvision;
            }

            return totalCommsCostsbyMaterialwithBadDebtprovision;
        }

        public static decimal GetNorthernIrelandCommsTotal(Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal northernIrelandTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                northernIrelandTotalwithBadDebtprovision += material.Value.NorthernIrelandWithBadDebtProvision;
            }

            return northernIrelandTotalwithBadDebtprovision;
        }

        public static decimal GetScotlandCommsTotal(Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal scotlandTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                scotlandTotalwithBadDebtprovision += material.Value.ScotlandWithBadDebtProvision;
            }

            return scotlandTotalwithBadDebtprovision;
        }

        public static decimal GetWalesCommsTotal(Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal walesTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                walesTotalwithBadDebtprovision += material.Value.WalesWithBadDebtProvision;
            }

            return walesTotalwithBadDebtprovision;
        }

        public static decimal GetEnglandCommsTotal(Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> commsCostSummary)
        {
            decimal englandTotalwithBadDebtprovision = 0;

            foreach (var material in commsCostSummary)
            {
                englandTotalwithBadDebtprovision += material.Value.EnglandWithBadDebtProvision;
            }

            return englandTotalwithBadDebtprovision;
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
