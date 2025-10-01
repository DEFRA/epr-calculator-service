﻿using System.Collections.Generic;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary.Common
{
    public static class MaterialCostsUtil
    {
        public static decimal GetNetReportedTonnage(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            MaterialDetail material,
            bool isOverAllTotalRow)
        {
            return isOverAllTotalRow
                ? CalcResultSummaryUtil.GetNetReportedTonnageOverallTotal(producerDisposalFees, material)
                : CalcResultSummaryUtil.GetNetReportedTonnageTotal(producersAndSubsidiaries, material, scaledUpProducers);
        }

        public static decimal? GetPreviousInvoicedTonnage(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            MaterialDetail material,
            bool isOverAllTotalRow,
            decimal? previousInvoicedNetTonnage)
        {
            return isOverAllTotalRow
                ? CalcResultSummaryUtil.GetPreviousInvoicedTonnageOverallTotal(producerDisposalFees, material)
                : previousInvoicedNetTonnage;
        }

        public static decimal GetProducerDisposalFee(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            MaterialDetail material,
            CalcResult calcResult,
            bool isOverAllTotalRow)
        {
            return isOverAllTotalRow
                ? CalcResultSummaryUtil.GetProducerDisposalFeeOverallTotal(producerDisposalFees, material)
                : CalcResultSummaryUtil.GetProducerDisposalFeeProducerTotal(producersAndSubsidiaries, material, calcResult, scaledUpProducers);
        }

        public static decimal GetBadDebtProvision(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            MaterialDetail material,
            CalcResult calcResult,
            bool isOverAllTotalRow)
        {
            return isOverAllTotalRow
                ? CalcResultSummaryUtil.GetBadDebtProvisionOverallTotal(producerDisposalFees, material)
                : CalcResultSummaryUtil.GetBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult, scaledUpProducers);
        }

        public static decimal GetProducerDisposalFeeWithBadDebtProvision(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            MaterialDetail material,
            CalcResult calcResult,
            bool isOverAllTotalRow)
        {
            return isOverAllTotalRow
                ? CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvisionOverallTotal(producerDisposalFees, material)
                : CalcResultSummaryUtil.GetProducerDisposalFeeWithBadDebtProvisionProducerTotal(producersAndSubsidiaries, material, calcResult, scaledUpProducers);
        }

        public static decimal GetCountryDisposalFeeWithBadDebtProvision(
            IEnumerable<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            IEnumerable<ProducerDetail> producersAndSubsidiaries,
            IEnumerable<CalcResultScaledupProducer> scaledUpProducers,
            MaterialDetail material,
            CalcResult calcResult,
            Countries country,
            bool isOverAllTotalRow)
        {
            return isOverAllTotalRow
                ? CalcResultSummaryUtil.GetCountryBadDebtProvisionOverallTotal(producerDisposalFees, material, country)
                : CalcResultSummaryUtil.GetCountryBadDebtProvisionTotal(producersAndSubsidiaries, material, calcResult, country, scaledUpProducers);
        }
    }
}
