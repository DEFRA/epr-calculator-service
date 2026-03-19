using AutoFixture;
using EPR.Calculator.Service.Function.Constants;

namespace EPR.Calculator.Service.Function.UnitTests.Constants
{
    [TestClass]
    public class TimeoutPoliciesTests
    {
        private IFixture Fixture = new Fixture();

        [TestMethod]
        public void CanGetAllPolicies()
        {
            // Assert
            Assert.IsInstanceOfType(TimeoutPolicies.AllPolicies, typeof(string[]));

            Assert.IsTrue(TimeoutPolicies.AllPolicies.Contains(TimeoutPolicies.RpdStatus));
            Assert.IsTrue(TimeoutPolicies.AllPolicies.Contains(TimeoutPolicies.Transpose));
            Assert.IsTrue(TimeoutPolicies.AllPolicies.Contains(TimeoutPolicies.PrepareCalcResults));
            Assert.IsTrue(TimeoutPolicies.AllPolicies.Length == 3);
        }
    }
}