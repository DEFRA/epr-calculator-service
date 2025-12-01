using System.Runtime.Serialization;

namespace EPR.Calculator.Service.Function.Services
{
    public static class ObligationStates
    {
        public const string Yes = "Y";
        public const string No = "N";
        public const string Error = "E";

        public static bool IsObligated(string status)
        {
            return string.IsNullOrWhiteSpace(status) || status == ObligationStates.Yes;
        }
    }
}