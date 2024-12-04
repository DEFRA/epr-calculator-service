// <copyright file="ConfigurationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace EPR.Calculator.Service.Function.UnitTests
{
    /// <summary>
    /// Contains tests for configuration variables.
    /// </summary>
    [TestClass]
    public class ConfigurationTests
    {
        /// <summary>
        /// Tests that the configuration variables match the expected environment variables.
        /// </summary>
        [TestMethod]
        public void Configuration_Variables_Test()
        {
            // Assert
            Assert.AreEqual(Environment.GetEnvironmentVariable("PipelineUrl"), Configuration.PipelineUrl);
            Assert.AreEqual(Environment.GetEnvironmentVariable("GetOrgDataPipelineName"), Configuration.OrgDataPipelineName);
            Assert.AreEqual(Environment.GetEnvironmentVariable("GetPomDataPipelineName"), Configuration.PomDataPipelineName);
            Assert.AreEqual(Environment.GetEnvironmentVariable("CheckInterval"), Configuration.CheckInterval);
            Assert.AreEqual(Environment.GetEnvironmentVariable("MaxCheckCount"), Configuration.MaxCheckCount);
            Assert.AreEqual(Environment.GetEnvironmentVariable("ExecuteRPDPipeline"), Configuration.ExecuteRPDPipeline);
        }
    }

}