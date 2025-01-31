namespace EPR.Calculator.Service.Function.UnitTests.Misc
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Misc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LocalDevelopmentConfigurationTests
    {
        private LocalDevelopmentConfiguration TestClass { get; init; }

        private IFixture Fixture { get; init; }

        public LocalDevelopmentConfigurationTests()
        {
            this.Fixture = new Fixture();
            this.TestClass = new LocalDevelopmentConfiguration();
        }

        [TestMethod]
        public void CanGetCheckInterval()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.CheckInterval, typeof(string));
        }

        [TestMethod]
        public void CanGetDbConnectionString()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.DbConnectionString, typeof(string));
        }

        [TestMethod]
        public void CanGetExecuteRPDPipeline()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.ExecuteRPDPipeline, typeof(string));
        }

        [TestMethod]
        public void CanGetMaxCheckCount()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.MaxCheckCount, typeof(string));
        }

        [TestMethod]
        public void CanGetOrgDataPipelineName()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.OrgDataPipelineName, typeof(string));
        }

        [TestMethod]
        public void CanGetPipelineUrl()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.PipelineUrl, typeof(string));
        }

        [TestMethod]
        public void CanGetPomDataPipelineName()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.PomDataPipelineName, typeof(string));
        }

        [TestMethod]
        public void CanGetPrepareCalcResultEndPoint()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.PrepareCalcResultEndPoint, typeof(Uri));
        }

        [TestMethod]
        public void CanGetPrepareCalcResultsTimeout()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.PrepareCalcResultsTimeout, typeof(TimeSpan));
        }

        [TestMethod]
        public void CanGetRpdStatusTimeout()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.RpdStatusTimeout, typeof(TimeSpan));
        }

        [TestMethod]
        public void CanGetStatusEndpoint()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.StatusEndpoint, typeof(Uri));
        }

        [TestMethod]
        public void CanGetTransposeEndpoint()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.TransposeEndpoint, typeof(Uri));
        }

        [TestMethod]
        public void CanGetTransposeTimeout()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.TransposeTimeout, typeof(TimeSpan));
        }
    }
}