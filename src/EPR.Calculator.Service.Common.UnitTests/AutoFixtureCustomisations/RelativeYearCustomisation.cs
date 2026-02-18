namespace EPR.Calculator.Service.Common.UnitTests.AutoFixtureCustomisations
{
    using System;
    using AutoFixture.Kernel;
    using EPR.Calculator.API.Data.Models;


    /// <summary>
    /// Customisation that allows AutoFixture to generate relative years.
    /// </summary>
    public class RelativeYearCustomisation : ISpecimenBuilder
    {
        /// <inheritdoc/>
        public object Create(object request, ISpecimenContext context)
        {
            if (!typeof(RelativeYear).Equals(request))
            {
                return new NoSpecimen();
            }

            var year = new Random().Next(2000, 2099);
            return new RelativeYear(year);
        }
    }
}
