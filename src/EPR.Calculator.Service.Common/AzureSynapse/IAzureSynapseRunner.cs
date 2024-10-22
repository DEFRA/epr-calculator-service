namespace EPR.Calculator.Service.Common.AzureSynapse
{
    /// <summary>
    /// Inteface for <see cref="AzureSynapseRunner"/>.
    /// </summary>
    public interface IAzureSynapseRunner
    {
        /// <summary>
        /// Run the pipeline, wait for it to complete, then update the status in the database.
        /// </summary>
        /// <param name="financialYear">The financial year to tell the pipeline to load data for.</param>
        /// <returns> True if the pipeline succeeded, false if it failed.</returns>
        Task<bool> Process(FinancialYear financialYear);
    }
}