using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;

namespace EPR.Calculator.Service.Function.Mapper
{
    public interface ICalcResult2aCommsDataByMaterialMapper
    {
       public CalcResult2aCommsDataByMaterial Map(List<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial);

    }
}
