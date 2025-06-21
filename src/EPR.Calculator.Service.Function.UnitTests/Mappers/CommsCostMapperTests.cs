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
            Assert.IsNotNull(result.ParametersCommsCost);
            Assert.IsNotNull(result.ParametersCommsCost.Percentages);
            Assert.AreEqual(null, result.ParametersCommsCost.Percentages.England);
            Assert.AreEqual(null, result.ParametersCommsCost.Percentages.Wales);
            Assert.AreEqual(null, result.ParametersCommsCost.Percentages.Scotland);
            Assert.AreEqual(null, result.ParametersCommsCost.Percentages.NorthernIreland);
            Assert.AreEqual(null, result.ParametersCommsCost.Percentages.Total);
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
            Assert.IsNotNull(result.ParametersCommsCost);
            Assert.IsNotNull(result.ParametersCommsCost.Percentages);
            Assert.AreEqual("50%", result.ParametersCommsCost.Percentages.England);
            Assert.AreEqual("30%", result.ParametersCommsCost.Percentages.Wales);
            Assert.AreEqual("20%", result.ParametersCommsCost.Percentages.Scotland);
            Assert.AreEqual("10%", result.ParametersCommsCost.Percentages.NorthernIreland);
            Assert.AreEqual("110%", result.ParametersCommsCost.Percentages.Total);
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
                       England = null,
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
            Assert.IsNotNull(result.ParametersCommsCost);
            Assert.IsNotNull(result.ParametersCommsCost.Percentages);
            Assert.AreEqual("0.00%", result.ParametersCommsCost.Percentages.England);
            Assert.AreEqual("30%", result.ParametersCommsCost.Percentages.Wales);
            Assert.AreEqual("20%", result.ParametersCommsCost.Percentages.Scotland);
            Assert.AreEqual("10%", result.ParametersCommsCost.Percentages.NorthernIreland);
            Assert.AreEqual("110%", result.ParametersCommsCost.Percentages.Total);
        }
    }
}
