namespace EPR.Calculator.Service.Function.UnitTests.Mapper
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Mapper;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OnePlusFourApportionmentMapperTests
    {
        private OnePlusFourApportionmentMapper _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new OnePlusFourApportionmentMapper();
        }

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment()
            {
                Name = "Test",
                CalcResultOnePlusFourApportionmentDetails =
                    [
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                            OrderId=2
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                            OrderId=3
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                            OrderId=4
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=14.53M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                     new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=14.53M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=0.20M,
                            Name="Test",
                            OrderId=5
                        }]
            };
            // Act
            var result = _testClass.Map(calcResultOnePlusFourApportionment);

            // Assert
            Assert.IsNotNull(result); 
            Assert.IsNotNull(result.TotalOfonePlusFour);
            Assert.IsNotNull(result.OnePlusFourApportionmentPercentages);
            Assert.IsNotNull(result.FourLADataPrepCharge);
            Assert.IsNotNull(result.OnePlusFourApportionmentPercentages);
        }

        [TestMethod]
        public void Map_ShouldMapCorrectly_WhenValidInputIsProvided()
        {
            // Arrange  
            var input = new CalcResultOnePlusFourApportionment
            {
                CalcResultOnePlusFourApportionmentDetails = new List<CalcResultOnePlusFourApportionmentDetail>
               {
                   new CalcResultOnePlusFourApportionmentDetail
                   {
                       OrderId = 1,
                       Name = "Test1",
                       EnglandTotal = 100,
                       ScotlandTotal = 200,
                       WalesTotal = 300,
                       NorthernIrelandTotal = 400,
                       Total = "1000",
                       EnglandDisposalTotal = "80",
                       ScotlandDisposalTotal = "30",
                       WalesDisposalTotal = "20",
                       NorthernIrelandDisposalTotal = "70",
                   },
                   new CalcResultOnePlusFourApportionmentDetail
                   {
                       OrderId = 2,
                       Name = "Test2",
                       EnglandTotal = 150,
                       ScotlandTotal = 250,
                       WalesTotal = 350,
                       NorthernIrelandTotal = 450,
                       Total = "1200",
                       EnglandDisposalTotal = "80",
                       ScotlandDisposalTotal = "30",
                       WalesDisposalTotal = "20",
                       NorthernIrelandDisposalTotal = "70",
                   },
                   new CalcResultOnePlusFourApportionmentDetail
                   {
                       OrderId = 3,
                       Name = "Test3",
                       EnglandTotal = 200,
                       ScotlandTotal = 300,
                       WalesTotal = 400,
                       NorthernIrelandTotal = 500,
                       Total = "1400",
                       EnglandDisposalTotal = "80",
                       ScotlandDisposalTotal = "30",
                       WalesDisposalTotal = "20",
                       NorthernIrelandDisposalTotal = "70",
                   },
                   new CalcResultOnePlusFourApportionmentDetail
                   {
                       OrderId = 4,
                       Name = "Test4",
                       EnglandTotal = 0.25m,
                       ScotlandTotal = 0.35m,
                       WalesTotal = 0.15m,
                       NorthernIrelandTotal = 0.25m,
                       Total = "1.00",
                       EnglandDisposalTotal = "80",
                       ScotlandDisposalTotal = "30",
                       WalesDisposalTotal = "20",
                       NorthernIrelandDisposalTotal = "70",
                   }
               },
                Name = "Test One Plus Four Apportionment"
            };

            // Act  
            var result = _testClass.Map(input);

            // Assert  
            Assert.IsNotNull(result);
            Assert.AreEqual("£100.00", result.OneFeeForLADisposalCosts.England);
            Assert.AreEqual("£200.00", result.OneFeeForLADisposalCosts.Scotland);
            Assert.AreEqual("£300.00", result.OneFeeForLADisposalCosts.Wales);
            Assert.AreEqual("£400.00", result.OneFeeForLADisposalCosts.NorthernIreland);
            Assert.AreEqual("1000", result.OneFeeForLADisposalCosts.Total);

            Assert.AreEqual("£150.00", result.FourLADataPrepCharge.England);
            Assert.AreEqual("£250.00", result.FourLADataPrepCharge.Scotland);
            Assert.AreEqual("£350.00", result.FourLADataPrepCharge.Wales);
            Assert.AreEqual("£450.00", result.FourLADataPrepCharge.NorthernIreland);
            Assert.AreEqual("1200", result.FourLADataPrepCharge.Total);

            Assert.AreEqual("£200.00", result.TotalOfonePlusFour.England);
            Assert.AreEqual("£300.00", result.TotalOfonePlusFour.Scotland);
            Assert.AreEqual("£400.00", result.TotalOfonePlusFour.Wales);
            Assert.AreEqual("£500.00", result.TotalOfonePlusFour.NorthernIreland);
            Assert.AreEqual("1400", result.TotalOfonePlusFour.Total);

            Assert.AreEqual("0.25%", result.OnePlusFourApportionmentPercentages.England);
            Assert.AreEqual("0.35%", result.OnePlusFourApportionmentPercentages.Scotland);
            Assert.AreEqual("0.15%", result.OnePlusFourApportionmentPercentages.Wales);
            Assert.AreEqual("0.25%", result.OnePlusFourApportionmentPercentages.NorthernIreland);
            Assert.AreEqual("1.00", result.OnePlusFourApportionmentPercentages.Total);
        }
    }
}