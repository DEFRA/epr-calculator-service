using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Interface;
using EPR.Calculator.Service.Function.Models;
using Microsoft.AspNetCore.Http;

namespace EPR.Calculator.Service.Function.Misc
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RpdStatusDataValidator"/> class.
    /// </summary>
    public class RpdStatusDataValidator(IOrgAndPomWrapper wrapper) : IRpdStatusDataValidator
    {
        private readonly IOrgAndPomWrapper wrapper = wrapper;

        public RpdStatusValidation IsValidRun(CalculatorRun? calcRun, int runId)
        {
            if (calcRun == null)
            {
                return new RpdStatusValidation
                {
                    isValid = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    ErrorMessage = $"Calculator Run {runId} is missing",
                };
            }

            if (calcRun.CalculatorRunOrganisationDataMasterId != null)
            {
                return new RpdStatusValidation
                {
                    isValid = false,
                    StatusCode = StatusCodes.Status422UnprocessableEntity,
                    ErrorMessage = $"Calculator Run {runId} already has OrganisationDataMasterId associated with it",
                };
            }

            if (calcRun.CalculatorRunPomDataMasterId != null)
            {
                return new RpdStatusValidation
                {
                    isValid = false,
                    StatusCode = StatusCodes.Status422UnprocessableEntity,
                    ErrorMessage = $"Calculator Run {runId} already has PomDataMasterId associated with it",
                };
            }

            if (calcRun.Classification is not (RunClassification.None or RunClassification.Running) )
            {
                return new RpdStatusValidation
                {
                    isValid = false,
                    StatusCode = StatusCodes.Status422UnprocessableEntity,
                    ErrorMessage = $"Calculator Run {runId} classification should be RUNNING or IN THE QUEUE",
                };
            }

            return new RpdStatusValidation
            {
                isValid = true,
            };
        }

        public RpdStatusValidation IsValidSuccessfulRun(int runId)
        {
            var pomDataExists = wrapper.AnyPomData();
            var organisationDataExists = wrapper.AnyOrganisationData();
            if (!pomDataExists || !organisationDataExists)
            {
                return new RpdStatusValidation
                {
                    isValid = false,
                    StatusCode = StatusCodes.Status422UnprocessableEntity,
                    ErrorMessage = "PomData or Organisation Data is missing",
                };
            }

            return new RpdStatusValidation
            {
                isValid = true,
            };
        }
    }
}
