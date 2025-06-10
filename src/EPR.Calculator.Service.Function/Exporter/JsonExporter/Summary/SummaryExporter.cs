using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter.CalcResult
{
    /// <summary>
    /// Converts a <see cref="CalcResultSummary"/> to a JSON string representation.
    /// </summary>
    public class SummaryExporter
    {
        /// <inheritdoc/>
        public string ConvertToJson(CalcResultSummary data)
            => JsonSerializer.Serialize(
                new { producerCalculationResultsSummary = new CalcResultLapcapDataToSerialise(data) },
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Converters = { new Converter.CurrencyConverter() },
                    
                    // This is required in order to output the £ symbol as-is rather than encoding it.
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) 
                });

        /// <summary>
        /// Structure to arrange the CalcResultSummary data using the property
        /// names and ordering required for serialisation.
        /// </summary>
        private readonly record struct CalcResultLapcapDataToSerialise(
            decimal FeeForLaDisposalCostsWithoutBadDebtprovision1,
            decimal BadDebtProvision1,
            decimal FeeForLaDisposalCostsWithBadDebtprovision1,

            decimal FeeForCommsCostsByMaterialWithoutBadDebtprovision2a,
            decimal BadDebtProvision2a,
            decimal FeeForCommsCostsByMaterialWitBadDebtprovision2a,

            decimal FeeForCommsCostsUkWideWithoutBadDebtprovision2b,
            decimal BadDebtProvision2b,
            decimal FeeForCommsCostsUkWideWithBadDebtprovision2b,

            decimal FeeForCommsCostsByCountryWithoutBadDebtprovision2c,
            decimal BadDebtProvision2c,
            decimal FeeForCommsCostsByCountryWideWithBadDebtprovision2c,

            decimal Total12a2b2cWithBadDebt,

            decimal SaOperatingCostsWithoutBadDebtProvision3,
            decimal BadDebtProvision3,
            decimal SaOperatingCostsWithBadDebtProvision3,

            decimal LaDataPrepCostsWithoutBadDebtProvision4,
            decimal BadDebtProvision4,
            decimal LaDataPrepCostsWithbadDebtProvision4,

            decimal OneOffFeeSaSetuCostsWithbadDebtProvision5,
            decimal BadDebtProvision5,
            decimal OneOffFeeSaSetuCostsWithoutbadDebtProvision5)
        {
            public CalcResultLapcapDataToSerialise(CalcResultSummary data)
            : this(data.TotalFeeforLADisposalCostswoBadDebtprovision1,
                data.BadDebtProvisionFor1,
                data.TotalFeeforLADisposalCostswithBadDebtprovision1,

                data.TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A,
                data.BadDebtProvisionFor2A,
                data.TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A,

                data.CommsCostHeaderWithoutBadDebtFor2bTitle,
                data.CommsCostHeaderBadDebtProvisionFor2bTitle,
                data.CommsCostHeaderWithBadDebtFor2bTitle,

                data.TwoCCommsCostsByCountryWithoutBadDebtProvision,
                data.TwoCBadDebtProvision,
                data.TwoCCommsCostsByCountryWithBadDebtProvision,

                data.TotalOnePlus2A2B2CFeeWithBadDebtProvision,

                data.SaOperatingCostsWoTitleSection3,
                data.BadDebtProvisionTitleSection3,
                data.SaOperatingCostsWithTitleSection3,

                data.LaDataPrepCostsTitleSection4,
                data.LaDataPrepCostsBadDebtProvisionTitleSection4,
                data.LaDataPrepCostsWithBadDebtProvisionTitleSection4,

                data.SaSetupCostsTitleSection5,
                data.SaSetupCostsBadDebtProvisionTitleSection5,
                data.SaSetupCostsWithBadDebtProvisionTitleSection5)
            {
            }


        }
    }
}
