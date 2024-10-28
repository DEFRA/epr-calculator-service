// <copyright file="ConfigurationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace EPR.Calculator.Service.Function.UnitTests
{
    [TestClass]
    public class ConfigurationTests
    {
        [TestMethod]
        public void Configuration_Vraiables_Test()
        {
            // Assert
            Assert.AreEqual(Configuration.PipelineUrl, Environment.GetEnvironmentVariable("PipelineUrl"));
            Assert.AreEqual(Configuration.GetOrgDataPipelineName, Environment.GetEnvironmentVariable("GetOrgDataPipelineName"));
            Assert.AreEqual(Configuration.GetPomDataPipelineName, Environment.GetEnvironmentVariable("GetPomDataPipelineName"));
            Assert.AreEqual(Configuration.CheckInterval, Environment.GetEnvironmentVariable("CheckInterval"));
            Assert.AreEqual(Configuration.MaxCheckCount, Environment.GetEnvironmentVariable("MaxCheckCount"));
        }
    }
}