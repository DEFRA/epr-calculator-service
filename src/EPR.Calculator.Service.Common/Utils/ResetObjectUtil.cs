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
        public static void ResetObject(object resetObject)
        {
            if (resetObject == null) return;

            Type type = resetObject.GetType();
            PropertyInfo[] properties = resetObject.GetType().GetProperties();

            foreach (var property in properties)
            {
                if (!property.CanWrite) continue;
                if (property.Name == "IsProducerScaledup" && property.GetValue(resetObject).ToString() == "Totals") continue;
                if ((property.Name == "IsTotalRow" || property.Name == "isOverallTotalRow") && (bool)property.GetValue(resetObject)) continue;

                Type propType = property.PropertyType;
                if (propType == typeof(string))
                { property.SetValue(resetObject, string.Empty); continue; }
                if (propType == typeof(int)) { property.SetValue(resetObject, 0); continue; }
                if (propType == typeof(float)) { property.SetValue(resetObject, 0); continue; }
                if (propType == typeof(double)) { property.SetValue(resetObject, 0); continue; }
                if (propType == typeof(decimal)) { property.SetValue(resetObject, 0m); continue; }
                if (propType.IsValueType) { object d = Activator.CreateInstance(propType); property.SetValue(resetObject, d); }

                else
                {
                    object c = property.GetValue(resetObject);
                    if (c != null) { ResetObject(c); }
                }
            }
        }
    }
}
