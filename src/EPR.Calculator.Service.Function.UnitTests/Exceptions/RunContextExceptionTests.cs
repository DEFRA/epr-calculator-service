using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;

namespace EPR.Calculator.Service.Function.UnitTests.Exceptions;

[TestCategory(TestCategories.Core)]
[TestClass]
public class RunContextExceptionTests
{
    [DataRow(RunType.Unknown)]
    [DataRow(RunType.Billing)]
    [DataRow(RunType.Calculator)]
    [TestMethod]
    public void Should_set_RunType(RunType runType)
    {
        var exception = new RunContextException(runType, 1, "");
        exception.RunType.ShouldBe(runType);
    }

    [DataRow(1)]
    [DataRow(0)]
    [DataRow(-1)]
    [TestMethod]
    public void Should_set_RunId(int runId)
    {
        var exception = new RunContextException(RunType.Unknown, runId, "");
        exception.RunId.ShouldBe(runId);
    }

    [DataRow("Invalid data")]
    [DataRow("Missing parameter")]
    [DataRow("")]
    [TestMethod]
    public void Should_set_Message(string message)
    {
        var exception = new RunContextException(RunType.Unknown, 1, message);
        exception.Message.ShouldBe(message);
    }
}