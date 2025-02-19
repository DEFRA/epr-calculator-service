﻿using EPR.Calculator.Service.Function.Builder.Summary.ThreeSa;

namespace EPR.Calculator.Service.Function.UnitTests.Builder.Summary.ThreeSa
{
    using EPR.Calculator.Service.Function.Builder.Summary.ThreeSA;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Models;

    [TestClass]
    public class ThreeSaCostsSummaryTests
    {
        private CalcResult? _calcResult;

        [TestInitialize]
        public void TestInitialize()
        {
            _calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtProvision = new KeyValuePair<string, string>("key1",
                        "6%"),
                    BadDebtValue = 6m,
                    Details =
                    [
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = "4 LA Data Prep Charge",
                            OrderId = 1,
                            England = "£40.00",
                            EnglandValue = 40,
                            Wales = "£30.00",
                            WalesValue = 30,
                            Scotland = "£20.00",
                            ScotlandValue = 20,
                            NorthernIreland = "£10.00",
                            NorthernIrelandValue = 10,
                            Total = "£100.00",
                            TotalValue = 100
                        }
                    ],
                    Materiality =
                    [
                        new CalcResultMateriality
                        {
                            Amount = "Amount £s",
                            AmountValue = 0,
                            Percentage = "%",
                            PercentageValue = 0,
                            SevenMateriality = "7 Materiality"
                        }
                    ],
                    Name = "Parameters - Other",
                    SaOperatingCost =
                    [
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = string.Empty,
                            OrderId = 1,
                            England = "£40.00",
                            EnglandValue = 40,
                            Wales = "£30.00",
                            WalesValue = 30,
                            Scotland = "£20.00",
                            ScotlandValue = 20,
                            NorthernIreland = "£10.00",
                            NorthernIrelandValue = 10,
                            Total = "£100.00",
                            TotalValue = 100
                        }
                    ],
                    SchemeSetupCost =
                    {
                        Name = "5 Scheme set up cost Yearly Cost",
                        OrderId = 1,
                        England = "£40.00",
                        EnglandValue = 40,
                        Wales = "£30.00",
                        WalesValue = 30,
                        Scotland = "£20.00",
                        ScotlandValue = 20,
                        NorthernIreland = "£10.00",
                        NorthernIrelandValue = 10,
                        Total = "£100.00",
                        TotalValue = 100
                    }
                },
                CalcResultDetail = new CalcResultDetail()
                {
                },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
                {
                    CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>()
                    {
                        new CalcResultLaDisposalCostDataDetail
                        {
                            DisposalCostPricePerTonne = "20",
                            England = "EnglandTest",
                            Wales = "WalesTest",
                            Name = "ScotlandTest",
                            Scotland = "ScotlandTest",
                            Material = "Material1",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = string.Empty,
                            ProducerReportedHouseholdPackagingWasteTonnage = string.Empty,
                            ReportedPublicBinTonnage = string.Empty
                        },
                        new CalcResultLaDisposalCostDataDetail
                        {
                            DisposalCostPricePerTonne = "20",
                            England = "EnglandTest",
                            Wales = "WalesTest",
                            Name = "Material1",
                            Scotland = "ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = string.Empty,
                            ProducerReportedHouseholdPackagingWasteTonnage = string.Empty,
                            ReportedPublicBinTonnage = string.Empty
                        },
                        new CalcResultLaDisposalCostDataDetail
                        {
                            DisposalCostPricePerTonne = "10",
                            England = "EnglandTest",
                            Wales = "WalesTest",
                            Name = "Material2",
                            Scotland = "ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Total = string.Empty,
                            ProducerReportedHouseholdPackagingWasteTonnage = string.Empty,
                            ReportedPublicBinTonnage = string.Empty
                        }
                    },
                    Name = "some test"
                },
                CalcResultLapcapData = new CalcResultLapcapData()
                {
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>()
                    {
                    }
                },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
                {
                    CalcResultOnePlusFourApportionmentDetails =
                    [
                        new()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 0.10M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 020M,
                            Name = "Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 0.10M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 020M,
                            Name = "Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 0.10M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 020M,
                            Name = "Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 14.53M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 020M,
                            Name = "Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 14.53M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 0.20M,
                            Name = "Test",
                            OrderId = 4
                        }
                    ],
                    Name = "some test"
                },
                CalcResultParameterCommunicationCost = new CalcResultParameterCommunicationCost
                {
                    Name = "some test"
                },
                CalcResultSummary = new CalcResultSummary
                {
                    ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>()
                    {
                        new()
                        {
                            ProducerCommsFeesByMaterial =
                                new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>()
                                {
                                },
                            ProducerDisposalFeesByMaterial =
                                new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>()
                                {
                                },
                            ProducerId = "1",
                            ProducerName = "Test",
                            TotalProducerDisposalFeeWithBadDebtProvision = 100,
                            TotalProducerCommsFeeWithBadDebtProvision = 100,
                            SubsidiaryId = "1",

                        }
                    }
                },
                CalcResultCommsCostReportDetail = new CalcResultCommsCost()
                {
                    CalcResultCommsCostCommsCostByMaterial =
                    [
                        new()
                        {
                            CommsCostByMaterialPricePerTonne = "0.42",
                            Name = "Material1",

                        },
                        new()
                        {
                            CommsCostByMaterialPricePerTonne = "0.3",
                            Name = "Material2",

                        }
                    ]
                },
                CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage()
                {
                    Name = string.Empty,
                    CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>(),
                    MaterialHeading = string.Empty,
                    TonnageHeading = string.Empty
                },
            };
        }

        [TestMethod]
        public void CanCallGetHeaders()
        {
            // Act
            var result = ThreeSaCostsSummary.GetHeaders().ToList();
            var columnIndex = 245;

            var expectedResult = new List<CalcResultSummaryHeader>();
            expectedResult.AddRange([
                new CalcResultSummaryHeader { Name = $"{ThreeSaCostHeader.SaOperatingCostsWithoutBadDebtProvisionTitleSection3}", ColumnIndex = columnIndex },
                new CalcResultSummaryHeader { Name = $"{ThreeSaCostHeader.BadDebtProvisionTitleSection3}", ColumnIndex = columnIndex+1 },
                new CalcResultSummaryHeader { Name = $"{ThreeSaCostHeader.SaOperatingCostsWithBadDebtProvisionTitleSection3}", ColumnIndex = columnIndex+2 }
            ]);

            // Assert
            Assert.AreEqual(expectedResult[0].Name, result[0].Name);
            Assert.AreEqual(expectedResult[0].ColumnIndex, result[0].ColumnIndex);
            Assert.AreEqual(expectedResult[1].Name, result[1].Name);
            Assert.AreEqual(expectedResult[1].ColumnIndex, result[1].ColumnIndex);
            Assert.AreEqual(expectedResult[2].Name, result[2].Name);
            Assert.AreEqual(expectedResult[2].ColumnIndex, result[2].ColumnIndex);
        }

        [TestMethod]
        public void CanCallGetSaSetupCostsWithoutBadDebtProvision()
        {
            // Act
            if (_calcResult != null)
            {
                var result = ThreeSaCostsSummary.GetThreeSaCostsWithoutBadDebtProvision(_calcResult);

                // Assert
                Assert.AreEqual(100, result);
            }
        }

        [TestMethod]
        public void CanCallGetBadDebtProvision()
        {
            // Act
            if (_calcResult != null)
            {
                var result = ThreeSaCostsSummary.GetSetUpBadDebtProvision(_calcResult);

                // Assert
                Assert.AreEqual(6, result);
            }
        }

        [TestMethod]
        public void CanCallGetSaSetupCostsWithBadDebtProvision()
        {
            // Act
            if (_calcResult != null)
            {
                var result = ThreeSaCostsSummary.GetThreeSaCostsWithBadDebtProvision(_calcResult);

                // Assert
                Assert.AreEqual(106, result);
            }
        }
    }
}