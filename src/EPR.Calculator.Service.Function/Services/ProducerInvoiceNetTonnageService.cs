using System.Diagnostics;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using Microsoft.Extensions.Logging;

namespace EPR.Calculator.Service.Function.Services
{
    public class ProducerInvoiceNetTonnageService : IProducerInvoiceNetTonnageService
    {
        private IDbLoadingChunkerService<ProducerInvoicedMaterialNetTonnage> producerInvoiceMaterialChunker { get; init; }

        private readonly ILogger<ProducerInvoiceNetTonnageService> logger;

        private readonly IMaterialService materialService;

        private readonly IProducerInvoiceTonnageMapper producerInvoiceTonnageMapper;

        public ProducerInvoiceNetTonnageService(IDbLoadingChunkerService<ProducerInvoicedMaterialNetTonnage> producerInvoiceMaterialChunker,
            ILogger<ProducerInvoiceNetTonnageService> logger,
            IMaterialService materialService,
            IProducerInvoiceTonnageMapper producerInvoiceTonnageMapper)
        {
            this.producerInvoiceMaterialChunker = producerInvoiceMaterialChunker;
            this.materialService = materialService;
            this.logger = logger;
            this.producerInvoiceTonnageMapper = producerInvoiceTonnageMapper;
        }


        public async Task<bool> CreateProducerInvoiceNetTonnage(CalcResult calcResult)
        {
            try
            {
                var producerInvoiceNetTonnage = new List<ProducerInvoicedMaterialNetTonnage>();
                var stopwatch = Stopwatch.StartNew();

                var producers = calcResult.CalcResultSummary.ProducerDisposalFees.Where(producer => producer.Level == CommonConstants.LevelOne.ToString());

                var materials = await materialService.GetMaterials();

                var runId = calcResult.CalcResultDetail.RunId;

                var invoiceTonnages = producers.SelectMany(producer =>
                    materials.Select(material =>
                    {
                        var disposalFees = producer.ProducerDisposalFeesByMaterial;

                        if (disposalFees is not null && disposalFees.TryGetValue(material.Code, out var feeSummary))
                        {
                            return producerInvoiceTonnageMapper.Map(new ProducerInvoiceTonnage
                            {
                                RunId = runId,
                                ProducerId = producer.ProducerIdInt,
                                NetTonnage = feeSummary.NetReportedTonnage,
                                MaterialId = material.Id
                            });
                        }

                        return new ProducerInvoicedMaterialNetTonnage();
                    }).Where(x => x != null)
                );

                producerInvoiceNetTonnage.AddRange(invoiceTonnages);

                if (producerInvoiceNetTonnage.Exists(t => t.CalculatorRunId > 0))
                {
                    await producerInvoiceMaterialChunker.InsertRecords(producerInvoiceNetTonnage);

                    stopwatch.Stop();
                    logger.LogInformation("Inserted {RecordCount} producer invoice net tonnage records in {Elapsed}",
                        producerInvoiceNetTonnage.Count, stopwatch.Elapsed.ToString("g"));

                }
                else
                {
                    logger.LogWarning("No producer invoice net tonnage to insert");
                    return false;
                }
                return true;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error occurred while populating the producer invoice net tonnage");

                return false;
            }
        }
    }
}