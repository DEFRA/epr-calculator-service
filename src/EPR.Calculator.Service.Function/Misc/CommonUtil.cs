namespace EPR.Calculator.Service.Function.Misc
{
    using System;

    public static class CommonUtil
    {
        public static decimal ConvertKilogramToTonne(double weight)
        {
            return Math.Round((decimal)weight / 1000, 3);
        }
    }
}
