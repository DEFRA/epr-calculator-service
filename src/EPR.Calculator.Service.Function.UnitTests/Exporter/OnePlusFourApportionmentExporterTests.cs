namespace EPR.Calculator.Service.Function.UnitTests.Exporter
{
    using System;
    using System.Text;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OnePlusFourApportionmentExporterTests
    {
        private OnePlusFourApportionmentExporter _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new OnePlusFourApportionmentExporter();
        }
    }
}