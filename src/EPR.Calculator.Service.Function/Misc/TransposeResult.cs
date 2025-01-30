namespace EPR.Calculator.Service.Function.Misc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    // Temp replacement for the IActionResult that the transpose endpoint returns.
    public record TransposeResult
    {
        required public int StatusCode { get; init; }

        public Exception? Exception { get; init; }

        public double? TimeTaken { get; init; }
    }
}
