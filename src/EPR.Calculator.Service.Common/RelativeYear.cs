namespace EPR.Calculator.Service.Common
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents a relative year, with the format yyyy.
    /// </summary>
    public partial record struct RelativeYear
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeYear"/> class.
        /// </summary>
        /// <param name="value">The year in yyyy format.</param>
        /// <remarks>
        /// Use this and <see cref="FinancialYear"/> in place of strings in places where it's helpfull to avoid
        /// confusion between date types.
        /// </remarks>
        public RelativeYear(int value)
        {
            this.Value = value;
        }

        private int Value { get; init; }

        /// <inheritdoc/>
        public override string ToString() => this.Value.ToString();

        public readonly int ToInt() => this.Value;

        public FinancialYear ToFinancialYear() => new FinancialYear($"{this.Value}-{this.Value - 1999}");
    }
}
