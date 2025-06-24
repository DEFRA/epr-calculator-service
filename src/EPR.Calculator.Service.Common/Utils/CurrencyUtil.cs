using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Common.Utils
{
    public static class CurrencyUtil
    {
        public static string ConvertToCurrency(decimal detail)
        {
            if (detail == 0)
            {
                return string.Empty;
            }
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            return detail.ToString("C", culture);
        }
    }
}
