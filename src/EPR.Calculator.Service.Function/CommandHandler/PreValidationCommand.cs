using EPR.Calculator.API.Data.DataModels;
using System.Collections.Generic;

namespace EPR.Calculator.Service.Function.CommandHandler
{
    public class PreValidationCommand : PreValidationsCommand
    {
        public IEnumerable<CalculatorRunOrganisationDataDetail> RunOrganisations { set; get; } = [];
        public IEnumerable<CalculatorRunPomDataDetail> RunPoms { set; get; } = [];
    }

}
