namespace EPR.Calculator.Service.Function.UnitTests.Converter
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Converter;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class DecimalPrecisionConverterTests
    {
        [TestMethod]
        public void WriteJson_RoundDemcimal()
        {
            // Act
            var converter = new DecimalPrecisionConverter(3);
            var val = 123.9879908m;
            var stringWriter =  new StringWriter();
            var jsonWriter = new JsonTextWriter(stringWriter);

            converter.WriteJson(jsonWriter, val, new JsonSerializer());
            jsonWriter.Flush();
            var result = stringWriter.ToString();

            // Assert
            Assert.AreEqual("123.988",result);
        }

        [TestMethod]
        public void ReadJson_RoundDemcimal()
        {
            // Act
            var converter = new DecimalPrecisionConverter(3);
            var json = "\"123.988\"";
            var stringReader = new StringReader(json);
            var jsonReader = new JsonTextReader(stringReader);

            jsonReader.Read();
            var result = converter.ReadJson(jsonReader, typeof(decimal), null, new JsonSerializer());

            // Assert
            Assert.AreEqual(123.988m, result);
        }
       
        [TestMethod]
        public void CanConvertReturnsFalse()
        {
            // Act
            var converter = new DecimalPrecisionConverter(3);

            Assert.IsFalse(converter.CanConvert(typeof(string)));
        }
    }
}