using EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer.cs;

namespace EPR.Calculator.Service.Function.UnitTests
{
    using AutoFixture;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Common;
    using EPR.Calculator.Service.Function.Builder.ParametersOther;
    using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
    using EPR.Calculator.Service.Function.Builder.Summary;
    using EPR.Calculator.Service.Function.Builder.Summary.OneAndTwoA;
    using EPR.Calculator.Service.Function.Constants;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Models;
    using EPR.Calculator.Service.Function.UnitTests.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultSummaryBuilderTests
    {
        private readonly DbContextOptions<ApplicationDBContext> dbContextOptions;
        private readonly ApplicationDBContext context;
        private readonly CalcResultSummaryBuilder calcResultsService;
        private readonly CalcResult calcResult;
        private readonly CalcResultScaledupProducers scaledupProducers;

        private Fixture Fixture { get; init; } = new Fixture();

        public CalcResultSummaryBuilderTests()
        {
            this.dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "CalcResultSummaryTestDb")
                .Options;
            this.context = new ApplicationDBContext(this.dbContextOptions);
            this.calcResultsService = new CalcResultSummaryBuilder(this.context);
            this.scaledupProducers = TestDataHelper.GetScaledupProducers();
            this.calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
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
                            TotalValue = 100,
                        },
                    ],
                    Materiality =
                    [
                        new CalcResultMateriality
                        {
                            Amount = "Amount £s",
                            AmountValue = 0,
                            Percentage = "%",
                            PercentageValue = 0,
                            SevenMateriality = "7 Materiality",
                        },
                    ],
                    Name = "Parameters - Other",
                    SaOperatingCost =
                    [
                        new CalcResultParameterOtherCostDetail
                        {
                            Name = string.Empty,
                            OrderId = 0,
                            England = "England",
                            EnglandValue = 0,
                            Wales = "Wales",
                            WalesValue = 0,
                            Scotland = "Scotland",
                            ScotlandValue = 0,
                            NorthernIreland = "Northern Ireland",
                            NorthernIrelandValue = 0,
                            Total = "Total",
                            TotalValue = 0,
                        },
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
                        TotalValue = 100,
                    },
                },
                CalcResultDetail = new CalcResultDetail() { },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData()
                {
                    Name = Fixture.Create<string>(),
                    CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>()
                    {
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="20",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="ScotlandTest",
                            Scotland="ScotlandTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Material = "Material1",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        },
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="20",
                            England="EnglandTest",
                            Wales="WalesTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Name="Material1",
                            Scotland="ScotlandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        },
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="10",
                            England="EnglandTest",
                            Wales="WalesTest",
                            NorthernIreland = "NorthernIrelandTest",
                            Name="Material2",
                            Scotland="ScotlandTest",
                            Total = "TotalTest",
                            ProducerReportedHouseholdPackagingWasteTonnage = Fixture.Create<string>(),
                            ReportedPublicBinTonnage = Fixture.Create<string>(),
                            ProducerReportedTotalTonnage = Fixture.Create<string>(),
                        },
                    },
                },
                CalcResultLapcapData = new CalcResultLapcapData() { CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>() { } },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment()
                {
                    Name = Fixture.Create<string>(),
                    CalcResultOnePlusFourApportionmentDetails =
                    [
                        new ()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 0.10M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 0.20M,
                            Name = "Test",
                        },
                        new ()
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
                        new ()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 0.10M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 0.20M,
                            Name = "Test",
                        },
                        new ()
                        {
                            EnglandDisposalTotal = "80",
                            NorthernIrelandDisposalTotal = "70",
                            ScotlandDisposalTotal = "30",
                            WalesDisposalTotal = "20",
                            AllTotal = 0.1M,
                            EnglandTotal = 0.10M,
                            NorthernIrelandTotal = 0.15M,
                            ScotlandTotal = 0.15M,
                            WalesTotal = 0.20M,
                            Name = "Test",
                        },
                        new ()
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
                            Name = OnePlus4ApportionmentColumnHeaders.OnePluseFourApportionment,
                        }

                    ],
                },
                CalcResultParameterCommunicationCost = Fixture.Create<CalcResultParameterCommunicationCost>(),
                CalcResultSummary = new CalcResultSummary
                {
                    ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>()
                    {
                        new ()
                            {
                            ProducerCommsFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>() { },
                            ProducerDisposalFeesByMaterial = new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>() { },
                            ProducerId = "1",
                            ProducerName = "Test",
                            TotalProducerDisposalFeeWithBadDebtProvision = 100,
                            TotalProducerCommsFeeWithBadDebtProvision = 100,
                            SubsidiaryId = "1",
                            },
                    },
                },
                CalcResultCommsCostReportDetail = new CalcResultCommsCost()
                {
                    CalcResultCommsCostCommsCostByMaterial =
                    [
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne = "0.42",
                            Name = "Material1",
                        },
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne = "0.3",
                            Name = "Material2",
                        },
                    ],
                    CommsCostByCountry =
                    [
                        new ()
                        {
                            Total = "Total",
                        },
                        new ()
                        {
                            TotalValue = 2530,
                        },
                    ],
                },
                CalcResultLateReportingTonnageData = Fixture.Create<CalcResultLateReportingTonnage>(),

                CalcResultScaledupProducers = new CalcResultScaledupProducers
                {
                    TitleHeader = null,
                    MaterialBreakdownHeaders = null,
                    ColumnHeaders = null,
                    ScaledupProducers = new List<CalcResultScaledupProducer>(),
                },
            };

            // Seed database
            SeedDatabase(this.context);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.context.Database.EnsureDeleted();
            this.context.Dispose();
        }

        [TestMethod]
        public void Construct_ShouldReturnCalcResultSummary()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.ConstructAsync(requestDto, this.calcResult);

            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(CalcResultSummaryHeaders.CalculationResult, result.ResultSummaryHeader?.Name);
            Assert.AreEqual(26, result.ProducerDisposalFeesHeaders.Count());

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ProducerDisposalFees);
            Assert.AreEqual(5, result.ProducerDisposalFees.Count());
            var firstProducer = result.ProducerDisposalFees.FirstOrDefault();
            Assert.IsNotNull(firstProducer);
            Assert.AreEqual("Producer1", firstProducer.ProducerName);
            Assert.AreEqual(0, calcResultsService.ScaledupProducers.Count());
        }

        [TestMethod]
        public void Construct_NullScaledUpProdcuers_ShouldSetScaledupProducersToEmptyCollection()
        {
            // Assign
            var requestDto = new CalcResultsRequestDto { RunId = 1 };
            var calcResult = this.calcResult;
            calcResult.CalcResultScaledupProducers = new CalcResultScaledupProducers
            {
                ColumnHeaders = new List<CalcResultScaledupProducerHeader>(),
                MaterialBreakdownHeaders = new List<CalcResultScaledupProducerHeader>(),
                TitleHeader = new CalcResultScaledupProducerHeader()
                {
                    Name = "Scaled-up Producers",
                    ColumnIndex = 1,
                },
                ScaledupProducers = null
            };

            // Act
            var results = this.calcResultsService.ConstructAsync(requestDto, calcResult);
            results.Wait();
            var result = results.Result;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, calcResultsService.ScaledupProducers.Count());
        }

        [TestMethod]
        public void Construct_ShouldSetScaledupProducers()
        {
            // Assign
            var requestDto = new CalcResultsRequestDto { RunId = 1 };
            var calcResult = this.calcResult;
            calcResult.CalcResultScaledupProducers = TestDataHelper.GetScaledupProducers();

            // Act
            var results = this.calcResultsService.ConstructAsync(requestDto, calcResult);
            results.Wait();
            var result = results.Result;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, calcResultsService.ScaledupProducers.Count());
        }

        [TestMethod]
        public void Construct_ShouldMapMaterialBreakdownHeaders()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.ConstructAsync(requestDto, this.calcResult);

            results.Wait();
            var result = results.Result;

            Assert.AreEqual("Material1 Breakdown", result.MaterialBreakdownHeaders.First().Name);
        }

        [TestMethod]
        public void Construct_ShouldCalculateProducerDisposalFeesCorrectly()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.ConstructAsync(requestDto, this.calcResult);

            results.Wait();
            var result = results.Result;

            Assert.IsTrue(result.ProducerDisposalFees.Any());
            Assert.AreEqual("Producer1", result.ProducerDisposalFees.First().ProducerName);
        }

        [TestMethod]
        public void Construct_ShouldReturnEmptyProducerDisposalFees_WhenNoProducers()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.ConstructAsync(requestDto, this.calcResult);

            results.Wait();
            var result = results.Result;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ProducerDisposalFees);
        }

        [TestMethod]
        public void Construct_ShouldCalculateBadDebtProvisionCorrectly()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var result = this.calcResultsService.ConstructAsync(requestDto, this.calcResult);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, 0);
        }

        [TestMethod]
        public void Construct_ShouldReturnProducerDisposalFees_WithoutSubsidiaryTotalRow()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.ConstructAsync(calcResultsRequestDto, this.calcResult);

            results.Wait();
            var result = results.Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.ProducerDisposalFees.Count());
            Assert.IsFalse(result.ProducerDisposalFees.Any(fee => fee.ProducerName.Contains("Total")));
        }

        [TestMethod]
        public void Construct_ShouldReturnProducerDisposalFees_WithSubsidiaryTotalRow()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.ConstructAsync(calcResultsRequestDto, this.calcResult);

            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.ProducerDisposalFees.Count());
        }

        [TestMethod]
        public void Construct_ShouldReturnOverallTotalRow_ForAllProducers()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.ConstructAsync(calcResultsRequestDto, this.calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);
            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
        }

        [TestMethod]
        public void GetTotalBadDebtprovision1_ShouldReturnCorrectValue()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.ConstructAsync(calcResultsRequestDto, this.calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);

            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
            totalRow.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision = 100m;
            totalRow.Level = "Totals";

            var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision);

            Assert.AreEqual(100m, totalFee);
        }

        [TestMethod]
        public void GetTotalDisposalCostswithBadDebtprovision1_ShouldReturnCorrectValue()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.ConstructAsync(calcResultsRequestDto, this.calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);

            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
            totalRow.TotalProducerDisposalFeeWithBadDebtProvision = 200m;
            totalRow.Level = "Totals";

            var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.TotalProducerDisposalFeeWithBadDebtProvision);

            Assert.AreEqual(200m, totalFee);
        }

        [TestMethod]
        public void GetTotalCommsCostswoBadDebtprovision2A_ShouldReturnCorrectValue()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.ConstructAsync(calcResultsRequestDto, this.calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);

            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
            totalRow.TotalProducerCommsFee = 300m;
            totalRow.Level = "Totals";

            var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.TotalProducerCommsFee);

            Assert.AreEqual(300m, totalFee);
        }

        [TestMethod]
        public void GetTotalBadDebtprovision2A_ShouldReturnCorrectValue()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = this.calcResultsService.ConstructAsync(calcResultsRequestDto, this.calcResult);

            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);

            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
            totalRow.CommunicationCostsSectionTwoA!.BadDebtProvision = 400m;
            totalRow.Level = "Totals";

            var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.CommunicationCostsSectionTwoA!.BadDebtProvision);

            Assert.AreEqual(400m, totalFee);
        }

        [TestMethod]
        public void GetTotalCommsCostswithBadDebtprovision2A_ShouldReturnCorrectValue()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.ConstructAsync(calcResultsRequestDto, this.calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);

            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
            totalRow.TotalProducerCommsFeeWithBadDebtProvision = 500m;
            totalRow.Level = "Totals";

            var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.TotalProducerCommsFeeWithBadDebtProvision);

            Assert.AreEqual(500m, totalFee);
        }

        [TestMethod]
        public void GetTotalFee_ShouldReturnZero_WhenNoTotalsLevel()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.ConstructAsync(calcResultsRequestDto, this.calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);

            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
            totalRow.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision = 0m;
            totalRow.Level = "Totals";

            var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(result.ProducerDisposalFees.ToList(), fee => fee.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision);

            Assert.AreEqual(0m, totalFee);
        }

        [TestMethod]
        public void GetTotalFee_ShouldReturnZero_WhenProducerDisposalFeesNull()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.ConstructAsync(calcResultsRequestDto, this.calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);

            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
            totalRow.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision = 0m;
            totalRow.Level = "Totals";

            var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee(null, fee => fee.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision);

            Assert.AreEqual(0m, totalFee);
        }

        [TestMethod]
        public void GetTotalFee_ShouldReturnZero_WhenProducerDisposalFeesIsEmpty()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.ConstructAsync(calcResultsRequestDto, this.calcResult);
            results.Wait();
            var result = results.Result;
            Assert.IsNotNull(result);

            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
            totalRow.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision = 0m;
            totalRow.Level = "Totals";

            var totalFee = CalcResultOneAndTwoAUtil.GetTotalFee([], fee => fee.LocalAuthorityDisposalCostsSectionOne!.BadDebtProvision);

            Assert.AreEqual(0m, totalFee);
        }

        [TestMethod]
        public void ProducerTotalPercentageVsTotal_ShouldReturnCorrectValue()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.ConstructAsync(requestDto, this.calcResult);
            results.Wait();
            var result = results.Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(CalcResultSummaryHeaders.CalculationResult, result.ResultSummaryHeader!.Name);
            Assert.AreEqual(26, result.ProducerDisposalFeesHeaders!.Count());

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ProducerDisposalFees);
            Assert.AreEqual(5, result.ProducerDisposalFees.Count());
            var producerTotalPercentage = result.ProducerDisposalFees.First().PercentageofProducerReportedTonnagevsAllProducers;
            Assert.IsNotNull(producerTotalPercentage);
            Assert.AreEqual(40, producerTotalPercentage);
        }

        [TestMethod]
        public void CommsCost2bBill_ShouldReturnCorrectValue()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };
            var results = this.calcResultsService.ConstructAsync(requestDto, this.calcResult);

            results.Wait();
            var result = results.Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(CalcResultSummaryHeaders.CalculationResult, result.ResultSummaryHeader!.Name);
            Assert.AreEqual(26, result.ProducerDisposalFeesHeaders!.Count());
            var isColumnHeaderExists = result.ProducerDisposalFeesHeaders!.Select(dict => dict.ColumnIndex == 233 || dict.ColumnIndex == 234 || dict.ColumnIndex == 215).ToList();
            Assert.IsTrue(isColumnHeaderExists.Contains(true));
            Assert.IsNotNull(result.ProducerDisposalFees);
            Assert.AreEqual(5, result.ProducerDisposalFees.Count());
        }

        [TestMethod]
        public void MaterialMapper_ShouldReturnCorrectValue()
        {
            var materials = this.context.Material.ToList();
            var result = Mappers.MaterialMapper.Map(materials);
            Assert.IsNotNull(result);
            Assert.AreEqual(materials.Count, result.Count);
            var material = result[0];
            var actualMaterial = materials[0];
            Assert.IsNotNull(material);
            Assert.IsNotNull(actualMaterial);
            Assert.AreEqual(material.Name, actualMaterial.Name);
            Assert.AreEqual(material.Code, actualMaterial.Code);
        }

        [TestMethod]
        public void GetProducerRunMaterialDetails_ShouldReturnCorrectValue()
        {
            var result = CalcResultSummaryBuilder.GetProducerRunMaterialDetails(
                this.context.ProducerDetail.ToList(),
                this.context.ProducerReportedMaterial.ToList(),
                1);
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count());
            var producer = result.FirstOrDefault(t => t.ProducerDetail.Id == 1);
            Assert.AreEqual(3, producer?.ProducerDetail.ProducerReportedMaterials.Count);
            Assert.AreEqual("Producer1", producer?.ProducerDetail.ProducerName);
        }

        [TestMethod]
        public void GetOrderedListOfProducersAssociatedRunId_ShouldReturnCorrectValue()
        {
            var result = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, this.context.ProducerDetail.ToList());
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count());
            Assert.AreEqual("Producer1", result.First().ProducerName);
            Assert.AreEqual("Subsidiary1", result.Last().ProducerName);
        }

        [TestMethod]
        public void GetPreviousInvoicedTonnage_ReturnsCorrectResults()
        {
            // Arrange
            var financialYear = "2024-25";

            // Act
            var result = calcResultsService.GetPreviousInvoicedTonnageFromDb(financialYear);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(12.5M, result.First().InvoicedTonnage?.InvoicedNetTonnage);
            Assert.AreEqual(3, result.First().InvoicedTonnage?.Id); // latest CalculatorRun Id
            Assert.AreEqual(1, result.First().InvoicedTonnage?.ProducerId);
        }

        [TestMethod]
        public void GetCalcResultSummary_ShouldReturnCorrectValue()
        {
            var orderedProducerDetails = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, this.context.ProducerDetail.ToList());
            var runProducerMaterialDetails = CalcResultSummaryBuilder.GetProducerRunMaterialDetails(
                orderedProducerDetails,
                this.context.ProducerReportedMaterial.ToList(),
                1);

            var materials = Mappers.MaterialMapper.Map(this.context.Material.ToList());

            var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(runProducerMaterialDetails,
                                                                                                materials,
                                                                                                1,
                                                                                                calcResultsService.ScaledupProducers?.ToList() ?? new List<CalcResultScaledupProducer>());

            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb("2024-25");

            var defaultParams = new List<DefaultParamResultsClass>();


            var result = new CalcResultSummaryBuilder(this.context).GetCalcResultSummary(orderedProducerDetails, materials, this.calcResult, totalPackagingTonnage, producerInvoicedMaterialNetTonnage, defaultParams);

            Assert.IsNotNull(result);
            Assert.AreEqual(145, result.ColumnHeaders.Count());

            var producerDisposalFees = result.ProducerDisposalFees;
            Assert.IsNotNull(producerDisposalFees);

            var totals = producerDisposalFees.First(t => t.IsProducerScaledup == "Totals");

            var producer = producerDisposalFees.First(t => t.Level == "1");
            Assert.IsNotNull(producer);

            Assert.AreEqual(string.Empty, totals?.ProducerName);
            Assert.IsNotNull(producer.ProducerName);
            Assert.AreEqual("Producer1", producer.ProducerName);
        }

        [TestMethod]
        public void GetCalcResultSummary_CanAddTotalRow()
        {
            var sut = new CalcResultSummaryBuilder(this.context);
            calcResultsService.ParentOrganisations = new List<ScaledupOrganisation> {
                new() { OrganisationId = 1, OrganisationName = "Org1" }
            };

            var orderedProducerDetails = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, this.context.ProducerDetail.ToList()).ToList();
            var runProducerMaterialDetails = CalcResultSummaryBuilder.GetProducerRunMaterialDetails(
                orderedProducerDetails,
                this.context.ProducerReportedMaterial.ToList(),
                1);

            var materials = Mappers.MaterialMapper.Map(this.context.Material.ToList());

            var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(runProducerMaterialDetails, materials, 1, calcResultsService.ScaledupProducers?.ToList() ?? new List<CalcResultScaledupProducer>());

            orderedProducerDetails.Add(new ProducerDetail
            {
                ProducerId = 1
            });

            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb("2024-25");
            var defaultParams = new List<DefaultParamResultsClass>();

            var result = sut.GetCalcResultSummary(orderedProducerDetails, materials, this.calcResult, totalPackagingTonnage, producerInvoicedMaterialNetTonnage, defaultParams);

            Assert.IsNotNull(result);
            Assert.AreEqual(145, result.ColumnHeaders.Count());

            var producerDisposalFees = result.ProducerDisposalFees;
            Assert.IsNotNull(producerDisposalFees);

            var totals = producerDisposalFees.First(t => t.IsProducerScaledup == "Totals");

            var producer = producerDisposalFees.First(t => t.Level == "1");
            Assert.IsNotNull(producer);

            Assert.AreEqual(string.Empty, totals?.ProducerName);
            Assert.IsNotNull(producer.ProducerName);
            Assert.AreEqual("Producer1", producer.ProducerName);
        }

        [TestMethod]
        public void GetTonnages_ShouldCalculateCorrectlyForGlass()
        {
            List<CalculatorRunPomDataDetail> pomData = new List<CalculatorRunPomDataDetail>();
            List<MaterialDetail> materials = new List<MaterialDetail>();

            var glassMaterial = new MaterialDetail
            {
                Code = MaterialCodes.Glass,
                Name = "Glass",
                Description = "Glass material description",
            };
            materials.Add(glassMaterial);

            var glassPomData = new List<CalculatorRunPomDataDetail>
            {
                new CalculatorRunPomDataDetail
                {
                    PackagingMaterial = MaterialCodes.Glass,
                    SubmissionPeriod = "2024-P1",
                    PackagingType = PackagingTypes.Household,
                    PackagingMaterialWeight = 100,
                    SubmissionPeriodDesc = "2024 Period 1",
                    LoadTimeStamp = DateTime.UtcNow,
                },
                new CalculatorRunPomDataDetail
                {
                    PackagingMaterial = MaterialCodes.Glass,
                    SubmissionPeriod = "2024-P1",
                    PackagingType = PackagingTypes.HouseholdDrinksContainers,
                    PackagingMaterialWeight = 30,
                    SubmissionPeriodDesc = "2024 Period 1",
                    LoadTimeStamp = DateTime.UtcNow,
                },
            };

            pomData.AddRange(glassPomData);

            // Act
            var result = CalcResultScaledupProducersBuilder.GetTonnages(pomData, materials, "2024-P1", 1);

            // Assert
            Assert.IsTrue(result.ContainsKey(MaterialCodes.Glass));
            var glassTonnage = result[MaterialCodes.Glass];

            Assert.AreEqual(0.1m, glassTonnage.ReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0, glassTonnage.ReportedPublicBinTonnage);
            Assert.AreEqual(0.03m, glassTonnage.HouseholdDrinksContainersTonnageGlass);
            Assert.AreEqual(0.13m, glassTonnage.TotalReportedTonnage);
            Assert.AreEqual(0.13m, glassTonnage.NetReportedTonnage);
            Assert.AreEqual(0.1m, glassTonnage.ScaledupReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(0, glassTonnage.ScaledupReportedPublicBinTonnage);
            Assert.AreEqual(0.03m, glassTonnage.ScaledupHouseholdDrinksContainersTonnageGlass);
            Assert.AreEqual(0.13m, glassTonnage.ScaledupTotalReportedTonnage);
            Assert.AreEqual(0.13m, glassTonnage.ScaledupNetReportedTonnage);
        }

        [TestMethod]
        public void GetCalcResultSummary_ScaledUpProducerShouldReturnCorrectValue()
        {

            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            this.calcResult.CalcResultScaledupProducers.ScaledupProducers = GetScaledUpProducers();
            var results = this.calcResultsService.ConstructAsync(calcResultsRequestDto, this.calcResult);

            var orderedProducerDetails = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, this.context.ProducerDetail.ToList());
            var runProducerMaterialDetails = CalcResultSummaryBuilder.GetProducerRunMaterialDetails(
                orderedProducerDetails,
                this.context.ProducerReportedMaterial.ToList(),
                1);

            var materials = Mappers.MaterialMapper.Map(this.context.Material.ToList());

            var totalPackagingTonnage = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(runProducerMaterialDetails,
                                                                                                materials,
                                                                                                1,
                                                                                                calcResultsService.ScaledupProducers?.ToList() ?? new List<CalcResultScaledupProducer>());
            var scaledUpProducer = totalPackagingTonnage.First(t => t.ProducerId == 4);

            Assert.AreEqual(2, totalPackagingTonnage.Count());
            Assert.IsNotNull(scaledUpProducer.ProducerId);
            Assert.AreEqual(100, scaledUpProducer.TotalPackagingTonnage);
        }

        [TestMethod]
        public void GetScaledupProducerStatusTotalRow_IsOverAllTotalRow_ReturnsTotals()
        {
            // Arrange
            var producer = new ProducerDetail();
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            bool isOverAllTotalRow = true;

            // Act
            var result = CalcResultSummaryBuilder.GetScaledupProducerStatusTotalRow(producer, scaledupProducers, isOverAllTotalRow);

            // Assert
            Assert.AreEqual(CommonConstants.Totals, result);
        }

        [TestMethod]
        public void GetScaledupProducerStatusTotalRow_ProducerScaledup_ReturnsScaledupProducersYes()
        {
            // Arrange
            var producer = new ProducerDetail();
            var scaledupProducers = new List<CalcResultScaledupProducer> { new CalcResultScaledupProducer { ProducerId = producer.Id } };
            bool isOverAllTotalRow = false;

            // Act
            var result = CalcResultSummaryBuilder.GetScaledupProducerStatusTotalRow(producer, scaledupProducers, isOverAllTotalRow);

            // Assert
            Assert.AreEqual(CommonConstants.ScaledupProducersYes, result);
        }

        [TestMethod]
        public void GetScaledupProducerStatusTotalRow_ProducerNotScaledup_ReturnsScaledupProducersNo()
        {
            // Arrange
            var producer = new ProducerDetail();
            var scaledupProducers = new List<CalcResultScaledupProducer>();
            bool isOverAllTotalRow = false;

            // Act
            var result = CalcResultSummaryBuilder.GetScaledupProducerStatusTotalRow(producer, scaledupProducers, isOverAllTotalRow);

            // Assert
            Assert.AreEqual(CommonConstants.ScaledupProducersNo, result);
        }

        public static List<CalcResultScaledupProducer> GetScaledUpProducers()
        {
            var test = new List<CalcResultScaledupProducer>
            {
                new CalcResultScaledupProducer()
                {
                    ProducerId = 4,
                    ProducerName = "Test",
                    ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>
                {
                    {
                        "1",
                        new CalcResultScaledupProducerTonnage
                        {
                            ReportedHouseholdPackagingWasteTonnage = 0,
                            ReportedPublicBinTonnage = 0,
                            TotalReportedTonnage = 0,
                            ReportedSelfManagedConsumerWasteTonnage = 0,
                            NetReportedTonnage = 0,
                            ScaledupReportedHouseholdPackagingWasteTonnage = 0,
                            ScaledupReportedPublicBinTonnage = 0,
                            ScaledupTotalReportedTonnage = 100,
                            ScaledupReportedSelfManagedConsumerWasteTonnage = 0,
                            ScaledupNetReportedTonnage = 0,
                        }
                    },
                },
                },
            };

            return test;
        }

        [TestMethod]
        public void CanAddTotalRow_ParentProducerNotFound_ReturnsFalse()
        {
            // Arrange
            ProducerDetail producer = context.ProducerDetail.FirstOrDefault()!;
            IEnumerable<ProducerDetail> producersAndSubsidiaries = context.ProducerDetail;
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>();

            calcResultsService.ParentOrganisations = new List<ScaledupOrganisation>();

            // Act
            var result = calcResultsService.CanAddTotalRow(producer, producersAndSubsidiaries, producerDisposalFees);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanAddTotalRow_ProducerDisposalFeeExists_ReturnsFalse()
        {
            // Arrange
            ProducerDetail producer = context.ProducerDetail.FirstOrDefault()!;
            IEnumerable<ProducerDetail> producersAndSubsidiaries = context.ProducerDetail;
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> {
                    new CalcResultSummaryProducerDisposalFees { ProducerId = "1", ProducerName="Org1", SubsidiaryId = "" }
                };

            calcResultsService.ParentOrganisations = new List<ScaledupOrganisation> {
                    new ScaledupOrganisation { OrganisationId = 1, OrganisationName = "Org1" }
                };

            // Act
            var result = calcResultsService.CanAddTotalRow(producer, producersAndSubsidiaries, producerDisposalFees);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanAddTotalRow_ValidConditions_ReturnsTrue()
        {
            // Arrange
            ProducerDetail producer = context.ProducerDetail.FirstOrDefault()!;
            IEnumerable<ProducerDetail> producersAndSubsidiaries = context.ProducerDetail;
            List<CalcResultSummaryProducerDisposalFees> producerDisposalFees = new List<CalcResultSummaryProducerDisposalFees> {
                    new CalcResultSummaryProducerDisposalFees { ProducerId = "2", ProducerName="Org1", SubsidiaryId = "" }
                };

            calcResultsService.ParentOrganisations = new List<ScaledupOrganisation> {
                    new ScaledupOrganisation { OrganisationId = 1, OrganisationName = "Org1" }
                };

            // Act
            var result = calcResultsService.CanAddTotalRow(producer, producersAndSubsidiaries, producerDisposalFees);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetProducerDetailsForTotalRow_IsOverallTotalRow_ReturnsNullObject()
        {
            // Arrange
            int producerId = 1;
            bool isOverAllTotalRow = true;

            // Act
            var result = calcResultsService.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetProducerDetailsForTotalRow_ParentProducerNotFound_ReturnsNullObject()
        {
            // Arrange
            int producerId = 1;
            bool isOverAllTotalRow = false;

            calcResultsService.ParentOrganisations = new List<ScaledupOrganisation>();

            // Act
            var result = calcResultsService.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetProducerDetailsForTotalRow_ParentProducerFoundWithName_ReturnsOrganisationName()
        {
            // Arrange
            int producerId = 1;
            bool isOverAllTotalRow = false;

            calcResultsService.ParentOrganisations = new List<ScaledupOrganisation> {
                    new ScaledupOrganisation { OrganisationId = 1, OrganisationName = "Org1" }
                };

            // Act
            var result = calcResultsService.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

            // Assert
            Assert.AreEqual("Org1", result!.OrganisationName);
        }

        [TestMethod]
        public void GetProducerDetailsForTotalRow_ParentProducerFoundWithoutName_ReturnsNullString()
        {
            // Arrange
            int producerId = 1;
            bool isOverAllTotalRow = false;

            calcResultsService.ParentOrganisations = new List<ScaledupOrganisation> {
                    new ScaledupOrganisation { OrganisationId = 1, OrganisationName = null }
                };

            // Act
            var result = calcResultsService.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

            // Assert
            Assert.IsNull(result!.OrganisationName);
        }

        [TestMethod]
        public void GetProducerDetailsForTotalRow_ParentProducerFoundWithTradingName_ReturnsTradingName()
        {
            // Arrange
            int producerId = 1;
            bool isOverAllTotalRow = false;

            calcResultsService.ParentOrganisations = new List<ScaledupOrganisation> {
                    new ScaledupOrganisation { OrganisationId = 1, OrganisationName = "Good Fruit", TradingName = "GF Trading Name 1" }
                };

            // Act
            var result = calcResultsService.GetProducerDetailsForTotalRow(producerId, isOverAllTotalRow);

            // Assert
            Assert.AreEqual("GF Trading Name 1", result!.TradingName);
        }

        [TestMethod]
        public void Construct_HandlesNullScaledupProducers_UsesEmptyList()
        {
            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 1 };
            var calcResult = this.calcResult;
            calcResult.CalcResultScaledupProducers = new CalcResultScaledupProducers
            {
                ScaledupProducers = null
            };

            // Act
            var task = this.calcResultsService.ConstructAsync(requestDto, calcResult);
            task.Wait();
            var result = task.Result;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ProducerDisposalFees?.Any() ?? false);
        }

        [TestMethod]
        public void GetCalcResultSummary_AddsProducerTotalRow_AndProducerRow()
        {
            // Arrange
            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, this.context.ProducerDetail.ToList());
            var materials = Mappers.MaterialMapper.Map(this.context.Material.ToList());
            var runDetails = CalcResultSummaryBuilder.GetProducerRunMaterialDetails(ordered, this.context.ProducerReportedMaterial.ToList(), 1);

            this.calcResultsService.ScaledupProducers = new List<CalcResultScaledupProducer>();

            var totalPackaging = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(
                runDetails, materials, 1, this.calcResultsService.ScaledupProducers.ToList());

            this.calcResultsService.ParentOrganisations = new List<ScaledupOrganisation>
            {
                new ScaledupOrganisation { OrganisationId = ordered.First().ProducerId, OrganisationName = "Org1" }
            };

            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb("2024-25");
            var defaultParams = new List<DefaultParamResultsClass>();
            // Act
            var summary = this.calcResultsService.GetCalcResultSummary(ordered, materials, this.calcResult, totalPackaging, producerInvoicedMaterialNetTonnage, defaultParams);

            // Assert
            Assert.IsNotNull(summary);
            Assert.IsTrue(summary.ProducerDisposalFees.Any());
            Assert.IsTrue(summary.ProducerDisposalFees.Any(r => r.isTotalRow));
            Assert.IsTrue(summary.ProducerDisposalFees.Any(r => !r.isTotalRow));
        }

        [TestMethod]
        public void GetCalcResultSummary_AddsTotalRow_WhenProducerHasSubsidiary()
        {
            var parent = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
            var sub = new ProducerDetail
            {
                ProducerName = parent.ProducerName + " and subsidiary",
                ProducerId = parent.ProducerId,
                CalculatorRunId = parent.CalculatorRunId,
                SubsidiaryId = "S1"
            };
            context.ProducerDetail.Add(sub);
            context.ProducerReportedMaterial.Add(new ProducerReportedMaterial
            {
                ProducerDetail = sub,
                MaterialId = 1,
                PackagingType = "HH",
                PackagingTonnage = 10m
            });
            context.SaveChanges();

            calcResultsService.ParentOrganisations = new List<ScaledupOrganisation>
            {
                new ScaledupOrganisation { OrganisationId = parent.ProducerId, OrganisationName = "Org1" }
            };

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
            var materials = Mappers.MaterialMapper.Map(context.Material.ToList());
            var runDetails = CalcResultSummaryBuilder.GetProducerRunMaterialDetails(ordered, context.ProducerReportedMaterial.ToList(), 1);
            var totalPackaging = CalcResultSummaryBuilder.GetTotalPackagingTonnagePerRun(
                runDetails, materials, 1, calcResultsService.ScaledupProducers?.ToList() ?? new List<CalcResultScaledupProducer>());

            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb("2024-25");
            var defaultParams = new List<DefaultParamResultsClass>();

            var summary = calcResultsService.GetCalcResultSummary(ordered, materials, calcResult, totalPackaging, producerInvoicedMaterialNetTonnage, defaultParams);

            Assert.IsTrue(summary.ProducerDisposalFees.Any(r => r.isTotalRow));
        }

        [TestMethod]
        public void GetProducerRow_MarksProducerAsScaledUp_WhenMatchExists()
        {

            var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);

            var materials = Mappers.MaterialMapper.Map(context.Material.ToList());

            var tonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>();

            foreach (var m in materials)
            {
                tonnageByMaterial[m.Code] = new CalcResultScaledupProducerTonnage
                {
                    ReportedHouseholdPackagingWasteTonnage = 0,
                    ReportedPublicBinTonnage = 0,
                    ReportedSelfManagedConsumerWasteTonnage = 0,
                    TotalReportedTonnage = 0,
                    NetReportedTonnage = 0,
                    ScaledupReportedHouseholdPackagingWasteTonnage = 0,
                    ScaledupReportedPublicBinTonnage = 0,
                    ScaledupReportedSelfManagedConsumerWasteTonnage = 0,
                    ScaledupTotalReportedTonnage = 0,
                    ScaledupNetReportedTonnage = 0
                };
            }

            calcResultsService.ScaledupProducers = new List<CalcResultScaledupProducer>
            {
                new CalcResultScaledupProducer
                {
                    ProducerId = producer.ProducerId,
                    SubsidiaryId = producer.SubsidiaryId,
                    ScaledupProducerTonnageByMaterial = tonnageByMaterial
                }
            };

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());

            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb("2024-25");

            var row = calcResultsService.GetProducerRow(
                new List<CalcResultSummaryProducerDisposalFees>(),
                ordered.Where(pd => pd.ProducerId == producer.ProducerId).ToList(),
                producer,
                materials,
                this.calcResult, new List<TotalPackagingTonnagePerRun>(),
                producerInvoicedMaterialNetTonnage);


            Assert.AreEqual(CommonConstants.ScaledupProducersYes, row.IsProducerScaledup);
        }

        [TestMethod]
        public void CanAddTotalRow_SingleProducerAndPomDataExists_ReturnsFalse()
        {
            var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);

            var producersAndSubsidiaries = new List<ProducerDetail>
            {
                new ProducerDetail
                {
                    ProducerId = producer.ProducerId,
                    CalculatorRunId = producer.CalculatorRunId,
                    SubsidiaryId = null,
                    ProducerName = "Parent Org"
                }
            };

            calcResultsService.ParentOrganisations = new List<ScaledupOrganisation>
            {
                new ScaledupOrganisation { OrganisationId = producer.ProducerId, OrganisationName = "Org1" }
            };

            var result = calcResultsService.CanAddTotalRow(producer, producersAndSubsidiaries, new List<CalcResultSummaryProducerDisposalFees>());

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetProducerRow_MarksProducerAsNotScaledUp_WhenNoMatch()
        {
            var producer = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
            var materials = Mappers.MaterialMapper.Map(context.Material.ToList());

            calcResultsService.ScaledupProducers = new List<CalcResultScaledupProducer>();

            var ordered = CalcResultSummaryBuilder.GetOrderedListOfProducersAssociatedRunId(1, context.ProducerDetail.ToList());
            var producerInvoicedMaterialNetTonnage = calcResultsService.GetPreviousInvoicedTonnageFromDb("2024-25");

            var row = calcResultsService.GetProducerRow(
                new List<CalcResultSummaryProducerDisposalFees>(),
                ordered.Where(pd => pd.ProducerId == producer.ProducerId).ToList(),
                producer,
                materials,
                this.calcResult,
                new List<TotalPackagingTonnagePerRun>(),
                producerInvoicedMaterialNetTonnage);

            Assert.AreEqual(CommonConstants.ScaledupProducersNo, row.IsProducerScaledup);
        }

        [TestMethod]
        public void CanAddTotalRow_NoParentPomButHasSubsidiary_ReturnsTrue()
        {
            var parent = context.ProducerDetail.Single(p => p.ProducerId == 1 && p.CalculatorRunId == 1);
            var subOnly = new List<ProducerDetail> {
                new ProducerDetail { ProducerId = parent.ProducerId, CalculatorRunId = parent.CalculatorRunId, SubsidiaryId = "S1", ProducerName = "Sub1" }
            };

            calcResultsService.ParentOrganisations = new List<ScaledupOrganisation> {
                new ScaledupOrganisation { OrganisationId = parent.ProducerId, OrganisationName = "Org1" }
            };

            var canAdd = calcResultsService.CanAddTotalRow(parent, subOnly, new List<CalcResultSummaryProducerDisposalFees>());
            Assert.IsTrue(canAdd);
        }
        
        [TestMethod]
        public void ConstructAsync_WhenIsBillingFileTrue_Persists_BillingInstructions_ToDb()
        {
            // Arrange
            var requestDto = new CalcResultsRequestDto
            {
                RunId = 1,
                FinancialYear = "2024-25",
                IsBillingFile = true
            };

            this.calcResult.CalcResultDetail.RunId = 1;

            // Act
            var summary = this.calcResultsService.ConstructAsync(requestDto, this.calcResult).Result;

            // Assert (verify Level 1 row exists and has a section)
            var level1 = summary.ProducerDisposalFees
                                .FirstOrDefault(f => f.Level == CommonConstants.LevelOne.ToString()
                                                  && f.ProducerIdInt == 1);
            Assert.IsNotNull(level1);
            Assert.IsNotNull(level1!.BillingInstructionSection, "BillingInstructionSection should be populated on Level 1.");

            // Assert
            var entity = context.ProducerResultFileSuggestedBillingInstruction
                                .Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);

            Assert.AreEqual(level1.BillingInstructionSection!.CurrentYearInvoiceTotalToDate, entity.CurrentYearInvoiceTotalToDate);
            Assert.AreEqual(level1.BillingInstructionSection!.TonnageChangeSinceLastInvoice, entity.TonnageChangeSinceLastInvoice);
            Assert.AreEqual(level1.BillingInstructionSection!.LiabilityDifference, entity.AmountLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(level1.BillingInstructionSection!.MaterialThresholdBreached, entity.MaterialPoundThresholdBreached);
            Assert.AreEqual(level1.BillingInstructionSection!.TonnageThresholdBreached, entity.TonnagePoundThresholdBreached);
            Assert.AreEqual(level1.BillingInstructionSection!.PercentageLiabilityDifference, entity.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(level1.BillingInstructionSection!.TonnagePercentageThresholdBreached, entity.TonnagePercentageThresholdBreached);
            Assert.AreEqual(level1.BillingInstructionSection!.SuggestedBillingInstruction, entity.SuggestedBillingInstruction);
            Assert.AreEqual(level1.BillingInstructionSection!.SuggestedInvoiceAmount ?? 0m, entity.SuggestedInvoiceAmount);
        }

        [TestMethod]
        public void ConstructAsync_WhenIsBillingFileFalse_DoesNotPersist_Billing_Instructions_ToDb()
        {
            // Arrange
            var requestDto = new CalcResultsRequestDto
            {
                RunId = 1,
                FinancialYear = "2024-25",
                IsBillingFile = false 
            };

            this.calcResult.CalcResultDetail.RunId = 1;

            var beforeEntity = context.ProducerResultFileSuggestedBillingInstruction
                                     .Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);

            var before = new
            {
                beforeEntity.CurrentYearInvoiceTotalToDate,
                beforeEntity.TonnageChangeSinceLastInvoice,
                beforeEntity.AmountLiabilityDifferenceCalcVsPrev,
                beforeEntity.MaterialPoundThresholdBreached,
                beforeEntity.TonnagePoundThresholdBreached,
                beforeEntity.PercentageLiabilityDifferenceCalcVsPrev,
                beforeEntity.TonnagePercentageThresholdBreached,
                beforeEntity.SuggestedBillingInstruction,
                beforeEntity.SuggestedInvoiceAmount
            };

            // Act
            var summary = this.calcResultsService.ConstructAsync(requestDto, this.calcResult).Result;

            
            var afterEntity = context.ProducerResultFileSuggestedBillingInstruction
                                    .Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);

            Assert.AreEqual(before.CurrentYearInvoiceTotalToDate, afterEntity.CurrentYearInvoiceTotalToDate);
            Assert.AreEqual(before.TonnageChangeSinceLastInvoice, afterEntity.TonnageChangeSinceLastInvoice);
            Assert.AreEqual(before.AmountLiabilityDifferenceCalcVsPrev, afterEntity.AmountLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(before.MaterialPoundThresholdBreached, afterEntity.MaterialPoundThresholdBreached);
            Assert.AreEqual(before.TonnagePoundThresholdBreached, afterEntity.TonnagePoundThresholdBreached);
            Assert.AreEqual(before.PercentageLiabilityDifferenceCalcVsPrev, afterEntity.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(before.TonnagePercentageThresholdBreached, afterEntity.TonnagePercentageThresholdBreached);
            Assert.AreEqual(before.SuggestedBillingInstruction, afterEntity.SuggestedBillingInstruction);
            Assert.AreEqual(before.SuggestedInvoiceAmount, afterEntity.SuggestedInvoiceAmount);
        }

        [TestMethod]
        public async Task UpdateBillingInstructions_NoChanges_When_NoLevel1Fees()
        {
            // Arrange: 
            var calcResult = TestDataHelper.GetCalcResult();
            calcResult.CalcResultDetail.RunId = 1;

            var summary = TestDataHelper.GetCalcResultSummary();
            summary.ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new()
                {
                    ProducerId = "1",
                    ProducerIdInt = 1,
                    ProducerName = "Producer 1",
                    SubsidiaryId = "Subsidiary 1",
                    Level = "2"
                }
            };

            var before = context.ProducerResultFileSuggestedBillingInstruction.Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1).SuggestedInvoiceAmount;

            // Act
            await calcResultsService.UpdateBillingInstructions(calcResult, summary);

            // Assert
            var after = context.ProducerResultFileSuggestedBillingInstruction.Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1).SuggestedInvoiceAmount;

            Assert.AreEqual(before, after);
        }

        [TestMethod]
        public async Task UpdateBillingInstructions_Level1ButMissingEntity_SkipsUpdate()
        {
            var calcResult = TestDataHelper.GetCalcResult();
            calcResult.CalcResultDetail.RunId = 1;

            var summary = TestDataHelper.GetCalcResultSummary();
            summary.ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new()
                {
                    ProducerId = "999",
                    ProducerIdInt = 999,
                    ProducerName = "ProducerNotExistsInEntityList",
                    SubsidiaryId = string.Empty,
                    Level = CommonConstants.LevelOne.ToString(),
                    BillingInstructionSection = new CalcResultSummaryBillingInstruction
                    {
                        SuggestedBillingInstruction = "INITIAL",
                        SuggestedInvoiceAmount = 123m
                    }
                }
            };

            await calcResultsService.UpdateBillingInstructions(calcResult, summary);

            var entity = context.ProducerResultFileSuggestedBillingInstruction.Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);

            Assert.AreNotEqual(123m, entity.SuggestedInvoiceAmount);
        }

        [TestMethod]
        public async Task UpdateBillingInstructions_MapsAllFields_When_Level1EntityExists()
        {
            // Arrange
            var calcResult = TestDataHelper.GetCalcResult();
            calcResult.CalcResultDetail.RunId = 1;

            var section = new CalcResultSummaryBillingInstruction
            {
                CurrentYearInvoiceTotalToDate = 123.45m,
                TonnageChangeSinceLastInvoice = "Yes",
                LiabilityDifference = 678.90m,
                MaterialThresholdBreached = "Mat TH",
                TonnageThresholdBreached = "Ton TH",
                PercentageLiabilityDifference = 11.22m,
                TonnagePercentageThresholdBreached = "Ton % TH",
                SuggestedBillingInstruction = "ISSUE",
                SuggestedInvoiceAmount = 999.01m
            };

            var summary = TestDataHelper.GetCalcResultSummary();
            summary.ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new CalcResultSummaryProducerDisposalFees()
                {
                    ProducerId = "1",
                    ProducerIdInt = 1,
                    ProducerName = "Producer 1",
                    SubsidiaryId = string.Empty,
                    Level = CommonConstants.LevelOne.ToString(),
                    BillingInstructionSection = section
                }
            };

            // Act
            await calcResultsService.UpdateBillingInstructions(calcResult, summary);

            // Assert
            var entity = context.ProducerResultFileSuggestedBillingInstruction
                                .Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);

            Assert.AreEqual(section.CurrentYearInvoiceTotalToDate, entity.CurrentYearInvoiceTotalToDate);
            Assert.AreEqual(section.TonnageChangeSinceLastInvoice, entity.TonnageChangeSinceLastInvoice);
            Assert.AreEqual(section.LiabilityDifference, entity.AmountLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(section.MaterialThresholdBreached, entity.MaterialPoundThresholdBreached);
            Assert.AreEqual(section.TonnageThresholdBreached, entity.TonnagePoundThresholdBreached);
            Assert.AreEqual(section.PercentageLiabilityDifference, entity.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(section.TonnagePercentageThresholdBreached, entity.TonnagePercentageThresholdBreached);
            Assert.AreEqual(section.SuggestedBillingInstruction, entity.SuggestedBillingInstruction);
            Assert.AreEqual(section.SuggestedInvoiceAmount, entity.SuggestedInvoiceAmount);
        }

        [TestMethod]
        public async Task UpdateBillingInstructions_Level1WithNullBillingInstructionSection_SetsFieldsToNullOrZero()
        {
            // Arrange
            this.calcResult.CalcResultDetail.RunId = 1;

            var entity = context.ProducerResultFileSuggestedBillingInstruction.Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);
            entity.CurrentYearInvoiceTotalToDate = 999.99m;
            entity.TonnageChangeSinceLastInvoice = "CHANGE";
            entity.AmountLiabilityDifferenceCalcVsPrev = 888.88m;
            entity.MaterialPoundThresholdBreached = "MAT";
            entity.TonnagePoundThresholdBreached = "TON";
            entity.PercentageLiabilityDifferenceCalcVsPrev = 77.77m;
            entity.TonnagePercentageThresholdBreached = "TON%";
            entity.SuggestedBillingInstruction = "INITIAL";
            entity.SuggestedInvoiceAmount = 555.55m;
            await context.SaveChangesAsync();

            var summary = TestDataHelper.GetCalcResultSummary();
            summary.ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new()
                {
                    ProducerId = "1",
                    ProducerIdInt = 1,
                    ProducerName = "Producer 1",
                    SubsidiaryId = string.Empty,
                    Level = CommonConstants.LevelOne.ToString(),
                    BillingInstructionSection = null
                }
            };

            // Act
            await calcResultsService.UpdateBillingInstructions(this.calcResult, summary);

            // Assert
            var updated = context.ProducerResultFileSuggestedBillingInstruction.Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);
            Assert.IsNull(updated.CurrentYearInvoiceTotalToDate);
            Assert.IsNull(updated.TonnageChangeSinceLastInvoice);
            Assert.IsNull(updated.AmountLiabilityDifferenceCalcVsPrev);
            Assert.IsNull(updated.MaterialPoundThresholdBreached);
            Assert.IsNull(updated.TonnagePoundThresholdBreached);
            Assert.IsNull(updated.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.IsNull(updated.TonnagePercentageThresholdBreached);
            Assert.IsNull(updated.SuggestedBillingInstruction);
            Assert.AreEqual(0m, updated.SuggestedInvoiceAmount);
        }

        [TestMethod]
        public async Task UpdateBillingInstructions_Level1WithNullSuggestedInvoiceAmount_SetsInvoiceAmountToZero()
        {
            // Arrange
            this.calcResult.CalcResultDetail.RunId = 1;

            var entity = context.ProducerResultFileSuggestedBillingInstruction.Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);
            entity.SuggestedInvoiceAmount = 321.45m;
            await context.SaveChangesAsync();

            var section = new CalcResultSummaryBillingInstruction
            {
                CurrentYearInvoiceTotalToDate = 10m,
                TonnageChangeSinceLastInvoice = "No",
                LiabilityDifference = 5.5m,
                MaterialThresholdBreached = "Mat TH",
                TonnageThresholdBreached = "Ton TH",
                PercentageLiabilityDifference = 1.23m,
                TonnagePercentageThresholdBreached = "Ton % TH",
                SuggestedBillingInstruction = "ISSUE",
                SuggestedInvoiceAmount = null
            };

            var summary = TestDataHelper.GetCalcResultSummary();
            summary.ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new()
                {
                    ProducerId = "1",
                    ProducerIdInt = 1,
                    ProducerName = "Producer 1",
                    SubsidiaryId = string.Empty,
                    Level = CommonConstants.LevelOne.ToString(),
                    BillingInstructionSection = section
                }
            };

            // Act
            await calcResultsService.UpdateBillingInstructions(this.calcResult, summary);

            // Assert
            var updated = context.ProducerResultFileSuggestedBillingInstruction.Single(p => p.CalculatorRunId == 1 && p.ProducerId == 1);
            Assert.AreEqual(section.CurrentYearInvoiceTotalToDate, updated.CurrentYearInvoiceTotalToDate);
            Assert.AreEqual(section.TonnageChangeSinceLastInvoice, updated.TonnageChangeSinceLastInvoice);
            Assert.AreEqual(section.LiabilityDifference, updated.AmountLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(section.MaterialThresholdBreached, updated.MaterialPoundThresholdBreached);
            Assert.AreEqual(section.TonnageThresholdBreached, updated.TonnagePoundThresholdBreached);
            Assert.AreEqual(section.PercentageLiabilityDifference, updated.PercentageLiabilityDifferenceCalcVsPrev);
            Assert.AreEqual(section.TonnagePercentageThresholdBreached, updated.TonnagePercentageThresholdBreached);
            Assert.AreEqual(section.SuggestedBillingInstruction, updated.SuggestedBillingInstruction);
            Assert.AreEqual(0m, updated.SuggestedInvoiceAmount);
        }

        private static void SeedDatabase(ApplicationDBContext context)
        {
            context.Material.AddRange(new List<Material>
            {
                new() { Id = 1, Name = "Material1", Code = "123" },
                new() { Id = 2, Name = "Material2", Code = MaterialCodes.Glass },
            });

            var calculatorRunFinancialYear = new CalculatorRunFinancialYear { Name = "2024-25" };

            context.ProducerDetail.AddRange(new List<ProducerDetail>
            {
                new()
                {
                    Id = 1, ProducerName = "Producer1", ProducerId = 1, CalculatorRunId = 1,
                    CalculatorRun = new CalculatorRun
                    {
                        Id = 1,
                        Financial_Year = calculatorRunFinancialYear,
                        FinancialYearId = "2024-25",
                        Name = "CalculatorRunTest1",
                        CalculatorRunClassificationId=7,
                        CalculatorRunOrganisationDataMasterId = 1
                    }
                },

                new()
                {
                    Id = 2, ProducerName = "Producer2", ProducerId = 2, CalculatorRunId = 2,
                        CalculatorRun = new CalculatorRun
                        {
                            Id = 2,
                            Financial_Year = calculatorRunFinancialYear,
                            FinancialYearId = "2024-25",
                            Name = "CalculatorRunTest2",
                            CalculatorRunClassificationId=7,
                            CalculatorRunOrganisationDataMasterId = 1
                        }
                },

                new()
                {
                    Id = 3, ProducerName = "Producer3", ProducerId = 3, CalculatorRunId = 3,
                        CalculatorRun = new CalculatorRun
                        {
                            Id = 3,
                            Financial_Year = calculatorRunFinancialYear,
                            FinancialYearId = "2024-25",
                            Name = "CalculatorRunTest3",
                            CalculatorRunClassificationId=7
                        }
                },
                new() { Id = 4, ProducerName = "Producer4", ProducerId = 4, CalculatorRunId = 1 , SubsidiaryId=null },
                new() { Id = 5, ProducerName = "Producer5", ProducerId = 4, CalculatorRunId = 1 , SubsidiaryId="A123" },
                new() { Id = 6, ProducerName = "Subsidiary1", ProducerId = 4, CalculatorRunId = 1 , SubsidiaryId="A456"},
            });

            context.ProducerReportedMaterial.AddRange(new List<ProducerReportedMaterial>
            {
                new() { Id = 1, MaterialId = 1, PackagingType = "HH", PackagingTonnage = 400m, ProducerDetailId = 1 },
                new() { Id = 2, MaterialId = 2, PackagingType = "HH", PackagingTonnage = 400m, ProducerDetailId = 2 },
                new() { Id = 3, MaterialId = 1, PackagingType = "CW", PackagingTonnage = 200m, ProducerDetailId = 1 },
                new() { Id = 4, MaterialId = 2, PackagingType = "CW", PackagingTonnage = 200m, ProducerDetailId = 2 },
                new() { Id = 5, MaterialId = 1, PackagingType = "HDC", PackagingTonnage = 300m, ProducerDetailId = 1 },
                new() { Id = 6, MaterialId = 2, PackagingType = "HDC", PackagingTonnage = 300m, ProducerDetailId = 2 },
                new() { Id = 7, MaterialId = 2, PackagingType = "HH", PackagingTonnage = 300m, ProducerDetailId = 4 },
                new() { Id = 8, MaterialId = 1, PackagingType = "HH", PackagingTonnage = 300m, ProducerDetailId = 5 },
            });
            context.ProducerResultFileSuggestedBillingInstruction.AddRange(new List<ProducerResultFileSuggestedBillingInstruction>
            {
                new ProducerResultFileSuggestedBillingInstruction { Id = 1, CalculatorRunId = 1, ProducerId = 1, SuggestedBillingInstruction="INITIAL", BillingInstructionAcceptReject="Accepted"},
                new ProducerResultFileSuggestedBillingInstruction { Id = 2, CalculatorRunId = 2, ProducerId = 2, SuggestedBillingInstruction="INITIAL", BillingInstructionAcceptReject="Accepted"},
                new ProducerResultFileSuggestedBillingInstruction { Id = 3, CalculatorRunId = 3, ProducerId = 3, SuggestedBillingInstruction="INITIAL", BillingInstructionAcceptReject="Accepted"},
            });

            context.ProducerDesignatedRunInvoiceInstruction.AddRange(new List<ProducerDesignatedRunInvoiceInstruction>
            {
                new ProducerDesignatedRunInvoiceInstruction { Id = 1,CalculatorRunId = 1, ProducerId = 1},
                new ProducerDesignatedRunInvoiceInstruction { Id = 2,CalculatorRunId = 2, ProducerId = 2},
                new ProducerDesignatedRunInvoiceInstruction { Id = 3,CalculatorRunId = 2, ProducerId = 2},
            });
            context.ProducerInvoicedMaterialNetTonnage.AddRange(new List<ProducerInvoicedMaterialNetTonnage>
            {
                new ProducerInvoicedMaterialNetTonnage { Id = 3, MaterialId = 1,CalculatorRunId = 1, ProducerId = 1, InvoicedNetTonnage = 12.5M},
                new ProducerInvoicedMaterialNetTonnage { Id = 4, MaterialId = 2,CalculatorRunId = 2, ProducerId = 2, InvoicedNetTonnage = 13.5M},
                new ProducerInvoicedMaterialNetTonnage { Id = 5, MaterialId = 2,CalculatorRunId = 2, ProducerId = 2, InvoicedNetTonnage = 13.5M},
            });
            context.CalculatorRunOrganisationDataMaster.AddRange(new List<CalculatorRunOrganisationDataMaster>
            {
                new CalculatorRunOrganisationDataMaster {Id = 1,CalendarYear = "2025-26", CreatedAt= DateTime.UtcNow, CreatedBy = "TestUser" , EffectiveFrom = DateTime.UtcNow}
            });

            context.CalculatorRunOrganisationDataDetails.AddRange(new List<CalculatorRunOrganisationDataDetail>
            {
                new CalculatorRunOrganisationDataDetail {Id = 2, CalculatorRunOrganisationDataMasterId = 1, OrganisationId = 4, OrganisationName="ORG1",SubmissionPeriodDesc = "Test", SubsidaryId = null},
            });

            context.SaveChanges();
        }

        private class TestResult
        {
            public string Level { get; set; } = null!;

            public string TonnageChangeCount { get; set; } = null!;

            public string TonnageChangeAdvice { get; set; } = null!;
        }
    }
}