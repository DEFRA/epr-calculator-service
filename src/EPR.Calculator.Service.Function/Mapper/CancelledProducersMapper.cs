using System;
using System.Collections.Generic;
using System.Linq;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Mapper
{
    public class CancelledProducersMapper : ICancelledProducersMapper
    {
        public CancelledProducers Map(CalcResultCancelledProducersResponse calcResultCancelledProducersResponse)
        {
            if (!calcResultCancelledProducersResponse.CancelledProducers.Any())
            {
                return new CancelledProducers
                {
                    Name = CommonConstants.CancelledProducers,
                    CancelledProducerTonnageInvoice = Array.Empty<CancelledProducerTonnageInvoice>()
                };
            }

            return new CancelledProducers
            {
                Name = CommonConstants.CancelledProducers,
                CancelledProducerTonnageInvoice = GetCancelledProducerTonnageInvoice(calcResultCancelledProducersResponse)
            };
        }

        private IEnumerable<CancelledProducerTonnageInvoice> GetCancelledProducerTonnageInvoice(CalcResultCancelledProducersResponse calcResultCancelledProducersResponse)
        {
            var cancelledProducerTonnageInvoices = new List<CancelledProducerTonnageInvoice>();

            foreach (var producer in calcResultCancelledProducersResponse.CancelledProducers)
            {
                int runNumber = 0;
                if (!string.IsNullOrWhiteSpace(producer.LatestInvoice?.RunNumberValue))
                {
                    _ = int.TryParse(producer.LatestInvoice!.RunNumberValue, out runNumber);
                }

                cancelledProducerTonnageInvoices.Add(new CancelledProducerTonnageInvoice
                {
                    ProducerId = producer.ProducerId,
                    SubsidiaryId = producer.SubsidiaryIdValue ?? string.Empty,
                    ProducerName = producer.ProducerOrSubsidiaryNameValue ?? string.Empty,
                    TradingName = producer.TradingNameValue ?? string.Empty,
                    LastProducerTonnages = GetLastProducerTonnages(producer.LastTonnage!),

                    RunNumber = runNumber,
                    RunName = producer.LatestInvoice?.RunNameValue ?? string.Empty,
                    BillingInstructionID = producer.LatestInvoice?.BillingInstructionIdValue ?? string.Empty,
                    LastInvoicedTotal = producer.LatestInvoice?.CurrentYearInvoicedTotalToDateValue ?? 0m
                });
            }

            return cancelledProducerTonnageInvoices;
        }

        private IEnumerable<LastProducerTonnages> GetLastProducerTonnages(LastTonnage lastTonnage)
        {
            var lastProducerTonnagesList = new List<LastProducerTonnages>();

            lastProducerTonnagesList.AddRange([
                new LastProducerTonnages
                {
                    MaterialName = lastTonnage.Aluminium_Header ?? MaterialNames.Aluminium,
                    LastTonnage = lastTonnage.AluminiumValue ?? CommonConstants.DefaultMinValue,
                },
                new LastProducerTonnages
                {
                    MaterialName = lastTonnage.FibreComposite_Header ?? MaterialNames.FibreComposite,
                    LastTonnage = lastTonnage.FibreCompositeValue ?? CommonConstants.DefaultMinValue,
                },
                new LastProducerTonnages
                {
                    MaterialName = lastTonnage.Glass_Header ?? MaterialNames.Glass,
                    LastTonnage = lastTonnage.GlassValue ?? CommonConstants.DefaultMinValue,
                },
                new LastProducerTonnages
                {
                    MaterialName = lastTonnage.PaperOrCard_Header ?? MaterialNames.PaperOrCard,
                    LastTonnage = lastTonnage.PaperOrCardValue ?? CommonConstants.DefaultMinValue,
                },
                new LastProducerTonnages
                {
                    MaterialName = lastTonnage.Plastic_Header ?? MaterialNames.Plastic,
                    LastTonnage = lastTonnage.PlasticValue ?? CommonConstants.DefaultMinValue,
                },
                new LastProducerTonnages
                {
                    MaterialName = lastTonnage.Steel_Header ?? MaterialNames.Steel,
                    LastTonnage = lastTonnage.SteelValue ?? CommonConstants.DefaultMinValue,
                },
                new LastProducerTonnages
                {
                    MaterialName = lastTonnage.Wood_Header ?? MaterialNames.Wood,
                    LastTonnage = lastTonnage.WoodValue ?? CommonConstants.DefaultMinValue,
                },
                new LastProducerTonnages
                {
                    MaterialName = lastTonnage.OtherMaterials_Header ?? MaterialNames.OtherMaterials,
                    LastTonnage = lastTonnage.OtherMaterialsValue ?? CommonConstants.DefaultMinValue,
                }
            ]);

            return lastProducerTonnagesList;
        }
    }
}
