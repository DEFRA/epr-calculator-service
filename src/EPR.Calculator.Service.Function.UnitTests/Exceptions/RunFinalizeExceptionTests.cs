using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;

namespace EPR.Calculator.Service.Function.UnitTests.Exceptions;

[TestCategory(TestCategories.Core)]
[TestClass]
public class RunFinalizeExceptionTests
{
    [DataRow(RunType.Unknown)]
    [DataRow(RunType.Billing)]
    [DataRow(RunType.Calculator)]
    [TestMethod]
    public void Should_set_RunType(RunType runType)
    {
        var exception = new RunFinalizeException(runType, 1, new Exception());
        exception.RunType.ShouldBe(runType);
    }

    [DataRow(1)]
    [DataRow(0)]
    [DataRow(-1)]
    [TestMethod]
    public void Should_set_RunId(int runId)
    {
        var exception = new RunFinalizeException(RunType.Unknown, runId, new Exception());
        exception.RunId.ShouldBe(runId);
    }

    [DataRow("Invalid data")]
    [DataRow("Missing parameter")]
    [DataRow("")]
    [TestMethod]
    public void Should_set_Message(string message)
    {
        var exception = new RunFinalizeException(RunType.Unknown, 1, new Exception(), message);
        exception.Message.ShouldBe(message);
    }

    [TestMethod]
    public void Should_default_Message()
    {
        var exception = new RunFinalizeException(RunType.Unknown, 1, new Exception());
        exception.Message.ShouldStartWith("Unable to finalize run");
    }

    [TestMethod]
    public void Should_set_InnerException()
    {
        var innerException = new Exception();
        var exception = new RunFinalizeException(RunType.Unknown, 1, innerException);
        exception.InnerException.ShouldBeSameAs(innerException);
    }
}