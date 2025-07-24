using Azure.Analytics.Synapse.Artifacts.Models;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Common.Logging;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Services
{
    public class ProducerInvoiceNetTonnageService : IProducerInvoiceNetTonnageService
    {
        private IDbLoadingChunkerService<ProducerInvoicedMaterialNetTonnage> producerInvoiceMaterialChunker { get; init; }

        private readonly ICalculatorTelemetryLogger telemetryLogger;

        private readonly IMaterialService materialService;

        private readonly IProducerInvoiceTonnageMapper producerInvoiceTonnageMapper;

        public ProducerInvoiceNetTonnageService(IDbLoadingChunkerService<ProducerInvoicedMaterialNetTonnage> producerInvoiceMaterialChunker,
            ICalculatorTelemetryLogger telemetryLogger,
            IMaterialService materialService,
            IProducerInvoiceTonnageMapper producerInvoiceTonnageMapper)
        {
            this.producerInvoiceMaterialChunker = producerInvoiceMaterialChunker;
            this.materialService = materialService;
            this.telemetryLogger = telemetryLogger;
            this.producerInvoiceTonnageMapper = producerInvoiceTonnageMapper;
        }


        public async Task<bool> CreateProducerInvoiceNetTonnage(CalcResult calcResult)
        {
            try
            {
                var producerInvoiceNetTonnage = new List<ProducerInvoicedMaterialNetTonnage>();
                var startTime = DateTime.UtcNow;

                var producers = calcResult.CalcResultSummary.ProducerDisposalFees.Where(producer => producer.Level == CommonConstants.LevelOne.ToString());

                var materials = await this.materialService.GetMaterials();

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

                if (producerInvoiceNetTonnage.Any(t => t.CalculatorRunId > 0))
                {
                    await this.producerInvoiceMaterialChunker.InsertRecords(producerInvoiceNetTonnage);

                    var endTime = DateTime.UtcNow;
                    var timeDiff = startTime - endTime;
                    this.telemetryLogger.LogInformation(new TrackMessage
                    {
                        RunId = calcResult.CalcResultDetail.RunId,
                        RunName = calcResult.CalcResultDetail.RunName,
                        Message = $"Inserting records {producerInvoiceNetTonnage.Count} into producer invoice net tonnage table for {calcResult.CalcResultDetail.RunId} completed in {timeDiff.TotalSeconds} seconds",
                    });

                }
                else
                {

                    this.telemetryLogger.LogInformation(new TrackMessage
                    {
                        RunId = calcResult.CalcResultDetail.RunId,
                        RunName = calcResult.CalcResultDetail.RunName,
                        Message = $"No producer invoice net tonnage to insert into table for {calcResult.CalcResultDetail.RunId}",
                    });
                    return false;
                }
                return true;
            }
            catch (Exception exception)
            {
                this.telemetryLogger.LogError(new ErrorMessage
                {
                    RunId = calcResult.CalcResultDetail.RunId,
                    RunName = calcResult.CalcResultDetail.RunName,
                    Message = "Error occurred while populating the  producer invoice net tonnage",
                    Exception = exception,
                });

                return false;
            }
        }
    }
}
