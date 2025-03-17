namespace EPR.Calculator.Service.Common.UnitTests.AutoFixtureCustomisations
{
    using System;
    using AutoFixture.Kernel;

    /// <summary>
    /// Customisation that allows AutoFixture to generate financial years.
    /// </summary>
    public class FinancialYearCustomisation : ISpecimenBuilder
    {
        /// <inheritdoc/>
        public object Create(object request, ISpecimenContext context)
        {
            if (!typeof(FinancialYear).Equals(request))
            {
                return new NoSpecimen();
            }

            var year = new Random().Next(1, 99);
            return new FinancialYear($"20{year:00}-{year - 1:00}");
        }
    }
}
