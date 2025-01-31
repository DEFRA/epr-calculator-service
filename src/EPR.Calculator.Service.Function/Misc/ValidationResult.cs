namespace EPR.Calculator.Service.Function.Misc
{
    using System.Collections.Generic;

    public class ValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        public ValidationResult() => this.ErrorMessages = new List<string>();

        public bool IsValid { get; set; }

        public IEnumerable<string> ErrorMessages { get; set; }
    }
}