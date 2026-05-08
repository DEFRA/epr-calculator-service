namespace EPR.Calculator.Service.Function.Rules;

public static class Rounding
{
    public static decimal KilogramsToTonnes(double weight)
    {
        return Math.Round((decimal)weight / 1000, 3);
    }
}