namespace EPR.Calculator.Service.Function.Models;

public class CalcResultCommsCostOnePlusFourApportionment
{
    public string Name { get; set; } = string.Empty;
    public decimal England { get; set; }
    public decimal Scotland { get; set; }
    public decimal NorthernIreland { get; set; }
    public decimal Wales { get; set; }
    public decimal Total { get; set; }
    public int OrderId { get; set; } // TODO remove this?
}
