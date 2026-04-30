using System.Collections.Immutable;
using System.Diagnostics;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IProducerInvoiceNetTonnageService
    {
        public Task CreateProducerInvoiceNetTonnage(RunContext runContext, CalcResult calcResult);
    }

    public class ProducerInvoiceNetTonnageService(
        ApplicationDBContext dbContext,
        IBulkOperations bulkOps,
        IMaterialService materialService,
        ILogger<ProducerInvoiceNetTonnageService> logger)
        : IProducerInvoiceNetTonnageService
    {
        public async Task CreateProducerInvoiceNetTonnage(RunContext runContext, CalcResult calcResult)
        {
            var stopwatch = Stopwatch.StartNew();
            var producers = calcResult.CalcResultSummary.ProducerDisposalFees.Where(producer => producer.Level == CommonConstants.LevelOne.ToString());
            var materials = await materialService.GetMaterials();

            var producerInvoiceNetTonnage = producers
                .SelectMany(producer => materials.Select(material =>
                {
                    var disposalFees = producer.ProducerDisposalFeesByMaterial;

                    if (disposalFees is not null && disposalFees.TryGetValue(material.Code, out var feeSummary))
                    {
                        return new ProducerInvoicedMaterialNetTonnage
                        {
                            CalculatorRunId = runContext.RunId,
                            ProducerId = producer.ProducerIdInt,
                            InvoicedNetTonnage = feeSummary.NetReportedTonnage.total,
                            MaterialId = material.Id
                        };
                    }

                    return new ProducerInvoicedMaterialNetTonnage();
                }))
                .ToImmutableArray();

            if (producerInvoiceNetTonnage.Length == 0)
            {
                throw new RunProcessingException(runContext, "No producer invoice net tonnages generated.");
            }

            await bulkOps.BulkInsertAsync(dbContext, producerInvoiceNetTonnage);

            stopwatch.Stop();
            logger.LogInformation("Inserted {RecordCount} producer invoice net tonnage records in {Elapsed}",
                producerInvoiceNetTonnage.Length, stopwatch.Elapsed);
        }
    }
}
