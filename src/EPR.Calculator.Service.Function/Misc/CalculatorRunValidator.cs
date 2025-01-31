﻿using EPR.Calculator.Service.Function.Data.DataModels;
using FluentValidation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EPR.Calculator.API.Validators
{
    public class CalculatorRunValidator : AbstractValidator<string>
    {
        public CalculatorRunValidator()
        {
            RuleFor(x => x).NotEmpty().WithMessage("Calculator Run Name is Required");
        }
        public ValidationResult ValidateCalculatorRunIds(CalculatorRun calculatorRun)
        {
            var errorMessages = new List<string>();

            var requiredMasterIds = new Dictionary<string, int?>
            {
                { "CalculatorRunOrganisationDataMasterId", calculatorRun.CalculatorRunOrganisationDataMasterId },
                { "DefaultParameterSettingMasterId", calculatorRun.DefaultParameterSettingMasterId },
                { "CalculatorRunPomDataMasterId", calculatorRun.CalculatorRunPomDataMasterId },
                { "LapcapDataMasterId", calculatorRun.LapcapDataMasterId }
            };

            errorMessages.AddRange(requiredMasterIds
                .Where(id => id.Value == null)
                .Select(id => $"{id.Key} is null"));

            return new ValidationResult
            {
                IsValid = errorMessages.Count == 0,
                ErrorMessages = errorMessages
            };
        }
    }
}