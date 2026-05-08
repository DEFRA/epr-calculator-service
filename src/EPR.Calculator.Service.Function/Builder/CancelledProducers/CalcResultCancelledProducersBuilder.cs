using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Billing.Contexts;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Builder.CancelledProducers;

public interface ICalcResultCancelledProducersBuilder
{
    Task<CalcResultCancelledProducersResponse> ConstructAsync(RunContext runContext);
}

public class CalcResultCancelledProducersBuilder(
    IInvoicedProducerService invoicedProducerService,
    IMaterialService materialService)
    : ICalcResultCancelledProducersBuilder
{
    public async Task<CalcResultCancelledProducersResponse> ConstructAsync(RunContext runContext)
    {
        return new CalcResultCancelledProducersResponse
        {
            TitleHeader = CommonConstants.CancelledProducers,
            CancelledProducers = await GetCancelledProducers(runContext)
        };
    }

    private async Task<ImmutableList<CalcResultCancelledProducersDto>> GetCancelledProducers(RunContext runContext)
    {
        var lookup = await GetMissingAcceptedCancelledInvoicedProducerRecordsLookup(runContext);
        var materialsByCode = await materialService.GetMaterialsByCode();

        var builder = ImmutableList.CreateBuilder<CalcResultCancelledProducersDto>();

        foreach (var (producerId, recordsByMaterialId) in lookup)
        {
            var latestRecord = recordsByMaterialId.Values.OrderByDescending(r => r.CalculatorRunId).First();

            builder.Add(new CalcResultCancelledProducersDto
            {
                ProducerId = producerId,
                ProducerOrSubsidiaryNameValue = latestRecord.ProducerName,
                TradingNameValue = latestRecord.TradingName,

                LastTonnage = new LastTonnage
                {
                    AluminiumValue = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.Aluminium].Id)?.InvoicedNetTonnage,
                    FibreCompositeValue = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.FibreComposite].Id)?.InvoicedNetTonnage,
                    GlassValue = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.Glass].Id)?.InvoicedNetTonnage,
                    PaperOrCardValue = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.PaperOrCard].Id)?.InvoicedNetTonnage,
                    PlasticValue = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.Plastic].Id)?.InvoicedNetTonnage,
                    WoodValue = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.Wood].Id)?.InvoicedNetTonnage,
                    SteelValue = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.Steel].Id)?.InvoicedNetTonnage,
                    OtherMaterialsValue = recordsByMaterialId.GetValueOrDefault(materialsByCode[MaterialCodes.OtherMaterials].Id)?.InvoicedNetTonnage
                },

                LatestInvoice = new LatestInvoice
                {
                    BillingInstructionIdValue = latestRecord.BillingInstructionId,
                    RunNumberValue = latestRecord.CalculatorRunId.ToString(),
                    RunNameValue = latestRecord.CalculatorName,
                    CurrentYearInvoicedTotalToDateValue = latestRecord.CurrentYearInvoicedTotalAfterThisRun
                }
            });
        }

        return builder.ToImmutable();
    }

    private async Task<ImmutableDictionary<int, ImmutableDictionary<int, InvoicedProducerRecord>>> GetMissingAcceptedCancelledInvoicedProducerRecordsLookup(RunContext runContext)
    {
        var producerIdsForRun = await invoicedProducerService.GetProducerIdsForRun(runContext.RunId);
        var invoicedProducerIdsForYear = await invoicedProducerService.GetInvoicedProducerIdsForYear(runContext.RelativeYear);
        var missingProducerIds = invoicedProducerIdsForYear.Except(producerIdsForRun);

        ImmutableHashSet<int> missingAcceptedCancelledProducerIds;

        if (runContext is BillingRunContext)
        {
            var acceptedCancelledProducers = await invoicedProducerService.GetAcceptedCancelledProducerIdsForRun(runContext.RunId);
            missingAcceptedCancelledProducerIds = acceptedCancelledProducers.Intersect(missingProducerIds);
        }
        else
        {
            var acceptedCancelledProducers = await invoicedProducerService.GetInvoicedThenCancelledProducerIdsForYear(runContext.RelativeYear);
            missingAcceptedCancelledProducerIds = missingProducerIds.Except(acceptedCancelledProducers);
        }

        var missingAcceptedCancelledInvoicedProducerRecords = await invoicedProducerService.GetInvoicedProducerRecords(runContext.RelativeYear, missingAcceptedCancelledProducerIds);

        // The grouping here selects the latest invoice for each producer/material combination
        return missingAcceptedCancelledInvoicedProducerRecords
            .GroupBy(r => new { r.ProducerId, r.MaterialId })
            .Select(group => group.OrderByDescending(t => t.CalculatorRunId).First())
            .GroupBy(r => r.ProducerId)
            .ToImmutableDictionary(g => g.Key, g => g.ToImmutableDictionary(r => r.MaterialId));
    }
}
