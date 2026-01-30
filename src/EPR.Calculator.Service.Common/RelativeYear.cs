namespace EPR.Calculator.Service.Common
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents a relative year, with the format yyyy.
    /// </summary>
    public partial record struct RelativeYear
    {
        [GeneratedRegex("^[0-9]{4}$", RegexOptions.IgnoreCase, "en-GB")]
        private static partial Regex RelativeYearRegex();

        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeYear"/> class.
        /// </summary>
        /// <param name="value">The year in yyyy format.</param>
        /// <remarks>
        /// Use this and <see cref="FinancialYear"/> in place of strings in places where it's helpfull to avoid
        /// confusion between date types.
        /// </remarks>
        public RelativeYear(string value)
        {
            if (!RelativeYearRegex().IsMatch(value))
            {
                throw new System.ArgumentException("The year must be in the format yyyy.");
            }

            this.Value = value;
        }

        private string Value { get; init; }

        public static implicit operator RelativeYear(string value) => new RelativeYear(value);

        public static implicit operator string(RelativeYear value) => value.ToString();

        /// <inheritdoc/>
        public override string ToString() => this.Value;
    }
}
