namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.Service.Common.Logging;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.Service.Function.Dtos;
    using EPR.Calculator.Service.Function.Enums;
    using EPR.Calculator.Service.Function.Interface;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using EPR.Calculator.API.Data.DataSeeder;
    using Microsoft.Azure.Amqp.Framing;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
    using System.Reflection;

    /// <summary>
    /// Service for transposing POM and organization data.
    /// </summary>
    public class OrgAndPomDataMYCPreValidationService : IOrgAndPomDataMYCPreValidationService
    {
        private readonly ApplicationDBContext context;
        private readonly ICalculatorTelemetryLogger telemetryLogger;

        public OrgAndPomDataMYCPreValidationService(
            ApplicationDBContext context,
            ICalculatorTelemetryLogger telemetryLogger)
        {
            this.context = context;
            this.telemetryLogger = telemetryLogger;
        }

        public ICommandTimeoutService CommandTimeoutService { get; init; }

        public async Task OrgAndPomDataMYCPreValidation(int calculatorRunId, string? runName, CancellationToken cancellationToken)
        {

            //gets calc data.
            var calculatorRun = await this.context.CalculatorRuns
               .Where(x => x.Id == calculatorRunId)
               .SingleAsync(cancellationToken);

            //gets run org data.
            var calculatorRunOrgDataDetails = await this.context.CalculatorRunOrganisationDataDetails
                .Where(x => x.CalculatorRunOrganisationDataMasterId == calculatorRun.CalculatorRunOrganisationDataMasterId)
                .OrderBy(x => x.SubmissionPeriodDesc)
                .ToListAsync(cancellationToken);

            //gets run pom data.
            var calculatorRunPomDataDetails = await this.context.CalculatorRunPomDataDetails
                .Where(x => x.CalculatorRunPomDataMasterId == calculatorRun.CalculatorRunPomDataMasterId)
                .OrderBy(x => x.SubmissionPeriodDesc)
                .ToListAsync(cancellationToken);

            //Pre-validation starts

            //1. Pre Validation of Data -Shell / Orchestrator

            //2. Pre - Validation of data -Single entry
            foreach (var organisation in calculatorRunOrgDataDetails)
            {
                // TO DO: Update flag IsValid to false in calculatorRunOrgDataDetails
                // TO DO: Insert Record in error table in db (new table)
            }

            //3. Pre - Validation of data -Multiple entry
                // Get the data based on group by producer id and check for multiple entry etc.,
            //4. Pre - Validation of data -Status Code 11 & 12(Data Wash)

            //5. Pre - Validation of data -Code 13 & 14(Data Wash)

            //6. Pre - Validation of data -Status Codes: 15, 17 & 19(Might not be needed)

            //7. Pre - Validation of data -Status Code 16

        }
    }
}