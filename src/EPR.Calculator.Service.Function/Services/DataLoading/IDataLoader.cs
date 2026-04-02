namespace EPR.Calculator.Service.Function.Services.DataLoading
{
    /// <summary>
    ///     Loads POM and Organisation data required for a calculator run.
    /// </summary>
    public interface IDataLoader
    {
        /// <summary>
        ///     Loads data for the specified calculator run.
        /// </summary>
        /// <param name="runParams">The parameters for the calculator run.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous load operation.</returns>
        Task LoadData(CalculatorRunParams runParams, CancellationToken cancellationToken = default);
    }
}