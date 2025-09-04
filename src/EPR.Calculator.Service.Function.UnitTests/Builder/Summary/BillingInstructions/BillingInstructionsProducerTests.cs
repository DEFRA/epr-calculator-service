using System.Globalization;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.BillingInstructions
{
    using AutoFixture;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Builder.ParametersOther;
    using EPR.Calculator.Service.Function.Builder.Summary.BillingInstructions;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Defines the <see cref="BillingInstructionsProducerTests" />
    /// </summary>
    [TestClass]
    public class BillingInstructionsProducerTests
    {
        /// <summary>
        /// Defines the _dbContext
        /// </summary>
        private readonly ApplicationDBContext _dbContext;

        /// <summary>
        /// Defines the _materials
        /// </summary>
        private readonly IEnumerable<MaterialDetail> _materials;

        /// <summary>
        /// Defines the _calcResult
        /// </summary>
        private readonly CalcResult _calcResult;

        /// <summary>
        /// Defines the _materialCostSummary
        /// </summary>
        private readonly Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial> _materialCostSummary;

        /// <summary>
        /// Defines the _commsCostSummary
        /// </summary>
        private readonly Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial> _commsCostSummary;

        /// <summary>
        /// Gets the Fixture
        /// </summary>
        private Fixture Fixture { get; init; } = new Fixture();

        /// <summary>
        /// Initializes a new instance of the <see cref="BillingInstructionsProducerTests"/> class.
        /// </summary>
        public BillingInstructionsProducerTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _dbContext = new ApplicationDBContext(dbContextOptions);
            _dbContext.Database.EnsureCreated();

            CreateMaterials();
            CreateProducerDetail();

            _materials = [
                new MaterialDetail
                {
                    Id = 1,
                    Code = "AL",
                    Name = "Aluminium",
                    Description = "Aluminium",
                },
                new MaterialDetail
                {
                    Id = 2,
                    Code = "FC",
                    Name = "Fibre composite",
                    Description = "Fibre composite",
                },
                new MaterialDetail
                {
                    Id = 3,
                    Code = "GL",
                    Name = "Glass",
                    Description = "Glass",
                },
                new MaterialDetail
                {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card",
                },
                new MaterialDetail
                {
                    Id = 5,
                    Code = "PL",
                    Name = "Plastic",
                    Description = "Plastic",
                },
                new MaterialDetail
                {
                    Id = 6,
                    Code = "ST",
                    Name = "Steel",
                    Description = "Steel",
                },
                new MaterialDetail
                {
                    Id = 7,
                    Code = "WD",
                    Name = "Wood",
                    Description = "Wood",
                },
                new MaterialDetail
                {
                    Id = 8,
                    Code = "OT",
                    Name = "Other materials",
                    Description = "Other materials",
                }
            ];

            _calcResult = TestDataHelper.GetCalcResult();

            _materialCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>();
            _commsCostSummary = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>();

            foreach (var material in _materials)
            {
                _materialCostSummary.Add(material, new CalcResultSummaryProducerDisposalFeesByMaterial
                {
                    HouseholdPackagingWasteTonnage = 1000,
                    ManagedConsumerWasteTonnage = 90,
                    NetReportedTonnage = 910,
                    PricePerTonne = 0.6676m,
                    ProducerDisposalFee = 607.52m,
                    BadDebtProvision = 36.45m,
                    ProducerDisposalFeeWithBadDebtProvision = 643.97m,
                    EnglandWithBadDebtProvision = 348.06m,
                    WalesWithBadDebtProvision = 78.46m,
                    ScotlandWithBadDebtProvision = 156.28m,
                    NorthernIrelandWithBadDebtProvision = 61.18m,
                });

                _commsCostSummary.Add(material, new CalcResultSummaryProducerCommsFeesCostByMaterial
                {
                    HouseholdPackagingWasteTonnage = 1000,
                    PriceperTonne = 0.6676m,
                    ProducerTotalCostWithoutBadDebtProvision = 607.52m,
                    BadDebtProvision = 36.45m,
                    ProducerTotalCostwithBadDebtProvision = 643.97m,
                    EnglandWithBadDebtProvision = 348.06m,
                    WalesWithBadDebtProvision = 78.46m,
                    ScotlandWithBadDebtProvision = 156.28m,
                    NorthernIrelandWithBadDebtProvision = 61.18m,
                });
            }
        }

        /// <summary>
        /// The TearDown
        /// </summary>
        [TestCleanup]
        public void TearDown()
        {
            _dbContext?.Database.EnsureDeleted();
        }

        /// <summary>
        /// The CanCallGetHeaders
        /// </summary>
        [TestMethod]
        public void CanCallGetHeaders()
        {
            // Act
            var result = BillingInstructionsProducer.GetHeaders().ToList();
            var columnIndex = 292;

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.CurrentYearInvoicedTotalToDate, ColumnIndex = columnIndex },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.TonnageChangeSinceLastInvoice, ColumnIndex = columnIndex + 1 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.LiabilityDifference, ColumnIndex = columnIndex + 2 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.MaterialThresholdBreached, ColumnIndex = columnIndex + 3 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.TonnageThresholdBreached, ColumnIndex = columnIndex + 4 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.PercentageLiabilityDifference, ColumnIndex = columnIndex + 5 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.MaterialPercentageThresholdBreached, ColumnIndex = columnIndex + 6 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.TonnagePercentagThresholdBreached, ColumnIndex = columnIndex + 6 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.SuggestedBillingInstruction, ColumnIndex = columnIndex + 6 },
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.SuggestedInvoiceAmount, ColumnIndex = columnIndex + 6 }
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[0].ColumnIndex, result[0].ColumnIndex);
            Assert.AreEqual(expectedResult[1].Name, result[1].Name);
            Assert.AreEqual(expectedResult[1].ColumnIndex, result[1].ColumnIndex);
            Assert.AreEqual(expectedResult[2].Name, result[2].Name);
            Assert.AreEqual(expectedResult[2].ColumnIndex, result[2].ColumnIndex);
            Assert.AreEqual(expectedResult[3].Name, result[3].Name);
            Assert.AreEqual(expectedResult[3].ColumnIndex, result[3].ColumnIndex);
            Assert.AreEqual(expectedResult[4].Name, result[4].Name);
            Assert.AreEqual(expectedResult[4].ColumnIndex, result[4].ColumnIndex);
            Assert.AreEqual(expectedResult[5].Name, result[5].Name);
            Assert.AreEqual(expectedResult[5].ColumnIndex, result[5].ColumnIndex);
            Assert.AreEqual(expectedResult[6].Name, result[6].Name);
            Assert.AreEqual(expectedResult[6].ColumnIndex, result[6].ColumnIndex);
        }

        /// <summary>
        /// The CanCallGetSummaryHeaders
        /// </summary>
        [TestMethod]
        public void CanCallGetSummaryHeaders()
        {
            // Act
            var result = BillingInstructionsProducer.GetSummaryHeaders().ToList();

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = BillingInstructionsHeader.Title, ColumnIndex = 292 }
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[0].ColumnIndex, result[0].ColumnIndex);
        }

        /// <summary>
        /// The CanCallSetValues
        /// </summary>

        [TestMethod]
        public void CanCallSetValues()
        {
            // Act
            var producerInvoicedMaterialNetTonnage = new List<ProducerInvoicedDto>
            {
                new ProducerInvoicedDto
                {
                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage
                    {
                        CalculatorRunId = 101,
                        InvoicedNetTonnage = 20,
                        MaterialId = 77,
                        ProducerId = 1,
                        Id = 22
                    },
                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction
                    {
                        ProducerId = 1,
                        Id = 22,
                        CurrentYearInvoicedTotalAfterThisRun = 20.00m
                    },
                    CalculatorRun = new CalculatorRun
                    {
                        Name = "Test",
                        Financial_Year = new CalculatorRunFinancialYear { Name = "2025-26" },
                        Id = 22
                    }
                }
            };

            var defaultParams = new List<DefaultParamResultsClass>();

            BillingInstructionsProducer.SetValues(_calcResult.CalcResultSummary, producerInvoicedMaterialNetTonnage, defaultParams);
            var fee = _calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection;


            var calcTotal = _calcResult.CalcResultSummary!.ProducerDisposalFees!.First().TotalProducerBillBreakdownCosts!.TotalProducerFeeWithBadDebtProvision;
            var expectedLiabilityDiff = (Math.Round(calcTotal, 2) - Math.Round(20.00m, 2));

            // Assert
            Assert.AreEqual(20.00m, fee!.CurrentYearInvoiceTotalToDate);
            Assert.AreEqual(null, fee.TonnageChangeSinceLastInvoice);
            Assert.AreEqual(expectedLiabilityDiff, fee.LiabilityDifference);
            Assert.AreEqual("-", fee.MaterialThresholdBreached);
            Assert.AreEqual("-", fee.TonnageThresholdBreached);
            Assert.AreEqual("-", fee.PercentageLiabilityDifference);
            Assert.AreEqual("-", fee.MaterialPercentageThresholdBreached);
            Assert.AreEqual("-", fee.TonnagePercentageThresholdBreached);
            Assert.AreEqual("INITIAL", fee.SuggestedBillingInstruction);
            Assert.AreEqual("10491.167766844124", fee.SuggestedInvoiceAmount);
        }

        [TestMethod]
        public void CalculateLiabilityDifference_Level1_ComputesRoundedDifference()
        {
            var summary = new CalcResultSummary
            {
                ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
                                        {
                                            new()
                                            {
                                                ProducerId = "101",
                                                ProducerIdInt = 101,
                                                Level = "1",
                                                SubsidiaryId = "1000",
                                                ProducerName = "P1",
                                                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
                                                {
                                                    TotalProducerFeeWithBadDebtProvision = 120.004m
                                                }
                                            }
                                        }
            };

            var invoiced = new List<ProducerInvoicedDto>
                            {
                                new()
                                {
                                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 101 },
                                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction
                                    {
                                        ProducerId = 101,
                                        CurrentYearInvoicedTotalAfterThisRun = 20.003m
                                    }
                                }
                            };

            BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());

            var fee = summary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
            var expected = Math.Round(120.004m, 2) - Math.Round(20.003m, 2); 
            Assert.AreEqual(expected, fee.LiabilityDifference);
        }

        [TestMethod]
        public void CalculateLiabilityDifference_LevelNot1_ReturnsNull()
        {
            var summary = new CalcResultSummary
            {
                ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
                                        {
                                            new()
                                            {
                                                ProducerId = "301",
                                                ProducerIdInt = 301,
                                                Level = "2",
                                                SubsidiaryId = "3000",
                                                ProducerName = "P3",
                                                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
                                                {
                                                    TotalProducerFeeWithBadDebtProvision = 50m
                                                }
                                            }
                                        }
            };

            var invoiced = new List<ProducerInvoicedDto>
                            {
                                new()
                                {
                                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 301 },
                                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction
                                    {
                                        ProducerId = 301,
                                        CurrentYearInvoicedTotalAfterThisRun = 10m
                                    }
                                }
                            };

            BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());
            Assert.IsNull(summary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!.LiabilityDifference);
        }

        [TestMethod]
        public void GetLiabilityDifference_NonTotalsRow_PassesThroughCalculatedValue()
        {
            var summary = new CalcResultSummary
            {
                ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
                                        {
                                            new()
                                            {
                                                ProducerId = "11",
                                                ProducerIdInt = 11,
                                                Level = "1",
                                                SubsidiaryId = "S-11",
                                                ProducerName = "P11",
                                                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 20m }
                                            }
                                        }
            };

            var invoiced = new List<ProducerInvoicedDto>
                                {
                                    new()
                                    {
                                        InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 11 },
                                        InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction
                                        {
                                            ProducerId = 11,
                                            CurrentYearInvoicedTotalAfterThisRun = 5m
                                        }
                                    }
                                };

            BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());

            Assert.AreEqual(15m, summary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!.LiabilityDifference);
        }

        [TestMethod]
        public void GetLiabilityDifference_TotalsRowWithNonZeroRunningTotal_ReturnsSum()
        {
            var a = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                Level = "1",
                SubsidiaryId = "S-1",
                ProducerName = "P1",
                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 50m }
            }; 

            var b = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "2",
                ProducerIdInt = 2,
                Level = "1",
                SubsidiaryId = "S-2",
                ProducerName = "P2",
                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 70m }
            }; 

            var total = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = string.Empty,
                ProducerIdInt = 0,
                Level = string.Empty,
                IsProducerScaledup = CommonConstants.Totals,
                ProducerName = "Totals",
                SubsidiaryId = string.Empty
            };

            var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { a, b, total } };

            var invoiced = new List<ProducerInvoicedDto>
                            {
                                new()
                                {
                                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 1 },
                                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 1, CurrentYearInvoicedTotalAfterThisRun = 20m }
                                },
                                new()
                                {
                                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 2 },
                                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 2, CurrentYearInvoicedTotalAfterThisRun = 80m }
                                }
                            };

            BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());

            var fees = summary.ProducerDisposalFees.ToList();
            var totalsRow = fees[2].BillingInstructionSection!;
            var d1 = Math.Round(50m, 2) - Math.Round(20m, 2);
            var d2 = Math.Round(70m, 2) - Math.Round(80m, 2);
            Assert.AreEqual(d1 + d2, totalsRow.LiabilityDifference);
        }

        [TestMethod]
        public void GetLiabilityDifference_TotalsRowWithZeroRunningTotal_ReturnsNull()
        {
            var a = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                Level = "1",
                SubsidiaryId = "S-1",
                ProducerName = "P1",
                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 50m }
            }; 

            var b = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "2",
                ProducerIdInt = 2,
                Level = "1",
                SubsidiaryId = "S-2",
                ProducerName = "P2",
                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 20m }
            }; 

            var total = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = string.Empty,
                ProducerIdInt = 0,
                Level = string.Empty,
                IsProducerScaledup = CommonConstants.Totals,
                ProducerName = "Totals",
                SubsidiaryId = string.Empty
            };

            var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { a, b, total } };

            var invoiced = new List<ProducerInvoicedDto>
                            {
                                new()
                                {
                                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 1 },
                                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 1, CurrentYearInvoicedTotalAfterThisRun = 20m }
                                },
                                new()
                                {
                                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 2 },
                                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 2, CurrentYearInvoicedTotalAfterThisRun = 50m }
                                }
                            };

            BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());

            var totalsRow = summary.ProducerDisposalFees.ToList()[2].BillingInstructionSection!;
            Assert.IsNull(totalsRow.LiabilityDifference);
        }


        [TestMethod]
        public void GetMaterialThresholdBreached_TotalsRow_ReturnsEmpty()
        {
            var total = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = string.Empty,
                ProducerIdInt = 0,
                Level = string.Empty,
                IsProducerScaledup = CommonConstants.Totals,
                ProducerName = "Totals",
                SubsidiaryId = string.Empty
            };

            var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { total } };

            BillingInstructionsProducer.SetValues(summary, new List<ProducerInvoicedDto>(), new List<DefaultParamResultsClass>());

            Assert.AreEqual(string.Empty, summary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!.MaterialThresholdBreached);
        }

        [TestMethod]
        public void GetMaterialThresholdBreached_NonLevel1_ReturnsHyphen()
        {
            var fee = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                Level = "2",
                SubsidiaryId = "S-1",
                ProducerName = "P1",
                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 100m }
            };
            var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

            var invoiced = new List<ProducerInvoicedDto>
                            {
                                new()
                                {
                                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 1 },
                                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 1, CurrentYearInvoicedTotalAfterThisRun = 90m }
                                }
                            };

            BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());

            Assert.AreEqual("-", fee.BillingInstructionSection!.MaterialThresholdBreached);
        }

        [TestMethod]
        public void GetMaterialThresholdBreached_NoLiabilityDifference_ReturnsHyphen()
        {
            var fee = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                Level = "1",
                SubsidiaryId = "S-1",
                ProducerName = "P1"
            };
            var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

            BillingInstructionsProducer.SetValues(
                summary,
                new List<ProducerInvoicedDto>
                {
                new()
                {
                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 1 },
                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 1, CurrentYearInvoicedTotalAfterThisRun = 90m }
                }
                    },
                    new List<DefaultParamResultsClass>
                    {
                new()
                {
                    ParameterUniqueReference = "MATT-AI",
                    ParameterValue = 50m,
                    ParameterCategory = "Amount Increase",
                    ParameterType = "Materiality threshold"
                },
                new()
                {
                    ParameterUniqueReference = "MATT-AD",
                    ParameterValue = -50m,
                    ParameterCategory = "Amount Decrease",
                    ParameterType = "Materiality threshold"
                }
                    });

            Assert.AreEqual("-", fee.BillingInstructionSection!.MaterialThresholdBreached);
        }

        [TestMethod]
        public void GetMaterialThresholdBreached_ParamsMissing_ReturnsHyphen()
        {
            var fee = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                Level = "1",
                SubsidiaryId = "S-1",
                ProducerName = "P1",
                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 200m }
            };
            var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

            var invoiced = new List<ProducerInvoicedDto>
                            {
                                new()
                                {
                                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 1 },
                                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 1, CurrentYearInvoicedTotalAfterThisRun = 100m }
                                }
                            };

            BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>()); 
            Assert.AreEqual("-", fee.BillingInstructionSection!.MaterialThresholdBreached);
        }

        [TestMethod]
        public void GetMaterialThresholdBreached_DiffGreaterOrEqual_AI_ReturnsPosVe()
        {
            var fee = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                Level = "1",
                SubsidiaryId = "S-1",
                ProducerName = "P1",
                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 150m }
            };
            var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

            var invoiced = new List<ProducerInvoicedDto>
                            {
                                new()
                                {
                                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 1 },
                                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 1, CurrentYearInvoicedTotalAfterThisRun = 100m }
                                }
                            };

                                    var dp = new List<DefaultParamResultsClass>
                            {
                                new()
                                {
                                    ParameterUniqueReference = "MATT-AI",
                                    ParameterValue = 50m,
                                    ParameterCategory = "Amount Increase",
                                    ParameterType = "Materiality threshold"
                                },
                                new()
                                {
                                    ParameterUniqueReference = "MATT-AD",
                                    ParameterValue = -50m,
                                    ParameterCategory = "Amount Decrease",
                                    ParameterType = "Materiality threshold"
                                }
                            };

            BillingInstructionsProducer.SetValues(summary, invoiced, dp);
            Assert.AreEqual("+ve", fee.BillingInstructionSection!.MaterialThresholdBreached);
        }

        [TestMethod]
        public void GetMaterialThresholdBreached_DiffLessOrEqual_AD_ReturnsNegVe()
        {
            var fee = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                Level = "1",
                SubsidiaryId = "S-1",
                ProducerName = "P1",
                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 40m }
            };
            var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

            var invoiced = new List<ProducerInvoicedDto>
                                {
                                    new()
                                    {
                                        InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 1 },
                                        InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 1, CurrentYearInvoicedTotalAfterThisRun = 100m }
                                    }
                                };

                                        var dp = new List<DefaultParamResultsClass>
                                {
                                    new()
                                    {
                                        ParameterUniqueReference = "MATT-AI",
                                        ParameterValue = 50m,
                                        ParameterCategory = "Amount Increase",
                                        ParameterType = "Materiality threshold"
                                    },
                                    new()
                                    {
                                        ParameterUniqueReference = "MATT-AD",
                                        ParameterValue = -50m,
                                        ParameterCategory = "Amount Decrease",
                                        ParameterType = "Materiality threshold"
                                    }
                                };

            BillingInstructionsProducer.SetValues(summary, invoiced, dp);
            Assert.AreEqual("-ve", fee.BillingInstructionSection!.MaterialThresholdBreached);
        }

        [TestMethod]
        public void GetMaterialThresholdBreached_DiffBetweenThresholds_ReturnsHyphen()
        {
            var fee = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                Level = "1",
                SubsidiaryId = "S-1",
                ProducerName = "P1",
                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 115m }
            };
            var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

            var invoiced = new List<ProducerInvoicedDto>
                                    {
                                        new()
                                        {
                                            InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 1 },
                                            InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 1, CurrentYearInvoicedTotalAfterThisRun = 100m }
                                        }
                                    };

                                            var dp = new List<DefaultParamResultsClass>
                                    {
                                        new()
                                        {
                                            ParameterUniqueReference = "MATT-AI",
                                            ParameterValue = 50m,
                                            ParameterCategory = "Amount Increase",
                                            ParameterType = "Materiality threshold"
                                        },
                                        new()
                                        {
                                            ParameterUniqueReference = "MATT-AD",
                                            ParameterValue = -50m,
                                            ParameterType = "Materiality threshold",
                                            ParameterCategory = "Amount Decrease"
                                        }
                                    };

            BillingInstructionsProducer.SetValues(summary, invoiced, dp);
            Assert.AreEqual("-", fee.BillingInstructionSection!.MaterialThresholdBreached);
        }

        [TestMethod]
        public void GetTonnageThresholdBreached_NoChange_ReturnsHyphen()
        {
            var fee = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                Level = "1",
                SubsidiaryId = "S-1",
                ProducerName = "P1",
                TonnageChangeAdvice = "NOCHANGE",
                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 200m }
            };
            var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

            var invoiced = new List<ProducerInvoicedDto>
                                {
                                    new()
                                    {
                                        InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 1 },
                                        InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 1, CurrentYearInvoicedTotalAfterThisRun = 100m }
                                    }
                                };

            var dp = new List<DefaultParamResultsClass>
                        {
                            new()
                            {
                                ParameterUniqueReference = "TONT-AI",
                                ParameterValue = 50m,
                                ParameterCategory = "Amount Increase",
                                ParameterType = "Tonnage change threshold"
                            },
                            new()
                            {
                                ParameterUniqueReference = "TONT-AD",
                                ParameterValue = -50m,
                                ParameterCategory = "Amount Decrease",
                                ParameterType = "Tonnage change threshold"
                            }
                        };

            BillingInstructionsProducer.SetValues(summary, invoiced, dp);
            Assert.AreEqual("-", fee.BillingInstructionSection!.TonnageThresholdBreached);
        }

        [TestMethod]
        public void GetTonnageThresholdBreached_Change_ParamsMissing_ReturnsHyphen()
        {
            var fee = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                Level = "1",
                SubsidiaryId = "S-1",
                ProducerName = "P1",
                TonnageChangeAdvice = "CHANGE",
                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 200m }
            };
            var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

            var invoiced = new List<ProducerInvoicedDto>
                            {
                                new()
                                {
                                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 1 },
                                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 1, CurrentYearInvoicedTotalAfterThisRun = 100m }
                                }
                            };

            BillingInstructionsProducer.SetValues(summary, invoiced, new List<DefaultParamResultsClass>());
            Assert.AreEqual("-", fee.BillingInstructionSection!.TonnageThresholdBreached);
        }

        [TestMethod]
        public void GetTonnageThresholdBreached_Change_DiffGreaterOrEqual_AI_ReturnsPosVe()
        {
            var fee = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                Level = "1",
                SubsidiaryId = "S-1",
                ProducerName = "P1",
                TonnageChangeAdvice = "CHANGE",
                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 160m }
            };
            var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

            var invoiced = new List<ProducerInvoicedDto>
                            {
                                new()
                                {
                                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 1 },
                                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 1, CurrentYearInvoicedTotalAfterThisRun = 100m }
                                }
                            };

                             var dp = new List<DefaultParamResultsClass>
                            {
                                new()
                                {
                                    ParameterUniqueReference = "TONT-AI",
                                    ParameterValue = 50m,
                                    ParameterCategory = "Amount Increase",
                                    ParameterType = "Tonnage change threshold"
                                },
                                new()
                                {
                                    ParameterUniqueReference = "TONT-AD",
                                    ParameterValue = -50m,
                                    ParameterCategory = "Amount Decrease",
                                    ParameterType = "Tonnage change threshold"
                                }
                            };

            BillingInstructionsProducer.SetValues(summary, invoiced, dp);
            Assert.AreEqual("+ve", fee.BillingInstructionSection!.TonnageThresholdBreached);
        }

        [TestMethod]
        public void GetTonnageThresholdBreached_Change_DiffLessOrEqual_AD_ReturnsNegVe()
        {
            var fee = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                Level = "1",
                SubsidiaryId = "S-1",
                ProducerName = "P1",
                TonnageChangeAdvice = "CHANGE",
                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 40m }
            };
            var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

            var invoiced = new List<ProducerInvoicedDto>
                            {
                                new()
                                {
                                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 1 },
                                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 1, CurrentYearInvoicedTotalAfterThisRun = 100m }
                                }
                            };

                                    var dp = new List<DefaultParamResultsClass>
                            {
                                new()
                                {
                                    ParameterUniqueReference = "TONT-AI",
                                    ParameterValue = 50m,
                                    ParameterCategory = "Amount Increase",
                                    ParameterType = "Tonnage change threshold"
                                },
                                new()
                                {
                                    ParameterUniqueReference = "TONT-AD",
                                    ParameterValue = -50m,
                                    ParameterCategory = "Amount Decrease",
                                    ParameterType = "Tonnage change threshold"
                                }
                            };

            BillingInstructionsProducer.SetValues(summary, invoiced, dp);
            Assert.AreEqual("-ve", fee.BillingInstructionSection!.TonnageThresholdBreached);
        }

        [TestMethod]
        public void GetTonnageThresholdBreached_Change_DiffBetweenThresholds_ReturnsHyphen()
        {
            var fee = new CalcResultSummaryProducerDisposalFees
            {
                ProducerId = "1",
                ProducerIdInt = 1,
                Level = "1",
                SubsidiaryId = "S-1",
                ProducerName = "P1",
                TonnageChangeAdvice = "CHANGE",
                TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { TotalProducerFeeWithBadDebtProvision = 110m }
            };
            var summary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee } };

            var invoiced = new List<ProducerInvoicedDto>
                            {
                                new()
                                {
                                    InvoicedTonnage = new ProducerInvoicedMaterialNetTonnage { ProducerId = 1 },
                                    InvoiceInstruction = new ProducerDesignatedRunInvoiceInstruction { ProducerId = 1, CurrentYearInvoicedTotalAfterThisRun = 100m }
                                }
                            };

                                    var dp = new List<DefaultParamResultsClass>
                            {
                                new()
                                {
                                    ParameterUniqueReference = "TONT-AI",
                                    ParameterValue = 50m,
                                    ParameterCategory = "Amount Increase",
                                    ParameterType = "Tonnage change threshold"
                                },
                                new()
                                {
                                    ParameterUniqueReference = "TONT-AD",
                                    ParameterValue = -50m,
                                    ParameterCategory = "Amount Decrease",
                                    ParameterType = "Tonnage change threshold"
                                }
                            };

            BillingInstructionsProducer.SetValues(summary, invoiced, dp);
            Assert.AreEqual("-", fee.BillingInstructionSection!.TonnageThresholdBreached);
        }

        /// <summary>
        /// The CreateMaterials
        /// </summary>
        private void CreateMaterials()
        {
            var materialDictionary = new Dictionary<string, string>();
            materialDictionary.Add("AL", "Aluminium");
            materialDictionary.Add("FC", "Fibre composite");
            materialDictionary.Add("GL", "Glass");
            materialDictionary.Add("PC", "Paper or card");
            materialDictionary.Add("PL", "Plastic");
            materialDictionary.Add("ST", "Steel");
            materialDictionary.Add("WD", "Wood");
            materialDictionary.Add("OT", "Other materials");

            foreach (var materialKv in materialDictionary)
            {
                _dbContext.Material.Add(new Material
                {
                    Name = materialKv.Value,
                    Code = materialKv.Key,
                    Description = "Some",
                });
            }

            _dbContext.SaveChanges();
        }

        /// <summary>
        /// The CreateProducerDetail
        /// </summary>
        private void CreateProducerDetail()
        {
            var producerNames = new string[]
            {
                "Allied Packaging",
                "Beeline Materials",
                "Cloud Boxes",
                "Decking and Shed",
                "Electric Things",
                "French Flooring",
                "Good Fruit Co",
                "Happy Shopper",
                "Icicle Foods",
                "Jumbo Box Store",
            };

            var producerId = 1;
            foreach (var producerName in producerNames)
            {
                _dbContext.ProducerDetail.Add(new ProducerDetail
                {
                    ProducerId = producerId++,
                    SubsidiaryId = $"{producerId}-Sub",
                    ProducerName = producerName,
                    CalculatorRunId = 1,
                });
            }

            _dbContext.SaveChanges();

            for (int producerDetailId = 1; producerDetailId <= 10; producerDetailId++)
            {
                for (int materialId = 1; materialId < 9; materialId++)
                {
                    _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        PackagingType = "HH",
                        PackagingTonnage = materialId * 100,
                    });
                    _dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        PackagingType = "CW",
                        PackagingTonnage = materialId * 50,
                    });
                }
            }

            _dbContext.SaveChanges();
        }
    }
}
