namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Metadata.Ecma335;
    using System.Text;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
    using EPR.Calculator.Service.Function.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BillingFileExporterTests
    {
        private BillingFileExporter testClass;
        private Mock<ILateReportingExporter> lateReportingExporter;
        private Mock<ICalcResultDetailExporter> resultDetailexporter;
        private Mock<IOnePlusFourApportionmentExporter> onePlusFourApportionmentExporter;
        private Mock<ICalcResultLaDisposalCostExporter> laDisposalCostExporter;
        private Mock<ICalcResultScaledupProducersExporter> calcResultScaledupProducersExporter;
        private Mock<ILapcaptDetailExporter> lapcaptDetailExporter;
        private Mock<ICalcResultParameterOtherCostExporter> parameterOtherCosts;
        private Mock<ICommsCostExporter> commsCostExporter;
        private Mock<ICalcResultSummaryExporter> calcResultSummaryExporter;

        
        public BillingFileExporterTests()
        {
            lateReportingExporter = new Mock<ILateReportingExporter>();
            resultDetailexporter = new Mock<ICalcResultDetailExporter>();
            onePlusFourApportionmentExporter = new Mock<IOnePlusFourApportionmentExporter>();
            laDisposalCostExporter = new Mock<ICalcResultLaDisposalCostExporter>();
            calcResultScaledupProducersExporter = new Mock<ICalcResultScaledupProducersExporter>();
            lapcaptDetailExporter = new Mock<ILapcaptDetailExporter>();
            parameterOtherCosts = new Mock<ICalcResultParameterOtherCostExporter>();
            commsCostExporter = new Mock<ICommsCostExporter>();
            calcResultSummaryExporter = new Mock<ICalcResultSummaryExporter>();
            testClass = new BillingFileExporter(lateReportingExporter.Object, resultDetailexporter.Object, onePlusFourApportionmentExporter.Object, laDisposalCostExporter.Object, calcResultScaledupProducersExporter.Object, lapcaptDetailExporter.Object, parameterOtherCosts.Object, commsCostExporter.Object, calcResultSummaryExporter.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new BillingFileExporter(lateReportingExporter.Object, resultDetailexporter.Object, onePlusFourApportionmentExporter.Object, laDisposalCostExporter.Object, calcResultScaledupProducersExporter.Object, lapcaptDetailExporter.Object, parameterOtherCosts.Object, commsCostExporter.Object, calcResultSummaryExporter.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

        //[TestMethod]
        //public void CannotConstructWithNullLateReportingExporter()
        //{
        //    Assert.ThrowsException<ArgumentNullException>(() => new BillingFileExporter(default(ILateReportingExporter), _resultDetailexporter.Object, _onePlusFourApportionmentExporter.Object, _laDisposalCostExporter.Object, _calcResultScaledupProducersExporter.Object, _lapcaptDetailExporter.Object, _parameterOtherCosts.Object, _commsCostExporter.Object, _calcResultSummaryExporter.Object));
        //}

        //[TestMethod]
        //public void CannotConstructWithNullResultDetailexporter()
        //{
        //    Assert.ThrowsException<ArgumentNullException>(() => new BillingFileExporter(_lateReportingExporter.Object, default(ICalcResultDetailExporter), _onePlusFourApportionmentExporter.Object, _laDisposalCostExporter.Object, _calcResultScaledupProducersExporter.Object, _lapcaptDetailExporter.Object, _parameterOtherCosts.Object, _commsCostExporter.Object, _calcResultSummaryExporter.Object));
        //}

        //[TestMethod]
        //public void CannotConstructWithNullOnePlusFourApportionmentExporter()
        //{
        //    Assert.ThrowsException<ArgumentNullException>(() => new BillingFileExporter(_lateReportingExporter.Object, _resultDetailexporter.Object, default(IOnePlusFourApportionmentExporter), _laDisposalCostExporter.Object, _calcResultScaledupProducersExporter.Object, _lapcaptDetailExporter.Object, _parameterOtherCosts.Object, _commsCostExporter.Object, _calcResultSummaryExporter.Object));
        //}

        //[TestMethod]
        //public void CannotConstructWithNullLaDisposalCostExporter()
        //{
        //    Assert.ThrowsException<ArgumentNullException>(() => new BillingFileExporter(_lateReportingExporter.Object, _resultDetailexporter.Object, _onePlusFourApportionmentExporter.Object, default(ICalcResultLaDisposalCostExporter), _calcResultScaledupProducersExporter.Object, _lapcaptDetailExporter.Object, _parameterOtherCosts.Object, _commsCostExporter.Object, _calcResultSummaryExporter.Object));
        //}

        //[TestMethod]
        //public void CannotConstructWithNullCalcResultScaledupProducersExporter()
        //{
        //    Assert.ThrowsException<ArgumentNullException>(() => new BillingFileExporter(_lateReportingExporter.Object, _resultDetailexporter.Object, _onePlusFourApportionmentExporter.Object, _laDisposalCostExporter.Object, default(ICalcResultScaledupProducersExporter), _lapcaptDetailExporter.Object, _parameterOtherCosts.Object, _commsCostExporter.Object, _calcResultSummaryExporter.Object));
        //}

        //[TestMethod]
        //public void CannotConstructWithNullLapcaptDetailExporter()
        //{
        //    Assert.ThrowsException<ArgumentNullException>(() => new BillingFileExporter(_lateReportingExporter.Object, _resultDetailexporter.Object, _onePlusFourApportionmentExporter.Object, _laDisposalCostExporter.Object, _calcResultScaledupProducersExporter.Object, default(ILapcaptDetailExporter), _parameterOtherCosts.Object, _commsCostExporter.Object, _calcResultSummaryExporter.Object));
        //}

        //[TestMethod]
        //public void CannotConstructWithNullParameterOtherCosts()
        //{
        //    Assert.ThrowsException<ArgumentNullException>(() => new BillingFileExporter(_lateReportingExporter.Object, _resultDetailexporter.Object, _onePlusFourApportionmentExporter.Object, _laDisposalCostExporter.Object, _calcResultScaledupProducersExporter.Object, _lapcaptDetailExporter.Object, default(ICalcResultParameterOtherCostExporter), _commsCostExporter.Object, _calcResultSummaryExporter.Object));
        //}

        //[TestMethod]
        //public void CannotConstructWithNullCommsCostExporter()
        //{
        //    Assert.ThrowsException<ArgumentNullException>(() => new BillingFileExporter(_lateReportingExporter.Object, _resultDetailexporter.Object, _onePlusFourApportionmentExporter.Object, _laDisposalCostExporter.Object, _calcResultScaledupProducersExporter.Object, _lapcaptDetailExporter.Object, _parameterOtherCosts.Object, default(ICommsCostExporter), _calcResultSummaryExporter.Object));
        //}

        //[TestMethod]
        //public void CannotConstructWithNullCalcResultSummaryExporter()
        //{
        //    Assert.ThrowsException<ArgumentNullException>(() => new BillingFileExporter(_lateReportingExporter.Object, _resultDetailexporter.Object, _onePlusFourApportionmentExporter.Object, _laDisposalCostExporter.Object, _calcResultScaledupProducersExporter.Object, _lapcaptDetailExporter.Object, _parameterOtherCosts.Object, _commsCostExporter.Object, default(ICalcResultSummaryExporter)));
        //}

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var results = fixture.Create<CalcResult>();
            var acceptedProducerIds = fixture.Create<List<int>>();

            resultDetailexporter.Setup(mock => mock.Export(It.IsAny<CalcResultDetail>(), It.IsAny<StringBuilder>())).Verifiable();
            onePlusFourApportionmentExporter.Setup(mock => mock.Export(It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<StringBuilder>())).Verifiable();
            lateReportingExporter.Setup(mock => mock.Export(It.IsAny<CalcResultLateReportingTonnage>())).Returns(fixture.Create<string>());
            calcResultScaledupProducersExporter.Setup(mock => mock.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<StringBuilder>())).Verifiable();
            lapcaptDetailExporter.Setup(mock => mock.Export(It.IsAny<CalcResultLapcapData>(), It.IsAny<StringBuilder>())).Verifiable();
            parameterOtherCosts.Setup(mock => mock.Export(It.IsAny<CalcResultParameterOtherCost>(), It.IsAny<StringBuilder>())).Verifiable();
            calcResultSummaryExporter.Setup(mock => mock.Export(It.IsAny<CalcResultSummary>(), It.IsAny<StringBuilder>())).Verifiable();
            laDisposalCostExporter.Setup(mock => mock.Export(It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<StringBuilder>())).Verifiable();
            commsCostExporter.Setup(mock => mock.Export(It.IsAny<CalcResultCommsCost>(), It.IsAny<StringBuilder>())).Verifiable();

            // Act
            var result = testClass.Export(results, acceptedProducerIds);

            // Assert
            resultDetailexporter.Verify(mock => mock.Export(It.IsAny<CalcResultDetail>(), It.IsAny<StringBuilder>()));
            onePlusFourApportionmentExporter.Verify(mock => mock.Export(It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<StringBuilder>()));
            lateReportingExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLateReportingTonnage>()));
            calcResultScaledupProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<StringBuilder>()));
            lapcaptDetailExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLapcapData>(), It.IsAny<StringBuilder>()));
            parameterOtherCosts.Verify(mock => mock.Export(It.IsAny<CalcResultParameterOtherCost>(), It.IsAny<StringBuilder>()));
            calcResultSummaryExporter.Verify(mock => mock.Export(It.IsAny<CalcResultSummary>(), It.IsAny<StringBuilder>()));
            laDisposalCostExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<StringBuilder>()));
            commsCostExporter.Verify(mock => mock.Export(It.IsAny<CalcResultCommsCost>(), It.IsAny<StringBuilder>()));

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CannotCallExportWithNullResults()
        {
            Assert.ThrowsException<ArgumentNullException>(() => testClass.Export(default(CalcResult), new Mock<IEnumerable<int>>().Object));
        }

        [TestMethod]
        public void CannotCallExportWithNullAcceptedProducerIds()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => testClass.Export(fixture.Create<CalcResult>(), default(IEnumerable<int>)));
        }

        [TestMethod]
        public void CanCallGetScaledUpProducersForExport()
        {
            // Arrange
            var fixture = new Fixture();
            var producers = fixture.Create<CalcResultScaledupProducers>();
            var acceptedProducerIds = fixture.Create<List<int>>();

            // Act
            var result = testClass.GetScaledUpProducersForExport(producers, acceptedProducerIds);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CannotCallGetScaledUpProducersForExportWithNullProducers()
        {
            Assert.ThrowsException<ArgumentNullException>(() => testClass.GetScaledUpProducersForExport(default(CalcResultScaledupProducers), new Mock<IEnumerable<int>>().Object));
        }

        [TestMethod]
        public void CannotCallGetScaledUpProducersForExportWithNullAcceptedProducerIds()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => testClass.GetScaledUpProducersForExport(fixture.Create<CalcResultScaledupProducers>(), default(IEnumerable<int>)));
        }
    }
}