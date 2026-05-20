namespace EPR.Calculator.Service.Function.Models;

// TODO reuse ByCountryApportionment
public class CalcResultCommsCostOnePlusFourApportionment
{
    public decimal EnglandCost { get; set; }
    public decimal WalesCost { get; set; }
    public decimal ScotlandCost { get; set; }
    public decimal NorthernIrelandCost { get; set; }

    private decimal? totalCost;
    public decimal TotalCost =>
        totalCost ??=
            EnglandCost + WalesCost + ScotlandCost + NorthernIrelandCost;
}
