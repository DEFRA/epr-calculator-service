using AutoFixture;
using EPR.Calculator.Service.Function.Constants;

namespace EPR.Calculator.Service.Function.UnitTests
{
    /// <summary>
    /// Contains tests for configuration variables.
    /// </summary>
    [TestClass]
    public class ConfigurationTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationTests"/> class.
        /// </summary>
        public ConfigurationTests() => Fixture = new Fixture();

        private Fixture Fixture { get; init; }

        /// <summary>
        /// Checks that the calculator run timeout can be retrieved.
        /// </summary>
        [TestMethod]
        public void CanGetPrepareCalcResultsTimeout()
        {
            // Arrange
            var testValueInMinutes = Fixture.Create<double>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.PrepareCalcResultsTimeout,
                testValueInMinutes.ToString());

            // Act
            var result = new Configuration().PrepareCalcResultsTimeout;

            // Assert
            Assert.AreEqual(TimeSpan.FromMinutes(testValueInMinutes), result);
        }

        /// <summary>
        /// Checks that when no value has been set for the calculator run timeout,
        /// the default value is retrieved.
        /// </summary>
        [TestMethod]
        public void CanGetDefaultPrepareCalcResultsTimeout()
        {
            // Arrange
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.PrepareCalcResultsTimeout,
                null);

            // Act
            var result = new Configuration().PrepareCalcResultsTimeout;

            // Assert
            Assert.AreEqual(TimeSpan.FromHours(Configuration.DefaultTimeout), result);
        }

        /// <summary>
        /// Checks that the rpd status timeout can be retrieved.
        /// </summary>
        [TestMethod]
        public void CanGetRpdStatusTimeout()
        {
            // Arrange
            var testValueInMinutes = Fixture.Create<double>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.RpdStatusTimeout,
                testValueInMinutes.ToString());

            // Act
            var result = new Configuration().RpdStatusTimeout;

            // Assert
            Assert.AreEqual(TimeSpan.FromMinutes(testValueInMinutes), result);
        }

        /// <summary>
        /// Checks that when no value has been set for the rpd status timeout,
        /// the default value is retrieved.
        /// </summary>
        [TestMethod]
        public void CanGetDefaultRpdStatusTimeout()
        {
            // Arrange
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.RpdStatusTimeout,
                null);

            // Act
            var result = new Configuration().RpdStatusTimeout;

            // Assert
            Assert.AreEqual(TimeSpan.FromHours(Configuration.DefaultTimeout), result);
        }

        /// <summary>
        /// Checks that the calculator run timeout can be retrieved.
        /// </summary>
        [TestMethod]
        public void CanGetTransposeTimeout()
        {
            // Arrange
            var testValueInMinutes = Fixture.Create<double>();
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.TransposeTimeout,
                testValueInMinutes.ToString());

            // Act
            var result = new Configuration().TransposeTimeout;

            // Assert
            Assert.AreEqual(TimeSpan.FromMinutes(testValueInMinutes), result);
        }

        /// <summary>
        /// Checks that when no value has been set for the calculator run timeout,
        /// the default value is retrieved.
        /// </summary>
        [TestMethod]
        public void CanGetDefaultTransposeTimeout()
        {
            // Arrange
            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.TransposeTimeout,
                null);

            // Act
            var result = new Configuration().TransposeTimeout;

            // Assert
            Assert.AreEqual(TimeSpan.FromHours(Configuration.DefaultTimeout), result);
        }

        [TestMethod]
        public void CanGetDbConnectionString()
        {
            // Arrange
            var connectionString = Fixture.Create<string>();

            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.DbConnectionString,
                connectionString);

            // Act
            var result = new Configuration().DbConnectionString;

            // Assert
            Assert.AreEqual(connectionString, result);
        }

        [TestMethod]
        public void CanGetTransposeEndpoint()
        {
            // Arrange
            var transposeEndpoint = Fixture.Create<Uri>();

            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.TransposeEndpoint,
                transposeEndpoint.ToString());

            // Act
            var result = new Configuration().TransposeEndpoint;

            // Assert
            Assert.AreEqual(transposeEndpoint, result);
        }

        [TestMethod]
        public void CanGetDbLoadingChunkSize()
        {
            // Arrange
            var dbLoadingChunkSize = Fixture.Create<int>();

            Environment.SetEnvironmentVariable(
               EnvironmentVariableKeys.DbLoadingChunkSize,
               dbLoadingChunkSize.ToString());

            // Act
            var result = new Configuration().DbLoadingChunkSize;

            // Assert
            Assert.AreEqual(dbLoadingChunkSize, result);
        }

        [TestMethod]
        public void CanGetResultFileCSVContainerName()
        {
            // Arrange
            var resultFileCSVContainerName = Fixture.Create<string>();

            Environment.SetEnvironmentVariable(
               EnvironmentVariableKeys.ResultFileCSVContainerName,
               resultFileCSVContainerName);

            // Act
            var result = new Configuration().ResultFileCSVContainerName;

            // Assert
            Assert.AreEqual(resultFileCSVContainerName, result);
        }

        [TestMethod]
        public void BillingFileCSVBlobContainerName()
        {
            // Arrange
            var resultFileCSVContainerName = Fixture.Create<string>();

            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.BillingFileCSVBlobContainerName,
                resultFileCSVContainerName);

            // Act
            var result = new Configuration().BillingFileCSVBlobContainerName;

            // Assert
            Assert.AreEqual(resultFileCSVContainerName, result);
        }

        [TestMethod]
        public void BillingFileJsonBlobContainerName()
        {
            // Arrange
            var resultFileCSVContainerName = Fixture.Create<string>();

            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.BillingFileJsonBlobContainerName,
                resultFileCSVContainerName);

            // Act
            var result = new Configuration().BillingFileJsonBlobContainerName;

            // Assert
            Assert.AreEqual(resultFileCSVContainerName, result);
        }

        [TestMethod]
        public void BlobConnectionString()
        {
            // Arrange
            var resultFileCSVContainerName = Fixture.Create<string>();

            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.BlobConnectionString,
                resultFileCSVContainerName);

            // Act
            var result = new Configuration().BlobConnectionString;

            // Assert
            Assert.AreEqual(resultFileCSVContainerName, result);
        }

        [TestMethod]
        public void InstrumentationKey()
        {
            // Arrange
            var resultFileCSVContainerName = Fixture.Create<string>();

            Environment.SetEnvironmentVariable(
                EnvironmentVariableKeys.InstrumentationKey,
                resultFileCSVContainerName);

            // Act
            var result = new Configuration().InstrumentationKey;

            // Assert
            Assert.AreEqual(resultFileCSVContainerName, result);
        }

    }
}