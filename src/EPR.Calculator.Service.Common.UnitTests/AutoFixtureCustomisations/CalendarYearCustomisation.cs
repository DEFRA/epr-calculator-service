namespace EPR.Calculator.Service.Common.UnitTests.AutoFixtureCustomisations
{
    using System;
    using AutoFixture.Kernel;

    /// <summary>
    /// Customisation that allows AutoFixture to generate calendar years.
    /// </summary>
    public class CalendarYearCustomisation : ISpecimenBuilder
    {
        /// <inheritdoc/>
        public object Create(object request, ISpecimenContext context)
        {
            if (!typeof(CalendarYear).Equals(request))
            {
                return new NoSpecimen();
            }

            var year = new Random().Next(0, 99);
            return new CalendarYear($"20{year:00}");
        }
    }
}
