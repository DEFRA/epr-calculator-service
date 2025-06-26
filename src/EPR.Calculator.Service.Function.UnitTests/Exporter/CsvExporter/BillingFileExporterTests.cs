namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter;
    using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
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
        private Mock<ILateReportingExporter> _lateReportingExporter;
        private Mock<ICalcResultDetailExporter> _resultDetailexporter;
        private Mock<IOnePlusFourApportionmentExporter> _onePlusFourApportionmentExporter;
        private Mock<ICalcResultLaDisposalCostExporter> _laDisposalCostExporter;
        private Mock<ICalcResultScaledupProducersExporter> _calcResultScaledupProducersExporter;
        private Mock<ILapcaptDetailExporter> _lapcaptDetailExporter;
        private Mock<ICalcResultParameterOtherCostExporter> _parameterOtherCosts;
        private Mock<ICommsCostExporter> _commsCostExporter;
        private Mock<ICalcResultSummaryExporter> _calcResultSummaryExporter;
        private Mock<ICalcResultCancelledProducersExporter> _calcResultCancelledProducersExporter;

        
        public BillingFileExporterTests()
        {
            _lateReportingExporter = new Mock<ILateReportingExporter>();
            _resultDetailexporter = new Mock<ICalcResultDetailExporter>();
            _onePlusFourApportionmentExporter = new Mock<IOnePlusFourApportionmentExporter>();
            _laDisposalCostExporter = new Mock<ICalcResultLaDisposalCostExporter>();
            _calcResultScaledupProducersExporter = new Mock<ICalcResultScaledupProducersExporter>();
            _lapcaptDetailExporter = new Mock<ILapcaptDetailExporter>();
            _parameterOtherCosts = new Mock<ICalcResultParameterOtherCostExporter>();
            _commsCostExporter = new Mock<ICommsCostExporter>();
            _calcResultSummaryExporter = new Mock<ICalcResultSummaryExporter>();
            _calcResultCancelledProducersExporter = new Mock<ICalcResultCancelledProducersExporter>();
            testClass = new BillingFileExporter(_lateReportingExporter.Object, _resultDetailexporter.Object, _onePlusFourApportionmentExporter.Object, _laDisposalCostExporter.Object, _calcResultScaledupProducersExporter.Object, _lapcaptDetailExporter.Object, _parameterOtherCosts.Object, _commsCostExporter.Object, _calcResultSummaryExporter.Object, _calcResultCancelledProducersExporter.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new BillingFileExporter(_lateReportingExporter.Object, _resultDetailexporter.Object, _onePlusFourApportionmentExporter.Object, _laDisposalCostExporter.Object, _calcResultScaledupProducersExporter.Object, _lapcaptDetailExporter.Object, _parameterOtherCosts.Object, _commsCostExporter.Object, _calcResultSummaryExporter.Object, _calcResultCancelledProducersExporter.Object);

            // Assert
            Assert.IsNotNull(instance);
        }       

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture();
            var results = fixture.Create<CalcResult>();
            var acceptedProducerIds = results.CalcResultScaledupProducers?.ScaledupProducers?.Select(t => t.ProducerId).Take(1).ToList() ?? fixture.Create<List<int>>();

            _resultDetailexporter.Setup(mock => mock.Export(It.IsAny<CalcResultDetail>(), It.IsAny<StringBuilder>())).Verifiable();
            _onePlusFourApportionmentExporter.Setup(mock => mock.Export(It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<StringBuilder>())).Verifiable();
            _lateReportingExporter.Setup(mock => mock.Export(It.IsAny<CalcResultLateReportingTonnage>())).Returns(fixture.Create<string>());
            _calcResultScaledupProducersExporter.Setup(mock => mock.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<StringBuilder>())).Verifiable();
            _lapcaptDetailExporter.Setup(mock => mock.Export(It.IsAny<CalcResultLapcapData>(), It.IsAny<StringBuilder>())).Verifiable();
            _parameterOtherCosts.Setup(mock => mock.Export(It.IsAny<CalcResultParameterOtherCost>(), It.IsAny<StringBuilder>())).Verifiable();
            _calcResultSummaryExporter.Setup(mock => mock.Export(It.IsAny<CalcResultSummary>(), It.IsAny<StringBuilder>())).Verifiable();
            _laDisposalCostExporter.Setup(mock => mock.Export(It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<StringBuilder>())).Verifiable();
            _commsCostExporter.Setup(mock => mock.Export(It.IsAny<CalcResultCommsCost>(), It.IsAny<StringBuilder>())).Verifiable();
            _calcResultCancelledProducersExporter.Setup(mock => mock.Export(It.IsAny<CalcResultCancelledProducersResponse>(), It.IsAny<StringBuilder>())).Verifiable();

            // Act
            var result = this.testClass.Export(results, acceptedProducerIds);

            // Assert
            _resultDetailexporter.Verify(mock => mock.Export(It.IsAny<CalcResultDetail>(), It.IsAny<StringBuilder>()));
            _onePlusFourApportionmentExporter.Verify(mock => mock.Export(It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<StringBuilder>()));
            _lateReportingExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLateReportingTonnage>()));
            _calcResultScaledupProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<StringBuilder>()));
            _lapcaptDetailExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLapcapData>(), It.IsAny<StringBuilder>()));
            _parameterOtherCosts.Verify(mock => mock.Export(It.IsAny<CalcResultParameterOtherCost>(), It.IsAny<StringBuilder>()));
            _calcResultSummaryExporter.Verify(mock => mock.Export(It.IsAny<CalcResultSummary>(), It.IsAny<StringBuilder>()));
            _laDisposalCostExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<StringBuilder>()));
            _commsCostExporter.Verify(mock => mock.Export(It.IsAny<CalcResultCommsCost>(), It.IsAny<StringBuilder>()));
            _calcResultCancelledProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultCancelledProducersResponse>(), It.IsAny<StringBuilder>()));

            Assert.IsNotNull(result);
        }

        
        [TestMethod]
        public void CanCallGetScaledUpProducersForExport()
        {
            // Arrange
            var fixture = new Fixture();
            var producers = fixture.Create<CalcResultScaledupProducers>();
            var acceptedProducerIds = producers.ScaledupProducers?.Select(t => t.ProducerId).Take(2).ToList() ?? fixture.Create<List<int>>();

            // Act
            var result = this.testClass.GetScaledUpProducersForExport(producers, acceptedProducerIds);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ScaledupProducers?.Count() > 0);
        }       
       
    }
}