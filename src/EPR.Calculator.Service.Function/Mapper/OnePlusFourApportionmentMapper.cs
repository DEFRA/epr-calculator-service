using EPR.Calculator.Service.Function.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.Mapper
{
    internal class OnePlusFourApportionmentMapper : IOnePlusFourApportionmentMapper
    {
        public CalcResultOnePlusFourApportionmentJson Map(CalcResultOnePlusFourApportionment calcResultOnePlusFourApportionment)
        {
            var i = 2;
            return new CalcResultOnePlusFourApportionmentJson()
            {
                OneFeeForLADisposalCosts = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.
            Where(t => t.OrderId == i).
            Select(y => new CalcResultOnePlusFourApportionmentDetailJson
            {
                England = y.EnglandTotal,
                Scotland = y.ScotlandTotal,
                Wales = y.WalesTotal,
                NorthernIreland = y.NorthernIrelandTotal,
                Total = y.Total
            }).SingleOrDefault(),
                FourLADataPrepCharge = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.
            Where(t => t.OrderId == i + 1).
            Select(y => new CalcResultOnePlusFourApportionmentDetailJson
            {
                England = y.EnglandTotal,
                Scotland = y.ScotlandTotal,
                Wales = y.WalesTotal,
                NorthernIreland = y.NorthernIrelandTotal,
                Total = y.Total
            }).SingleOrDefault(),
                TotalOfonePlusFour = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.
            Where(t => t.OrderId == i + 2).
            Select(y => new CalcResultOnePlusFourApportionmentDetailJson
            {
                England = y.EnglandTotal,
                Scotland = y.ScotlandTotal,
                Wales = y.WalesTotal,
                NorthernIreland = y.NorthernIrelandTotal,
                Total = y.Total
            }).SingleOrDefault(),
                OnePlusFourApportionmentPercentages = calcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.
            Where(t => t.OrderId == i + 3).
            Select(y => new CalcResultOnePlusFourApportionmentDetailJson
            {
                England = y.EnglandTotal,
                Scotland = y.ScotlandTotal,
                Wales = y.WalesTotal,
                NorthernIreland = y.NorthernIrelandTotal,
                Total = y.Total
            }).SingleOrDefault()
            };
        }
    }
}
