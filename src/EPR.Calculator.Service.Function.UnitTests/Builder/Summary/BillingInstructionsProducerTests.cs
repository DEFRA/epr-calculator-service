using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.BillingRun.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class BillingInstructionsProducerTests
{
    private readonly CalcResult calcResult = TestDataHelper.GetCalcResult();

    private readonly InvoicedProducer defaultInvoicedProducer = new()
    {
        CalculatorRunId = 0,
        CalculatorName = "ignored",
        ProducerId = 1,
        ProducerName = "ignored",
        TradingName = null,
        MaterialId = 1,
        BillingInstructionId = null,
        InvoicedNetTonnage = null,
        CurrentYearInvoicedTotalAfterThisRun = 20.003m
    };

    [TestMethod]
    public void BillingInstructionsProducer_CanCallSetValues()
    {
        // Act
        List<InvoicedProducer> producerInvoicedMaterialNetTonnage =
        [
            new()
            {
                CalculatorRunId = 101,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 77,
                BillingInstructionId = null,
                InvoicedNetTonnage = 20,
                CurrentYearInvoicedTotalAfterThisRun = 20.00m
            }
        ];

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 99999m, Percentage = 99999m },
            MaterialityDecrease = new Materiality { Amount = -99999m, Percentage = 55000m }
        };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, producerInvoicedMaterialNetTonnage, otherCost);
        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection;

        var calcTotal = calcResult.CalcResultSummary.ProducerDisposalFees.First().TotalProducerBillBreakdownCosts!.FeeWithBadDebtProvision.Total;
        var expectedLiabilityDiff = Math.Round(calcTotal, 2) - Math.Round(20.00m, 2);

        // Assert
        Assert.AreEqual(20.00m, fee!.CurrentYearInvoiceTotalToDate);
        Assert.AreEqual(null, fee.TonnageChangeSinceLastInvoice);
        Assert.AreEqual(expectedLiabilityDiff, fee.LiabilityDifference);
        Assert.AreEqual("-", fee.MaterialThresholdBreached);
        Assert.AreEqual("-", fee.TonnageThresholdBreached);
        Assert.AreEqual(52355.85m, fee.PercentageLiabilityDifference);
        Assert.AreEqual("-ve", fee.MaterialPercentageThresholdBreached);
        Assert.AreEqual("-", fee.TonnagePercentageThresholdBreached);
        Assert.AreEqual(BillingConstants.Suggestion.Delta, fee.SuggestedBillingInstruction);
        Assert.AreEqual(10471.17m, fee.SuggestedInvoiceAmount);
    }

    [TestMethod]
    public void CalculateLiabilityDifference_Level1_ComputesRoundedDifference()
    {
        var summary = new CalcResultSummary
        {
            OverallTotal = TestDataHelper.GetOverallTotalRow(),
            ProducerDisposalFees =
            [
                new CalcResultSummaryProducerDisposalFees
                {
                    CalculatorRunId = 0,
                    ProducerId = 101,
                    Level = "1",
                    SubsidiaryId = "1000",
                    ProducerName = "P1",
                    TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
                    {
                        FeeWithBadDebtProvision = new ByCountryCost { England = 120.004m, Wales = 0, Scotland = 0, NorthernIreland = 0 }
                    }
                }
            ]
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 101,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 20.003m
            }
        ];

        BillingInstructionsProducer.SetValues(summary, invoiced, new CalcResultParameterOtherCost());

        var fee = summary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        var expected = Math.Round(120.004m, 2) - Math.Round(20.003m, 2);
        Assert.AreEqual(expected, fee.LiabilityDifference);
    }

    [TestMethod]
    public void CalculateLiabilityDifference_LevelNot1_ReturnsNull()
    {
        var summary = new CalcResultSummary
        {
            OverallTotal = TestDataHelper.GetOverallTotalRow(),
            ProducerDisposalFees = (List<CalcResultSummaryProducerDisposalFees>)
            [
                new CalcResultSummaryProducerDisposalFees
                {
                    CalculatorRunId = 0,
                    ProducerId = 301,
                    Level = "2",
                    SubsidiaryId = "3000",
                    ProducerName = "P3",
                    TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
                    {
                        FeeWithBadDebtProvision = new ByCountryCost { England = 50m, Wales = 0, Scotland = 0, NorthernIreland = 0 }
                    }
                }
            ]
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 301,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 10m
            }
        ];

        BillingInstructionsProducer.SetValues(summary, invoiced, new CalcResultParameterOtherCost());
        Assert.IsNull(summary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!.LiabilityDifference);
    }

    [TestMethod]
    public void GetLiabilityDifference_NonTotalsRow_PassesThroughCalculatedValue()
    {
        var summary = new CalcResultSummary
        {
            OverallTotal = TestDataHelper.GetOverallTotalRow(),
            ProducerDisposalFees = (List<CalcResultSummaryProducerDisposalFees>)
            [
                new CalcResultSummaryProducerDisposalFees
                {
                    CalculatorRunId = 0,
                    ProducerId = 11,
                    Level = "1",
                    SubsidiaryId = "S-11",
                    ProducerName = "P11",
                    TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 20m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
                }
            ]
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 11,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 5m
            }
        ];

        BillingInstructionsProducer.SetValues(summary, invoiced, new CalcResultParameterOtherCost());

        Assert.AreEqual(15m, summary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!.LiabilityDifference);
    }

    [TestMethod]
    public void GetLiabilityDifference_TotalsRowWithNonZeroRunningTotal_ReturnsSum()
    {
        var a = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 50m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
        };

        var b = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 2,
            Level = "1",
            SubsidiaryId = "S-2",
            ProducerName = "P2",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 70m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
        };

        var total = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 0,
            Level = string.Empty,
            ProducerName = "Totals",
            SubsidiaryId = string.Empty
        };

        var summary = new CalcResultSummary
        {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { a, b },
            OverallTotal = total
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 20m
            },
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 2,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 80m
            }
        ];

        BillingInstructionsProducer.SetValues(summary, invoiced, new CalcResultParameterOtherCost());

        var d1 = Math.Round(50m, 2) - Math.Round(20m, 2);
        var d2 = Math.Round(70m, 2) - Math.Round(80m, 2);
        Assert.AreEqual(d1 + d2, summary.OverallTotal!.BillingInstructionSection!.LiabilityDifference);
    }

    [TestMethod]
    public void GetLiabilityDifference_TotalsRowWithZeroRunningTotal_ReturnsNull()
    {
        var a = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 50m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
        };

        var b = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 2,
            Level = "1",
            SubsidiaryId = "S-2",
            ProducerName = "P2",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 20m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
        };

        var total = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 0,
            Level = string.Empty,
            ProducerName = "Totals",
            SubsidiaryId = string.Empty
        };

        var summary = new CalcResultSummary
        {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { a, b },
            OverallTotal = total
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 20m
            },
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 2,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 50m
            }
        ];

        BillingInstructionsProducer.SetValues(summary, invoiced, new CalcResultParameterOtherCost());

        Assert.IsNull(summary.OverallTotal!.BillingInstructionSection!.LiabilityDifference);
    }


    [TestMethod]
    public void GetMaterialThresholdBreached_TotalsRow_ReturnsEmpty()
    {
        var total = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 0,
            Level = string.Empty,
            ProducerName = "Totals",
            SubsidiaryId = string.Empty
        };

        var summary = new CalcResultSummary
        {
            OverallTotal = total
        };

        BillingInstructionsProducer.SetValues(summary, new List<InvoicedProducer>(), new CalcResultParameterOtherCost());

        Assert.AreEqual(string.Empty, summary.OverallTotal!.BillingInstructionSection!.MaterialThresholdBreached);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_NonLevel1_ReturnsHyphen()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 1,
            Level = "2",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 100m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
        };
        var summary = new CalcResultSummary {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee },
            OverallTotal = TestDataHelper.GetOverallTotalRow()
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 90m
            }
        ];

        BillingInstructionsProducer.SetValues(summary, invoiced, new CalcResultParameterOtherCost());

        Assert.AreEqual("-", fee.BillingInstructionSection!.MaterialThresholdBreached);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_NoLiabilityDifference_ReturnsHyphen()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
            {
                FeeWithBadDebtProvision = new ByCountryCost { England = 90m, Wales = 0, Scotland = 0, NorthernIreland = 0 }
            }
        };
        var summary = new CalcResultSummary {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee },
            OverallTotal = TestDataHelper.GetOverallTotalRow()
        };

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            MaterialityDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(
            summary,
            [
                new InvoicedProducer
                {
                    CalculatorRunId = 0,
                    CalculatorName = "ignored",
                    ProducerId = 1,
                    ProducerName = "ignored",
                    TradingName = null,
                    MaterialId = 0,
                    BillingInstructionId = null,
                    InvoicedNetTonnage = null,
                    CurrentYearInvoicedTotalAfterThisRun = 90m
                }
            ],
            otherCost);

        Assert.AreEqual("-", fee.BillingInstructionSection!.MaterialThresholdBreached);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_DiffWithinThresholds_ReturnsHyphen()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 200m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
        };
        var summary = new CalcResultSummary {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee },
            OverallTotal = TestDataHelper.GetOverallTotalRow()
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 200m, Percentage = 0 },
            MaterialityDecrease = new Materiality { Amount = -200m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, otherCost);
        Assert.AreEqual("-", fee.BillingInstructionSection!.MaterialThresholdBreached);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_DiffGreaterOrEqual_AI_ReturnsPosVe()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 150m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
        };
        var summary = new CalcResultSummary {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee },
            OverallTotal = TestDataHelper.GetOverallTotalRow()
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            MaterialityDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, otherCost);
        Assert.AreEqual("+ve", fee.BillingInstructionSection!.MaterialThresholdBreached);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_DiffLessOrEqual_AD_ReturnsNegVe()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 40m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
        };
        var summary = new CalcResultSummary {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee },
            OverallTotal = TestDataHelper.GetOverallTotalRow()
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            MaterialityDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, otherCost);
        Assert.AreEqual("-ve", fee.BillingInstructionSection!.MaterialThresholdBreached);
    }

    [TestMethod]
    public void GetMaterialThresholdBreached_DiffBetweenThresholds_ReturnsHyphen()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 115m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
        };
        var summary = new CalcResultSummary {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee },
            OverallTotal = TestDataHelper.GetOverallTotalRow()
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            MaterialityDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, otherCost);
        Assert.AreEqual("-", fee.BillingInstructionSection!.MaterialThresholdBreached);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_NoChange_ReturnsHyphen()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TonnageChangeAdvice = "NOCHANGE",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 200m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
        };
        var summary = new CalcResultSummary {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee },
            OverallTotal = TestDataHelper.GetOverallTotalRow()
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            TonnageChangeDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, otherCost);
        Assert.AreEqual("-", fee.BillingInstructionSection!.TonnageThresholdBreached);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_Change_DiffWithinThresholds_ReturnsHyphen()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TonnageChangeAdvice = "CHANGE",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 200m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
        };
        var summary = new CalcResultSummary {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee },
            OverallTotal = TestDataHelper.GetOverallTotalRow()
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 200m, Percentage = 0 },
            TonnageChangeDecrease = new Materiality { Amount = -200m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, otherCost);
        Assert.AreEqual("-", fee.BillingInstructionSection!.TonnageThresholdBreached);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_Change_DiffGreaterOrEqual_AI_ReturnsPosVe()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TonnageChangeAdvice = "CHANGE",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 160m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
        };
        var summary = new CalcResultSummary {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee },
            OverallTotal = TestDataHelper.GetOverallTotalRow()
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            TonnageChangeDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, otherCost);
        Assert.AreEqual("+ve", fee.BillingInstructionSection!.TonnageThresholdBreached);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_Change_DiffLessOrEqual_AD_ReturnsNegVe()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TonnageChangeAdvice = "CHANGE",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 40m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
        };
        var summary = new CalcResultSummary {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee },
            OverallTotal = TestDataHelper.GetOverallTotalRow()
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            TonnageChangeDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, otherCost);
        Assert.AreEqual("-ve", fee.BillingInstructionSection!.TonnageThresholdBreached);
    }

    [TestMethod]
    public void GetTonnageThresholdBreached_Change_DiffBetweenThresholds_ReturnsHyphen()
    {
        var fee = new CalcResultSummaryProducerDisposalFees
        {
            CalculatorRunId = 0,
            ProducerId = 1,
            Level = "1",
            SubsidiaryId = "S-1",
            ProducerName = "P1",
            TonnageChangeAdvice = "CHANGE",
            TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision { FeeWithBadDebtProvision = new ByCountryCost { England = 110m, Wales = 0, Scotland = 0, NorthernIreland = 0 } }
        };
        var summary = new CalcResultSummary {
            ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> { fee },
            OverallTotal = TestDataHelper.GetOverallTotalRow()
        };

        List<InvoicedProducer> invoiced =
        [
            new()
            {
                CalculatorRunId = 0,
                CalculatorName = "ignored",
                ProducerId = 1,
                ProducerName = "ignored",
                TradingName = null,
                MaterialId = 0,
                BillingInstructionId = null,
                InvoicedNetTonnage = null,
                CurrentYearInvoicedTotalAfterThisRun = 100m
            }
        ];

        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 50m, Percentage = 0 },
            TonnageChangeDecrease = new Materiality { Amount = -50m, Percentage = 0 }
        };

        BillingInstructionsProducer.SetValues(summary, invoiced, otherCost);
        Assert.AreEqual("-", fee.BillingInstructionSection!.TonnageThresholdBreached);
    }

    [TestMethod]
    public void CalculatePercentageLiabilityDifference_LevelNot1_ReturnsNull()
    {
        calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.IsNull(fee.PercentageLiabilityDifference);
    }

    [TestMethod]
    public void CalculatePercentageLiabilityDifference_Level1_ComputesRoundedDifference()
    {
        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(52348.00m, fee.PercentageLiabilityDifference);
    }

    [TestMethod]
    public void CalculatePercentageLiabilityDifference_Total_ReturnsNull()
    {
        var summary = TestDataHelper.GetCalcResultSummary();

        BillingInstructionsProducer.SetValues(summary, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        Assert.IsNull(summary.OverallTotal!.BillingInstructionSection!.PercentageLiabilityDifference);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_LevelNot1_ReturnsHypen()
    {
        calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.MaterialPercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_Level1_ReturnsPositive()
    {
        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 0, Percentage = 50m }
        };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], otherCost);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Positive, fee.MaterialPercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_Level1_ReturnsNegative()
    {
        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 0, Percentage = 99999m },
            MaterialityDecrease = new Materiality { Amount = 0, Percentage = 55000m }
        };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], otherCost);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Negative, fee.MaterialPercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateMaterialPercentageThresholdBreached_Level1_ReturnsHypen()
    {
        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 0, Percentage = 99999m },
            MaterialityDecrease = new Materiality { Amount = 0, Percentage = -99999m }
        };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], otherCost);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.MaterialPercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_LevelNot1_ReturnsHypen()
    {
        calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_WhenTonnageChangeIsNull_ReturnsHypen()
    {
        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 0, Percentage = 50m }
        };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], otherCost);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_Level1_ReturnsPositive()
    {
        calcResult.CalcResultSummary.ProducerDisposalFees.First().TonnageChangeAdvice = "CHANGE";

        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 0, Percentage = 50m }
        };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], otherCost);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Positive, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_Level1_ReturnsNegative()
    {
        calcResult.CalcResultSummary.ProducerDisposalFees.First().TonnageChangeAdvice = "CHANGE";

        var otherCost = new CalcResultParameterOtherCost
        {
            TonnageChangeIncrease = new Materiality { Amount = 0, Percentage = 99999m },
            TonnageChangeDecrease = new Materiality { Amount = 0, Percentage = 55000m }
        };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], otherCost);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Negative, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateTonnagePercentageThresholdBreached_Level1_ReturnsHypen()
    {
        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.TonnagePercentageThresholdBreached);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_NotLevel1_ReturnsHypen()
    {
        calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_Level1_ReturnsDelta()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 100m };

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 0, Percentage = 99999m },
            MaterialityDecrease = new Materiality { Amount = 0, Percentage = 55000m }
        };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [invoicedProducer], otherCost);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(BillingConstants.Suggestion.Delta, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_Level1_ReturnsRebill()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 15000m };

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 0, Percentage = 99999m },
            MaterialityDecrease = new Materiality { Amount = 0, Percentage = 55000m }
        };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [invoicedProducer], otherCost);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(BillingConstants.Suggestion.Rebill, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateSuggestedBillingInstruction_Level1_ReturnsHypen()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 10491.17m };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [invoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(CommonConstants.Hyphen, fee.SuggestedBillingInstruction);
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_NotLevel1_ReturnsHypen()
    {
        calcResult.CalcResultSummary.ProducerDisposalFees.First().Level = "2";

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [defaultInvoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.IsNull(fee.SuggestedInvoiceAmount);
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_Level1_ReturnsLiabilityDifference()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 100m };

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 0, Percentage = 99999m },
            MaterialityDecrease = new Materiality { Amount = 0, Percentage = 55000m }
        };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [invoicedProducer], otherCost);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(fee.LiabilityDifference, fee.SuggestedInvoiceAmount);
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_Level1_ReturnsTotalProducerFeeWithBadDebtProvision()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 15000m };

        var otherCost = new CalcResultParameterOtherCost
        {
            MaterialityIncrease = new Materiality { Amount = 0, Percentage = 99999m },
            MaterialityDecrease = new Materiality { Amount = 0, Percentage = 55000m }
        };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [invoicedProducer], otherCost);

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.AreEqual(10491.17m, Math.Round(fee.SuggestedInvoiceAmount ?? 0m, 2));
    }

    [TestMethod]
    public void CalculateGetSuggestedInvoiceAmount_Level1_ReturnsHypen()
    {
        var invoicedProducer = defaultInvoicedProducer with { CurrentYearInvoicedTotalAfterThisRun = 10491.17m };

        BillingInstructionsProducer.SetValues(calcResult.CalcResultSummary, [invoicedProducer], new CalcResultParameterOtherCost());

        var fee = calcResult.CalcResultSummary.ProducerDisposalFees.ToList()[0].BillingInstructionSection!;
        Assert.IsNull(fee.SuggestedInvoiceAmount);
    }
}
