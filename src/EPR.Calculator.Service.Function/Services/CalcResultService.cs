using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;

namespace EPR.Calculator.Service.Function.Services
{
    public interface ICalcResultService
    {
        Task StoreProjectedH1Data(int runId, IReadOnlyList<CalcResultH1ProjectedProducer> projectedProducers, CancellationToken cancellationToken);
        Task StoreProjectedH2Data(int runId, IReadOnlyList<CalcResultH2ProjectedProducer> projectedProducers, CancellationToken cancellationToken);
        Task StoreScaledData(int runId, IReadOnlyList<CalcResultScaledupProducer> scaled, CancellationToken cancellationToken);
        Task StorePartialData(int runId, IReadOnlyList<CalcResultPartialObligation> partial, CancellationToken cancellationToken);
        Task StoreTransformedProducers(List<L1Producer> producerDetails, CancellationToken cancellationToken);
        Task StoreProducerDisposalFees(int runId, List<CalcResultSummaryProducerDisposalFees> producerDisposalFees, CancellationToken cancellationToken);
        Task<IReadOnlyList<CalcResultH1ProjectedProducer>> ReadH1ProjectedData(int runId, CancellationToken cancellationToken);
        Task<IReadOnlyList<CalcResultH2ProjectedProducer>> ReadH2ProjectedData(int runId, CancellationToken cancellationToken);
        Task<IReadOnlyList<CalcResultScaledupProducer>> ReadScaledData(int runId, CancellationToken cancellationToken);
        Task<IReadOnlyList<CalcResultPartialObligation>> ReadPartialData(int runId, CancellationToken cancellationToken);
        Task<IReadOnlyList<CalcResultSummaryProducerDisposalFees>> ReadProducerDisposalFees(int runId, CancellationToken cancellationToken);
    }

    public class CalcResultService(IBulkOperations bulkOps, ApplicationDBContext dbContext) : ICalcResultService
    {
        public async Task StoreProjectedH1Data(int runId, IReadOnlyList<CalcResultH1ProjectedProducer> projectedProducers, CancellationToken cancellationToken)
        {
            await bulkOps.BulkInsertAsync(dbContext, projectedProducers.SelectMany(p => 
                p.H1ProjectedTonnageByMaterial.Select(m => 
                    MapToTransformProjectedH1(runId, p.ProducerId, p.SubsidiaryId, m.Key, p.SubmissionPeriodCode, p.Level, m.Value)
                )
            ), cancellationToken);
        }

        public async Task StoreProjectedH2Data(int runId, IReadOnlyList<CalcResultH2ProjectedProducer> projectedProducers, CancellationToken cancellationToken)
        {
            await bulkOps.BulkInsertAsync(dbContext, projectedProducers.SelectMany(p => 
                p.H2ProjectedTonnageByMaterial.Select(m => 
                    MapToTransformProjectedH2(runId, p.ProducerId, p.SubsidiaryId, m.Key, p.SubmissionPeriodCode, p.Level, m.Value)
                )
            ), cancellationToken);
        }

        public async Task<IReadOnlyList<CalcResultH1ProjectedProducer>> ReadH1ProjectedData(int runId, CancellationToken cancellationToken)
        {
            return await dbContext.TransformProjectedH1
                        .Where(p => p.CalculatorRunId == runId)
                        .GroupBy(p => new { p.ProducerId, p.SubsidiaryId, p.SubmissionPeriodCode, p.Level })
                        .Select(g => new CalcResultH1ProjectedProducer
                        {
                            ProducerId = g.Key.ProducerId,
                            SubsidiaryId = g.Key.SubsidiaryId,
                            Level = g.Key.Level,
                            SubmissionPeriodCode = g.Key.SubmissionPeriodCode,
                            H1ProjectedTonnageByMaterial = MapToH1MaterialTonnages(g.ToList())
                        })
                        .OrderBy(p => p.ProducerId)
                        .ThenBy(p => p.Level)
                        .ThenBy(p => p.SubsidiaryId)
                        .ToImmutableListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<CalcResultH2ProjectedProducer>> ReadH2ProjectedData(int runId, CancellationToken cancellationToken)
        {
            return await dbContext.TransformProjectedH2
                        .Where(p => p.CalculatorRunId == runId)
                        .GroupBy(p => new { p.ProducerId, p.SubsidiaryId, p.SubmissionPeriodCode, p.Level })
                        .Select(g => new CalcResultH2ProjectedProducer
                        {
                            ProducerId = g.Key.ProducerId,
                            SubsidiaryId = g.Key.SubsidiaryId,
                            Level = g.Key.Level,
                            SubmissionPeriodCode = g.Key.SubmissionPeriodCode,
                            H2ProjectedTonnageByMaterial = MapToH2MaterialTonnages(g.ToList())
                        })
                        .OrderBy(p => p.ProducerId)
                        .ThenBy(p => p.Level)
                        .ThenBy(p => p.SubsidiaryId)
                        .ToImmutableListAsync(cancellationToken);
        }

        public async Task StoreScaledData(int runId, IReadOnlyList<CalcResultScaledupProducer> scaled, CancellationToken cancellationToken)
        {
             await bulkOps.BulkInsertAsync(dbContext, scaled.SelectMany(p => 
                p.PomData.Select(m => 
                    new TransformScaled
                    {
                        CalculatorRunId = runId,
                        ProducerId = p.ProducerId, 
                        SubsidiaryId = p.SubsidiaryId,
                        ProducerName = p.ProducerName,
                        TradingName = p.TradingName,
                        SubmissionPeriodCode = p.SubmissionPeriodCode,
                        Level = p.Level,
                        IsSubTotal = p.IsSubtotalRow,
                        DaysInSubmissionPeriod = p.DaysInSubmissionPeriod,
                        DaysInWholePeriod = p.DaysInWholePeriod,
                        ScaleupFactor = p.ScaleupFactor,
                        MaterialId = m.MaterialId,
                        PackagingType = m.PackagingType,
                        Tonnage = m.Tonnage,
                        ScaledTonnage = m.ScaledTonnage
                    }
                )
            ), cancellationToken);
        }
        
        public async Task<IReadOnlyList<CalcResultScaledupProducer>> ReadScaledData(int runId, CancellationToken cancellationToken)
        {
            return await dbContext.TransformScaled
                        .Where(p => p.CalculatorRunId == runId)
                        .GroupBy(p => new { p.ProducerId, p.SubsidiaryId, p.ProducerName, p.TradingName, p.SubmissionPeriodCode, p.Level, p.IsSubTotal, p.DaysInSubmissionPeriod, p.DaysInWholePeriod, p.ScaleupFactor })
                        .Select(g => 
                            new CalcResultScaledupProducer
                            {
                                ProducerId = g.Key.ProducerId,
                                SubsidiaryId = g.Key.SubsidiaryId,
                                ProducerName = g.Key.ProducerName,
                                TradingName = g.Key.TradingName,
                                Level = g.Key.Level,
                                IsSubtotalRow = g.Key.IsSubTotal,
                                SubmissionPeriodCode = g.Key.SubmissionPeriodCode,
                                DaysInSubmissionPeriod = g.Key.DaysInSubmissionPeriod,
                                DaysInWholePeriod = g.Key.DaysInWholePeriod,
                                ScaleupFactor = g.Key.ScaleupFactor,
                                PomData = MapToScaled(g.ToList())
                            }
                        )
                        .OrderBy(p => p.ProducerId)
                        .ThenBy(p => p.Level)
                        .ThenBy(p => p.SubsidiaryId)
                        .ThenBy(p => p.SubmissionPeriodCode)
                        .ToImmutableListAsync(cancellationToken);
        }

        public async Task StorePartialData(int runId, IReadOnlyList<CalcResultPartialObligation> partial, CancellationToken cancellationToken){
            await bulkOps.BulkInsertAsync(dbContext, partial.SelectMany(p => 
                p.PartialObligationTonnageByMaterial.Select(m => 
                    MapToTransformPartial(runId, m.Key, p, m.Value)
                )
            ), cancellationToken);
        }

        public async Task<IReadOnlyList<CalcResultPartialObligation>> ReadPartialData(int runId, CancellationToken cancellationToken){
            return await dbContext.TransformPartial
                        .Where(p => p.CalculatorRunId == runId)
                        .GroupBy(p => new { p.ProducerId, p.SubsidiaryId, p.ProducerName, p.TradingName, p.SubmissionYear, p.Level, p.DaysInSubmissionYear, p.JoiningDate, p.DaysObligated, p.ObligatedFactor })
                        .Select(g => 
                            new CalcResultPartialObligation
                            {
                                ProducerId = g.Key.ProducerId,
                                SubsidiaryId = g.Key.SubsidiaryId,
                                ProducerName = g.Key.ProducerName,
                                TradingName = g.Key.TradingName,
                                Level = g.Key.Level,
                                SubmissionYear = g.Key.SubmissionYear,
                                DaysInSubmissionYear = g.Key.DaysInSubmissionYear,
                                JoiningDate = g.Key.JoiningDate,
                                DaysObligated = g.Key.DaysObligated,
                                ObligatedFactor = g.Key.ObligatedFactor,
                                PartialObligationTonnageByMaterial = MapToPartial(g.ToList())
                            }
                        )
                        .OrderBy(p => p.ProducerId)
                        .ThenBy(p => p.Level)
                        .ThenBy(p => p.SubsidiaryId)
                        .ToImmutableListAsync(cancellationToken);
        }

        public async Task StoreTransformedProducers(List<L1Producer> producerDetails, CancellationToken cancellationToken)
        {
            await bulkOps.BulkInsertAsync(dbContext, producerDetails
                    .SelectMany(p => p.Producers)
                    .SelectMany(p => p.ProducerReportedMaterials.Select(rm =>
                        new TransformProducerReportedMaterial
                        {
                            ProducerDetailId             = rm.ProducerDetailId,
                            MaterialId                   = rm.MaterialId,
                            SubmissionPeriod             = rm.SubmissionPeriod,
                            PackagingType                = rm.PackagingType,
                            PackagingTonnage             = rm.PackagingTonnage,
                            PackagingTonnageRed          = rm.PackagingTonnageRed,
                            PackagingTonnageAmber        = rm.PackagingTonnageAmber,
                            PackagingTonnageGreen        = rm.PackagingTonnageGreen,
                            PackagingTonnageRedMedical   = rm.PackagingTonnageRedMedical,
                            PackagingTonnageAmberMedical = rm.PackagingTonnageAmberMedical,
                            PackagingTonnageGreenMedical = rm.PackagingTonnageGreenMedical
                        }
                    )
                ).ToList(), cancellationToken);
        }

        public async Task StoreProducerDisposalFees(int runId, List<CalcResultSummaryProducerDisposalFees> producerDisposalFees, CancellationToken cancellationToken)
        {
            await bulkOps.BulkInsertAsync(
                    dbContext, 
                    producerDisposalFees, 
                    cfg => { cfg.IncludeGraph = true; }, 
                    cancellationToken
                );
        }

        public async Task<IReadOnlyList<CalcResultSummaryProducerDisposalFees>> ReadProducerDisposalFees(int runId, CancellationToken cancellationToken)
        {
            return await dbContext.ProducerDisposalFee
                        .Include(p => p.ProducerDisposalFeesByMaterial)
                        .Where(p => p.CalculatorRunId == runId)
                        .ToImmutableListAsync(cancellationToken);
        }
    
        private static Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage> MapToH1MaterialTonnages(List<TransformProjectedH1> transformProjectedH1s)
        {
            return transformProjectedH1s.ToDictionary(
                t => t.MaterialCode, 
                t => new CalcResultH1ProjectedProducerMaterialTonnage
                {
                    HouseholdTonnage = t.HouseholdTonnage,
                    HouseholdRAMTonnage = new RAMTonnage
                    {
                        Red = t.HouseholdTonnageRed,
                        Amber = t.HouseholdTonnageAmber,
                        Green = t.HouseholdTonnageGreen,
                        RedMedical = t.HouseholdTonnageRedMedical,
                        AmberMedical = t.HouseholdTonnageAmberMedical,
                        GreenMedical = t.HouseholdTonnageGreenMedical
                    },
                    PublicBinTonnage = t.PublicBinTonnage,
                    PublicBinRAMTonnage = new RAMTonnage
                    {
                        Red = t.PublicBinTonnageRed,
                        Amber = t.PublicBinTonnageAmber,
                        Green = t.PublicBinTonnageGreen,
                        RedMedical = t.PublicBinTonnageRedMedical,
                        AmberMedical = t.PublicBinTonnageAmberMedical,
                        GreenMedical = t.PublicBinTonnageGreenMedical
                    },
                    HouseholdDrinksContainerTonnage = t.HDCTonnage,
                    HouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RAMTonnage
                    {
                        Red = t.HDCTonnageRed!.Value,
                        Amber = t.HDCTonnageAmber!.Value,
                        Green = t.HDCTonnageGreen!.Value,
                        RedMedical = t.HDCTonnageRedMedical!.Value,
                        AmberMedical = t.HDCTonnageAmberMedical!.Value,
                        GreenMedical = t.HDCTonnageGreenMedical!.Value
                    } : null,
                    HouseholdTonnageWithoutRAM = t.HouseholdTonnageWithoutRAM,
                    PublicBinTonnageWithoutRAM = t.PublicBinTonnageWithoutRAM,
                    HouseholdDrinksContainerTonnageWithoutRAM = t.HDCTonnageWithoutRAM,
                    ProjectedHouseholdTonnage = t.ProjectedHouseholdTonnage,
                    ProjectedHouseholdRAMTonnage = new RAMTonnage
                    {
                        Red = t.ProjectedHouseholdTonnageRed,
                        Amber = t.ProjectedHouseholdTonnageAmber,
                        Green = t.ProjectedHouseholdTonnageGreen,
                        RedMedical = t.ProjectedHouseholdTonnageRedMedical,
                        AmberMedical = t.ProjectedHouseholdTonnageAmberMedical,
                        GreenMedical = t.ProjectedHouseholdTonnageGreenMedical
                    },
                    ProjectedPublicBinTonnage = t.ProjectedPublicBinTonnage,
                    ProjectedPublicBinRAMTonnage = new RAMTonnage
                    {
                        Red = t.ProjectedPublicBinTonnageRed,
                        Amber = t.ProjectedPublicBinTonnageAmber,
                        Green = t.ProjectedPublicBinTonnageGreen,
                        RedMedical = t.ProjectedPublicBinTonnageRedMedical,
                        AmberMedical = t.ProjectedPublicBinTonnageAmberMedical,
                        GreenMedical = t.ProjectedPublicBinTonnageGreenMedical
                    },
                    ProjectedHouseholdDrinksContainerTonnage = t.ProjectedHDCTonnage,
                    ProjectedHouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RAMTonnage
                    {
                        Red = t.ProjectedHDCTonnageRed!.Value,
                        Amber = t.ProjectedHDCTonnageAmber!.Value,
                        Green = t.ProjectedHDCTonnageGreen!.Value,
                        RedMedical = t.ProjectedHDCTonnageRedMedical!.Value,
                        AmberMedical = t.ProjectedHDCTonnageAmberMedical!.Value,
                        GreenMedical = t.ProjectedHDCTonnageGreenMedical!.Value
                    } : null,
                    H2RamProportions = new RAMProportions
                    {
                        Red = t.H2RamProportionsRed,
                        Amber = t.H2RamProportionsAmber,
                        Green = t.H2RamProportionsGreen,
                        RedMedical = t.H2RamProportionsRedMedical,
                        AmberMedical = t.H2RamProportionsAmberMedical,
                        GreenMedical = t.H2RamProportionsGreenMedical
                    }
                }
            );
        }

        private static Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage> MapToH2MaterialTonnages(List<TransformProjectedH2> transformProjectedH2s)
        {
            return transformProjectedH2s.ToDictionary(
                t => t.MaterialCode,
                t => new CalcResultH2ProjectedProducerMaterialTonnage
                {
                    HouseholdTonnage = t.HouseholdTonnage,
                    HouseholdRAMTonnage = new RAMTonnage
                    {
                        Red = t.HouseholdTonnageRed,
                        Amber = t.HouseholdTonnageAmber,
                        Green = t.HouseholdTonnageGreen,
                        RedMedical = t.HouseholdTonnageRedMedical,
                        AmberMedical = t.HouseholdTonnageAmberMedical,
                        GreenMedical = t.HouseholdTonnageGreenMedical
                    },
                    PublicBinTonnage = t.PublicBinTonnage,
                    PublicBinRAMTonnage = new RAMTonnage
                    {
                        Red = t.PublicBinTonnageRed,
                        Amber = t.PublicBinTonnageAmber,
                        Green = t.PublicBinTonnageGreen,
                        RedMedical = t.PublicBinTonnageRedMedical,
                        AmberMedical = t.PublicBinTonnageAmberMedical,
                        GreenMedical = t.PublicBinTonnageGreenMedical
                    },
                    HouseholdDrinksContainerTonnage = t.HDCTonnage,
                    HouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RAMTonnage
                    {
                        Red = t.HDCTonnageRed!.Value,
                        Amber = t.HDCTonnageAmber!.Value,
                        Green = t.HDCTonnageGreen!.Value,
                        RedMedical = t.HDCTonnageRedMedical!.Value,
                        AmberMedical = t.HDCTonnageAmberMedical!.Value,
                        GreenMedical = t.HDCTonnageGreenMedical!.Value
                    } : null,
                    HouseholdTonnageWithoutRAM = t.HouseholdTonnageWithoutRAM,
                    PublicBinTonnageWithoutRAM = t.PublicBinTonnageWithoutRAM,
                    HouseholdDrinksContainerTonnageWithoutRAM = t.HDCTonnageWithoutRAM,
                    ProjectedHouseholdTonnage = t.ProjectedHouseholdTonnage,
                    ProjectedHouseholdRAMTonnage = new RAMTonnage
                    {
                        Red = t.ProjectedHouseholdTonnageRed,
                        Amber = t.ProjectedHouseholdTonnageAmber,
                        Green = t.ProjectedHouseholdTonnageGreen,
                        RedMedical = t.ProjectedHouseholdTonnageRedMedical,
                        AmberMedical = t.ProjectedHouseholdTonnageAmberMedical,
                        GreenMedical = t.ProjectedHouseholdTonnageGreenMedical
                    },
                    ProjectedPublicBinTonnage = t.ProjectedPublicBinTonnage,
                    ProjectedPublicBinRAMTonnage = new RAMTonnage
                    {
                        Red = t.ProjectedPublicBinTonnageRed,
                        Amber = t.ProjectedPublicBinTonnageAmber,
                        Green = t.ProjectedPublicBinTonnageGreen,
                        RedMedical = t.ProjectedPublicBinTonnageRedMedical,
                        AmberMedical = t.ProjectedPublicBinTonnageAmberMedical,
                        GreenMedical = t.ProjectedPublicBinTonnageGreenMedical
                    },
                    ProjectedHouseholdDrinksContainerTonnage = t.ProjectedHDCTonnage,
                    ProjectedHouseholdDrinksContainerRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? new RAMTonnage
                    {
                        Red = t.ProjectedHDCTonnageRed!.Value,
                        Amber = t.ProjectedHDCTonnageAmber!.Value,
                        Green = t.ProjectedHDCTonnageGreen!.Value,
                        RedMedical = t.ProjectedHDCTonnageRedMedical!.Value,
                        AmberMedical = t.ProjectedHDCTonnageAmberMedical!.Value,
                        GreenMedical = t.ProjectedHDCTonnageGreenMedical!.Value
                    } : null
                }
            );
        }

        private static TransformProjectedH1 MapToTransformProjectedH1(int runId, int producerId, string? subsidiaryId, string materialCode, string submissionPeriod, string level, CalcResultH1ProjectedProducerMaterialTonnage tonnage)
        {
            return new TransformProjectedH1
            {
                CalculatorRunId = runId,
                ProducerId = producerId,
                SubsidiaryId = subsidiaryId,
                MaterialCode = materialCode,
                SubmissionPeriodCode = submissionPeriod,
                Level = level,
                HouseholdTonnage = tonnage.HouseholdTonnage,
                HouseholdTonnageRed = tonnage.HouseholdRAMTonnage.Red,
                HouseholdTonnageAmber = tonnage.HouseholdRAMTonnage.Amber,
                HouseholdTonnageGreen = tonnage.HouseholdRAMTonnage.Green,
                HouseholdTonnageRedMedical = tonnage.HouseholdRAMTonnage.RedMedical,
                HouseholdTonnageAmberMedical = tonnage.HouseholdRAMTonnage.AmberMedical,
                HouseholdTonnageGreenMedical = tonnage.HouseholdRAMTonnage.GreenMedical,
                PublicBinTonnage = tonnage.PublicBinTonnage,
                PublicBinTonnageRed = tonnage.PublicBinRAMTonnage.Red,
                PublicBinTonnageAmber = tonnage.PublicBinRAMTonnage.Amber,
                PublicBinTonnageGreen = tonnage.PublicBinRAMTonnage.Green,
                PublicBinTonnageRedMedical = tonnage.PublicBinRAMTonnage.RedMedical,
                PublicBinTonnageAmberMedical = tonnage.PublicBinRAMTonnage.AmberMedical,
                PublicBinTonnageGreenMedical = tonnage.PublicBinRAMTonnage.GreenMedical,
                HDCTonnage = tonnage.HouseholdDrinksContainerTonnage,
                HDCTonnageRed = tonnage.HouseholdDrinksContainerRAMTonnage?.Red,
                HDCTonnageAmber = tonnage.HouseholdDrinksContainerRAMTonnage?.Amber,
                HDCTonnageGreen = tonnage.HouseholdDrinksContainerRAMTonnage?.Green,
                HDCTonnageRedMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.RedMedical,
                HDCTonnageAmberMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.AmberMedical,
                HDCTonnageGreenMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.GreenMedical,
                HouseholdTonnageWithoutRAM = tonnage.HouseholdTonnageWithoutRAM,
                PublicBinTonnageWithoutRAM = tonnage.PublicBinTonnageWithoutRAM,
                HDCTonnageWithoutRAM = tonnage.HouseholdDrinksContainerTonnageWithoutRAM,
                ProjectedHouseholdTonnage = tonnage.ProjectedHouseholdTonnage,
                ProjectedHouseholdTonnageRed = tonnage.ProjectedHouseholdRAMTonnage.Red,
                ProjectedHouseholdTonnageAmber = tonnage.ProjectedHouseholdRAMTonnage.Amber,
                ProjectedHouseholdTonnageGreen = tonnage.ProjectedHouseholdRAMTonnage.Green,
                ProjectedHouseholdTonnageRedMedical = tonnage.ProjectedHouseholdRAMTonnage.RedMedical,
                ProjectedHouseholdTonnageAmberMedical = tonnage.ProjectedHouseholdRAMTonnage.AmberMedical,
                ProjectedHouseholdTonnageGreenMedical = tonnage.ProjectedHouseholdRAMTonnage.GreenMedical,
                ProjectedPublicBinTonnage = tonnage.ProjectedPublicBinTonnage,
                ProjectedPublicBinTonnageRed = tonnage.ProjectedPublicBinRAMTonnage.Red,
                ProjectedPublicBinTonnageAmber = tonnage.ProjectedPublicBinRAMTonnage.Amber,
                ProjectedPublicBinTonnageGreen = tonnage.ProjectedPublicBinRAMTonnage.Green,
                ProjectedPublicBinTonnageRedMedical = tonnage.ProjectedPublicBinRAMTonnage.RedMedical,
                ProjectedPublicBinTonnageAmberMedical = tonnage.ProjectedPublicBinRAMTonnage.AmberMedical,
                ProjectedPublicBinTonnageGreenMedical = tonnage.ProjectedPublicBinRAMTonnage.GreenMedical,
                ProjectedHDCTonnage = tonnage.ProjectedHouseholdDrinksContainerTonnage,
                ProjectedHDCTonnageRed = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.Red,
                ProjectedHDCTonnageAmber = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.Amber,
                ProjectedHDCTonnageGreen = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.Green,
                ProjectedHDCTonnageRedMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedical,
                ProjectedHDCTonnageAmberMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedical,
                ProjectedHDCTonnageGreenMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedical,
                H2RamProportionsRed = tonnage.H2RamProportions.Red,
                H2RamProportionsAmber = tonnage.H2RamProportions.Amber,
                H2RamProportionsGreen = tonnage.H2RamProportions.Green,
                H2RamProportionsRedMedical = tonnage.H2RamProportions.RedMedical,
                H2RamProportionsAmberMedical = tonnage.H2RamProportions.AmberMedical,
                H2RamProportionsGreenMedical = tonnage.H2RamProportions.GreenMedical
            };
        }

        private static TransformProjectedH2 MapToTransformProjectedH2(int runId, int producerId, string? subsidiaryId, string materialCode, string submissionPeriod, string level,CalcResultH2ProjectedProducerMaterialTonnage tonnage)
        {
            return new TransformProjectedH2
            {
                CalculatorRunId = runId,
                ProducerId = producerId,
                SubsidiaryId = subsidiaryId,
                MaterialCode = materialCode,
                Level = level,
                SubmissionPeriodCode = submissionPeriod,
                HouseholdTonnage = tonnage.HouseholdTonnage,
                HouseholdTonnageRed = tonnage.HouseholdRAMTonnage.Red,
                HouseholdTonnageAmber = tonnage.HouseholdRAMTonnage.Amber,
                HouseholdTonnageGreen = tonnage.HouseholdRAMTonnage.Green,
                HouseholdTonnageRedMedical = tonnage.HouseholdRAMTonnage.RedMedical,
                HouseholdTonnageAmberMedical = tonnage.HouseholdRAMTonnage.AmberMedical,
                HouseholdTonnageGreenMedical = tonnage.HouseholdRAMTonnage.GreenMedical,
                PublicBinTonnage = tonnage.PublicBinTonnage,
                PublicBinTonnageRed = tonnage.PublicBinRAMTonnage.Red,
                PublicBinTonnageAmber = tonnage.PublicBinRAMTonnage.Amber,
                PublicBinTonnageGreen = tonnage.PublicBinRAMTonnage.Green,
                PublicBinTonnageRedMedical = tonnage.PublicBinRAMTonnage.RedMedical,
                PublicBinTonnageAmberMedical = tonnage.PublicBinRAMTonnage.AmberMedical,
                PublicBinTonnageGreenMedical = tonnage.PublicBinRAMTonnage.GreenMedical,
                HDCTonnage = tonnage.HouseholdDrinksContainerTonnage,
                HDCTonnageRed = tonnage.HouseholdDrinksContainerRAMTonnage?.Red,
                HDCTonnageAmber = tonnage.HouseholdDrinksContainerRAMTonnage?.Amber,
                HDCTonnageGreen = tonnage.HouseholdDrinksContainerRAMTonnage?.Green,
                HDCTonnageRedMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.RedMedical,
                HDCTonnageAmberMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.AmberMedical,
                HDCTonnageGreenMedical = tonnage.HouseholdDrinksContainerRAMTonnage?.GreenMedical,
                HouseholdTonnageWithoutRAM = tonnage.HouseholdTonnageWithoutRAM,
                PublicBinTonnageWithoutRAM = tonnage.PublicBinTonnageWithoutRAM,
                HDCTonnageWithoutRAM = tonnage.HouseholdDrinksContainerTonnageWithoutRAM,
                ProjectedHouseholdTonnage = tonnage.ProjectedHouseholdTonnage,
                ProjectedHouseholdTonnageRed = tonnage.ProjectedHouseholdRAMTonnage.Red,
                ProjectedHouseholdTonnageAmber = tonnage.ProjectedHouseholdRAMTonnage.Amber,
                ProjectedHouseholdTonnageGreen = tonnage.ProjectedHouseholdRAMTonnage.Green,
                ProjectedHouseholdTonnageRedMedical = tonnage.ProjectedHouseholdRAMTonnage.RedMedical,
                ProjectedHouseholdTonnageAmberMedical = tonnage.ProjectedHouseholdRAMTonnage.AmberMedical,
                ProjectedHouseholdTonnageGreenMedical = tonnage.ProjectedHouseholdRAMTonnage.GreenMedical,
                ProjectedPublicBinTonnage = tonnage.ProjectedPublicBinTonnage,
                ProjectedPublicBinTonnageRed = tonnage.ProjectedPublicBinRAMTonnage.Red,
                ProjectedPublicBinTonnageAmber = tonnage.ProjectedPublicBinRAMTonnage.Amber,
                ProjectedPublicBinTonnageGreen = tonnage.ProjectedPublicBinRAMTonnage.Green,
                ProjectedPublicBinTonnageRedMedical = tonnage.ProjectedPublicBinRAMTonnage.RedMedical,
                ProjectedPublicBinTonnageAmberMedical = tonnage.ProjectedPublicBinRAMTonnage.AmberMedical,
                ProjectedPublicBinTonnageGreenMedical = tonnage.ProjectedPublicBinRAMTonnage.GreenMedical,
                ProjectedHDCTonnage = tonnage.ProjectedHouseholdDrinksContainerTonnage,
                ProjectedHDCTonnageRed = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.Red,
                ProjectedHDCTonnageAmber = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.Amber,
                ProjectedHDCTonnageGreen = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.Green,
                ProjectedHDCTonnageRedMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.RedMedical,
                ProjectedHDCTonnageAmberMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.AmberMedical,
                ProjectedHDCTonnageGreenMedical = tonnage.ProjectedHouseholdDrinksContainerRAMTonnage?.GreenMedical
            };
        }

        private static ImmutableList<ScaledupPomEntry> MapToScaled(List<TransformScaled> scaled)
        {
            return scaled.Select(s => 
                new ScaledupPomEntry(
                    MaterialId: s.MaterialId,
                    PackagingType: s.PackagingType,
                    Tonnage: s.Tonnage,
                    ScaledTonnage: s.ScaledTonnage
                )
            ).ToImmutableList();
        }

        private static TransformPartial MapToTransformPartial(int runId, string materialCode, CalcResultPartialObligation producer, CalcResultPartialObligationTonnage tonnage)
        {
            return new TransformPartial
            {
                CalculatorRunId = runId,
                ProducerId = producer.ProducerId,
                SubsidiaryId = producer.SubsidiaryId,
                ProducerName = producer.ProducerName,
                TradingName = producer.TradingName,
                Level = producer.Level,
                SubmissionYear = producer.SubmissionYear,
                DaysInSubmissionYear = producer.DaysInSubmissionYear,
                JoiningDate = producer.JoiningDate,
                DaysObligated = producer.DaysObligated,
                ObligatedFactor = producer.ObligatedFactor,
                MaterialCode = materialCode,
                HouseholdTonnage = tonnage.HouseholdTonnage,
                HouseholdTonnageRed = tonnage.HouseholdRAMTonnage?.Red,
                HouseholdTonnageAmber = tonnage.HouseholdRAMTonnage?.Amber,
                HouseholdTonnageGreen = tonnage.HouseholdRAMTonnage?.Green,
                HouseholdTonnageRedMedical = tonnage.HouseholdRAMTonnage?.RedMedical,
                HouseholdTonnageAmberMedical = tonnage.HouseholdRAMTonnage?.AmberMedical,
                HouseholdTonnageGreenMedical = tonnage.HouseholdRAMTonnage?.GreenMedical,
                PublicBinTonnage = tonnage.PublicBinTonnage,
                PublicBinTonnageRed = tonnage.PublicBinRAMTonnage?.Red,
                PublicBinTonnageAmber = tonnage.PublicBinRAMTonnage?.Amber,
                PublicBinTonnageGreen = tonnage.PublicBinRAMTonnage?.Green,
                PublicBinTonnageRedMedical = tonnage.PublicBinRAMTonnage?.RedMedical,
                PublicBinTonnageAmberMedical = tonnage.PublicBinRAMTonnage?.AmberMedical,
                PublicBinTonnageGreenMedical = tonnage.PublicBinRAMTonnage?.GreenMedical,
                HDCTonnage = tonnage.HouseholdDrinksContainersTonnage,
                HDCTonnageRed = tonnage.HouseholdDrinksContainersRAMTonnage?.Red,
                HDCTonnageAmber = tonnage.HouseholdDrinksContainersRAMTonnage?.Amber,
                HDCTonnageGreen = tonnage.HouseholdDrinksContainersRAMTonnage?.Green,
                HDCTonnageRedMedical = tonnage.HouseholdDrinksContainersRAMTonnage?.RedMedical,
                HDCTonnageAmberMedical = tonnage.HouseholdDrinksContainersRAMTonnage?.AmberMedical,
                HDCTonnageGreenMedical = tonnage.HouseholdDrinksContainersRAMTonnage?.GreenMedical,
                SMCWTonnage = tonnage.SelfManagedConsumerWasteTonnage
            };
        }

        private static Dictionary<string, CalcResultPartialObligationTonnage> MapToPartial(List<TransformPartial> partial)
        {
            RAMTonnage? ToMaybeRamTonnage(
                decimal? red,
                decimal? amber,
                decimal? green,
                decimal? redMedical,
                decimal? amberMedical,
                decimal? greenMedical)
            {
                return red is null && amber is null && green is null && redMedical is null && amberMedical is null && greenMedical is null
                    ? null
                    : new RAMTonnage
                    {
                        Red = red!.Value,
                        Amber = amber!.Value,
                        Green = green!.Value,
                        RedMedical = redMedical!.Value,
                        AmberMedical = amberMedical!.Value,
                        GreenMedical = greenMedical!.Value
                    };
            }

            return partial.ToDictionary(
                t => t.MaterialCode,
                t => new CalcResultPartialObligationTonnage
                {
                    ObligatedFactor = t.ObligatedFactor,
                    HouseholdTonnage = t.HouseholdTonnage,
                    HouseholdRAMTonnage = ToMaybeRamTonnage(
                        t.HouseholdTonnageRed,
                        t.HouseholdTonnageAmber,
                        t.HouseholdTonnageGreen,
                        t.HouseholdTonnageRedMedical,
                        t.HouseholdTonnageAmberMedical,
                        t.HouseholdTonnageGreenMedical
                    ),
                    PublicBinTonnage = t.PublicBinTonnage,
                    PublicBinRAMTonnage = ToMaybeRamTonnage(
                        t.PublicBinTonnageRed,
                        t.PublicBinTonnageAmber,
                        t.PublicBinTonnageGreen,
                        t.PublicBinTonnageRedMedical,
                        t.PublicBinTonnageAmberMedical,
                        t.PublicBinTonnageGreenMedical
                    ),
                    HouseholdDrinksContainersTonnage = t.HDCTonnage,
                    HouseholdDrinksContainersRAMTonnage = t.MaterialCode == MaterialCodes.Glass ? ToMaybeRamTonnage(
                        t.HDCTonnageRed,
                        t.HDCTonnageAmber,
                        t.HDCTonnageGreen,
                        t.HDCTonnageRedMedical,
                        t.HDCTonnageAmberMedical,
                        t.HDCTonnageGreenMedical
                    ) : null,
                    SelfManagedConsumerWasteTonnage = t.SMCWTonnage
                }
            );
        }

    }
}
