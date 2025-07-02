using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Mapper;
using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.UnitTests
{
    [TestClass]
    public class CommsCostMapperTests
    {
        private CommsCostMapper _mapper;

        public CommsCostMapperTests()
        {
            _mapper = new CommsCostMapper();
        }

        [TestMethod]
        public void Map_ShouldReturnDefaultJson_WhenDataRowIsNull()
        {
            // Arrange  
            var calcResultCommsCost = new CalcResultCommsCost
            {
                CalcResultCommsCostOnePlusFourApportionment = new List<CalcResultCommsCostOnePlusFourApportionment>()
            };

            // Act  
            var result = _mapper.Map(calcResultCommsCost);

            // Assert  
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.OnePlusFourCommsCostApportionmentPercentages);
            Assert.AreEqual(null, result.OnePlusFourCommsCostApportionmentPercentages.England);
            Assert.AreEqual(null, result.OnePlusFourCommsCostApportionmentPercentages.Wales);
            Assert.AreEqual(null, result.OnePlusFourCommsCostApportionmentPercentages.Scotland);
            Assert.AreEqual(null, result.OnePlusFourCommsCostApportionmentPercentages.NorthernIreland);
            Assert.AreEqual(null, result.OnePlusFourCommsCostApportionmentPercentages.Total);
        }

        [TestMethod]
        public void Map_ShouldMapPercentagesCorrectly_WhenDataRowIsPresent()
        {
            // Arrange  
            var calcResultCommsCost = new CalcResultCommsCost
            {
                CalcResultCommsCostOnePlusFourApportionment = new List<CalcResultCommsCostOnePlusFourApportionment>
               {
                   new CalcResultCommsCostOnePlusFourApportionment(),
                   new CalcResultCommsCostOnePlusFourApportionment
                   {
                       Name = CalcResultCommsCostBuilder.OnePlusFourApportionment,
                       England = "50",
                       Wales = "30%",
                       Scotland = "20",
                       NorthernIreland = "10%",
                       Total = "110"
                   }
               }
            };

            // Act  
            var result = _mapper.Map(calcResultCommsCost);

            // Assert  
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.OnePlusFourCommsCostApportionmentPercentages);
            Assert.AreEqual("50%", result.OnePlusFourCommsCostApportionmentPercentages.England);
            Assert.AreEqual("30%", result.OnePlusFourCommsCostApportionmentPercentages.Wales);
            Assert.AreEqual("20%", result.OnePlusFourCommsCostApportionmentPercentages.Scotland);
            Assert.AreEqual("10%", result.OnePlusFourCommsCostApportionmentPercentages.NorthernIreland);
            Assert.AreEqual("110%", result.OnePlusFourCommsCostApportionmentPercentages.Total);
        }

        [TestMethod]
        public void Map_ShouldHandleEmptyOrNullInput_WhenDataRowIsPresent()
        {
            // Arrange  
            var calcResultCommsCost = new CalcResultCommsCost
            {
                CalcResultCommsCostOnePlusFourApportionment = new List<CalcResultCommsCostOnePlusFourApportionment>
               {
                   new CalcResultCommsCostOnePlusFourApportionment(),
                   new CalcResultCommsCostOnePlusFourApportionment
                   {
                       Name = CalcResultCommsCostBuilder.OnePlusFourApportionment,
                       England = string.Empty,
                       Wales = "30%",
                       Scotland = "20",
                       NorthernIreland = "10%",
                       Total = "110"
                   }
               }
            };

            // Act  
            var result = _mapper.Map(calcResultCommsCost);

            // Assert  
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.OnePlusFourCommsCostApportionmentPercentages);
            Assert.AreEqual("0.00%", result.OnePlusFourCommsCostApportionmentPercentages.England);
            Assert.AreEqual("30%", result.OnePlusFourCommsCostApportionmentPercentages.Wales);
            Assert.AreEqual("20%", result.OnePlusFourCommsCostApportionmentPercentages.Scotland);
            Assert.AreEqual("10%", result.OnePlusFourCommsCostApportionmentPercentages.NorthernIreland);
            Assert.AreEqual("110%", result.OnePlusFourCommsCostApportionmentPercentages.Total);
        }
    }
}
