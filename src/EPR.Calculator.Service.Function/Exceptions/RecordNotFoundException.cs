using System;

namespace EPR.Calculator.Service.Function.Exceptions
{
    public class RecordNotFoundException : Exception
    {
        public RecordNotFoundException(string message): base(message) {
        }
    }
}
