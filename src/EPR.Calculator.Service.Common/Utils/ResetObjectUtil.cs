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

            PropertyInfo[] properties = resetObject.GetType().GetProperties();

            foreach (var property in properties)
            {
                ResetObjectValue(resetObject, property);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Critical Code Smell",
            "S3776:Cognitive Complexity of methods should not be too high",
            Justification = "Temporaraly suppress - will refactor later.")]
        private static void ResetObjectValue(object resetObject, PropertyInfo property)
        {
            if (!property.CanWrite) return;
            else if (property.Name == "IsProducerScaledup" && property.GetValue(resetObject)?.ToString() == "Totals") return;
            else if (property.Name == "IsTotalRow" || property.Name == "isOverallTotalRow") {
            var totalRowValue = (property.GetValue(resetObject));
            if (totalRowValue is not null && (bool)totalRowValue) return;
            }

            Type? propType = property.PropertyType;
            if (propType == typeof(string))
            { 
                property.SetValue(resetObject, string.Empty); return; 
            }
            else if (propType == typeof(int) || propType == typeof(double))
            {
                property.SetValue(resetObject, 0);
                return; 
            }
            else if (propType == typeof(decimal)) { property.SetValue(resetObject, 0m); return; }
            else if (propType is not null && propType.IsValueType)
            {
                object? d = Activator.CreateInstance(propType);
                property.SetValue(resetObject, d);
            }
            else
            {
                object? c = property.GetValue(resetObject);
                if (c != null) { ResetObject(c); }
            }
        }
    }
}
