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
    public interface ICalcResult2ACommsDataByMaterialMapper
    {
       public CalcResult2ACommsDataByMaterial Map(List<CalcResultCommsCostCommsCostByMaterial> commsCostByMaterial);

    }
}
