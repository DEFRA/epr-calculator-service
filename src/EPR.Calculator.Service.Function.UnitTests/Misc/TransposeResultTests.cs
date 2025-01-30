namespace EPR.Calculator.Service.Function.UnitTests.Misc
{
    using System;
    using AutoFixture;
    using EPR.Calculator.Service.Function.Misc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TransposeResultTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransposeResultTests"/> class.
        /// </summary>
        public TransposeResultTests()
        {
            this.Fixture = new Fixture();

            this.StatusCode = this.Fixture.Create<int>();
            this.Exception = this.Fixture.Create<Exception>();
            this.TimeTaken = this.Fixture.Create<double?>();
            this.TestClass = new TransposeResult
            {
                StatusCode = this.StatusCode,
                Exception = this.Exception,
                TimeTaken = this.TimeTaken,
            };
        }

        private TransposeResult TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private int StatusCode { get; init; }

        private Exception Exception { get; init; }

        private double? TimeTaken { get; init; }

        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new TransposeResult
            {
                StatusCode = this.StatusCode,
                Exception = this.Exception,
                TimeTaken = this.TimeTaken,
            };

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void ImplementsIEquatable_TransposeResult()
        {
            // Arrange
            var same = new TransposeResult
            {
                StatusCode = this.StatusCode,
                Exception = this.Exception,
                TimeTaken = this.TimeTaken,
            };
            var different = this.Fixture.Create<TransposeResult>();

            // Assert
            Assert.IsFalse(this.TestClass.Equals(default(object)));
            Assert.IsFalse(this.TestClass.Equals(new object()));
            Assert.IsTrue(this.TestClass.Equals((object)same));
            Assert.IsFalse(this.TestClass.Equals((object)different));
            Assert.IsTrue(this.TestClass.Equals(same));
            Assert.IsFalse(this.TestClass.Equals(different));
            Assert.AreEqual(same.GetHashCode(), this.TestClass.GetHashCode());
            Assert.AreNotEqual(different.GetHashCode(), this.TestClass.GetHashCode());
            Assert.IsTrue(this.TestClass == same);
            Assert.IsFalse(this.TestClass == different);
            Assert.IsFalse(this.TestClass != same);
            Assert.IsTrue(this.TestClass != different);
        }

        [TestMethod]
        public void StatusCodeIsInitializedCorrectly()
        {
            Assert.AreEqual(this.StatusCode, this.TestClass.StatusCode);
        }

        [TestMethod]
        public void ExceptionIsInitializedCorrectly()
        {
            Assert.AreSame(this.Exception, this.TestClass.Exception);
        }

        [TestMethod]
        public void TimeTakenIsInitializedCorrectly()
        {
            Assert.AreEqual(this.TimeTaken, this.TestClass.TimeTaken);
        }
    }
}