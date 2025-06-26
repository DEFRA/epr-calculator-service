using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Common.Utils
{
    public static class ResetObjectUtil
    {
        public static void ResetObject(object j)
        {
            try
            {
                if (j == null) return;

                Type type = j.GetType();
                PropertyInfo[] properties = j.GetType().GetProperties();

                foreach (var property in properties)
                {
                    if (!property.CanWrite) continue;
                    if (property.Name == "IsProducerScaledup" && property.GetValue(j).ToString() == "Totals") continue;
                    if ((property.Name == "IsTotalRow" || property.Name == "isOverallTotalRow") && (bool)property.GetValue(j)) continue;

                    Type propType = property.PropertyType;
                    if (propType == typeof(string))
                    { property.SetValue(j, string.Empty); continue; }
                    if (propType == typeof(int)) { property.SetValue(j, 0); continue; }
                    if (propType == typeof(float)) { property.SetValue(j, 0); continue; }
                    if (propType == typeof(double)) { property.SetValue(j, 0); continue; }
                    if (propType == typeof(decimal)) { property.SetValue(j, 0m); continue; }
                    if (propType.IsValueType) { object d = Activator.CreateInstance(propType); property.SetValue(j, d); }

                    else
                    {
                        object c = property.GetValue(j);
                        if (c != null) { ResetObject(c); }
                    }
                }
            }
            catch (Exception ex) { }
        }
    }
}
