namespace EPR.Calculator.Service.Function.Builder.CancelledProducers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.EntityFrameworkCore;

    public class CalcResultCancelledProducersBuilder : ICalcResultCancelledProducersBuilder
    {        
        public async Task<CalcResultCancelledProducersResponse> Construct(CalcResultsRequestDto resultsRequestDto)
        {        
            // Set up Top Header
            var topHeader = new CalcResultCancelledProducersDTO
            {
                ProducerId_Header = CommonConstants.ProducerId,
                SubsidiaryId_Header = CommonConstants.SubsidiaryId,
                ProducerOrSubsidiaryName_Header = CommonConstants.ProducerOrSubsidiaryName,
                TradingName_Header = CommonConstants.TradingName,
                LastTonnage = new LastTonnage
                {
                    LastTonnage_Header = CommonConstants.LastTonnage,
                    Aluminium_Header = CommonConstants.Aluminium,
                    FibreComposite_Header = CommonConstants.FibreComposite,
                    Glass_Header = CommonConstants.Glass,
                    PaperOrCard_Header = CommonConstants.PaperOrCard,
                    Plastic_Header = CommonConstants.Plastic,
                    Steel_Header = CommonConstants.Steel,
                    Wood_Header = CommonConstants.Wood,
                    OtherMaterials_Header = CommonConstants.OtherMaterials
                },
                LatestInvoice = new LatestInvoice
                {
                    LatestInvoice_Header = CommonConstants.LatestInvoice,
                    LastInvoicedTotal_Header = CommonConstants.LastInvoicedTotal,
                    RunNumber_Header = CommonConstants.RunNumber,
                    RunName_Header = CommonConstants.RunName,
                    BillingInstructionId_Header = CommonConstants.BillingInstructionId
                },
            };
            
            var response = new CalcResultCancelledProducersResponse
            {
                TitleHeader = CommonConstants.CancelledProducers,
                CancelledProducers = new List<CalcResultCancelledProducersDTO> { topHeader }
            };

            return response;
        }
    }
}