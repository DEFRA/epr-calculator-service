using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.Service.Function.PreValidator
{
    public class ValidationResult
    {
        public IEnumerable<ErrorReport> Errors { get; set; } = [];
    }

    public class ErrorReport
    {
        public int ProducerId { get; set; }
        public string SubsidaryId { get; set; }
        public int CalculatorRunId { get; set; }
        public string LeaverCode { get; set; }
        public int ErrorTypeId { get; set; }
        public string SubmitterOrgId { get; set; }
    }


    public interface IValidator
    {
        ValidationResult Validate(int calculatorRunId,
                                  IEnumerable<CalculatorRunOrganisationDataDetail> runOrganisations,
                                  IEnumerable<CalculatorRunPomDataDetail> runPoms);
    }

    public interface IValidationOrchestrator
    {
        void Validate(int calculatorRunId);
    }

    public interface IValidationFactory
    {
        IEnumerable<IValidator> CreateValidators();
    }

    public class ValidationOrchestrator : IValidationOrchestrator
    {
        private  readonly ApplicationDBContext _context;
        private IValidationFactory _validationFactory;
        public ValidationOrchestrator(ApplicationDBContext context, IValidationFactory validationFactory)
        {
            _context = context;
            _validationFactory = validationFactory;
        }
        public void Validate(int calculatorRunId)
        {
            var runOrganisations = (from run in this._context.CalculatorRuns
                                    join orgMaster in this._context.CalculatorRunOrganisationDataMaster on run.CalculatorRunOrganisationDataMasterId equals orgMaster.Id
                                    join orgDetail in this._context.CalculatorRunOrganisationDataDetails on orgMaster.Id equals orgDetail.CalculatorRunOrganisationDataMasterId
                                    where run.Id == calculatorRunId
                                    select orgDetail);

            var runPoms = (from run in this._context.CalculatorRuns
                           join pomMaster in this._context.CalculatorRunPomDataMaster on run.CalculatorRunPomDataMasterId equals pomMaster.Id
                           join pomDetail in this._context.CalculatorRunPomDataDetails on pomMaster.Id equals pomDetail.CalculatorRunPomDataMasterId
                           where run.Id == calculatorRunId
                           select pomDetail);

            var validators = _validationFactory.CreateValidators();

            foreach (var validator in validators)
            {
                ValidateScenario(validator, calculatorRunId, runOrganisations, runPoms);
            }
        }

        private void ValidateScenario(IValidator validator,
                                      int calculatorRunId,
                                      IEnumerable<CalculatorRunOrganisationDataDetail> runOrganisations,
                                      IEnumerable<CalculatorRunPomDataDetail> runPoms)
        {
            // var validRunPoms = runPoms.Where(p => p.IsValid);

            // var validRunOrgs = runPoms.Where(p => p.IsValid);

            // var result = validator.Validate(calculatorRunId, validRunOrgs, validRunPoms);

            //if (result.Errors.Any())
            //{
            //this._context.CalculatorRunErrorReports.AddRange(result.Errors.Select(e => new CalculatorRunErrorReport
            //{
            //    CalculatorRunId = e.CalculatorRunId,
            //    ErrorTypeId = e.ErrorTypeId,
            //    LeaverCode = e.LeaverCode,
            //    ProducerId = e.ProducerId,
            //    SubsidaryId = e.SubsidaryId,
            //    SubmitterOrgId = e.SubmitterOrgId
            //}));

            // Update Run Details to mark as Validation Failed

            // Update Pom Details to mark as Validation Failed

            // this._context.SaveChanges();
            // }
        }
    }
}
