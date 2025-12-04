using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Service.Common.Logging
{
    public interface IConsoleWrapper
    {
        void WriteLine(string message);
    }
    
    [ExcludeFromCodeCoverage]
    public class ConsoleWrapper : IConsoleWrapper
    {
        public void WriteLine(string message) => Console.WriteLine(message);
    }
}