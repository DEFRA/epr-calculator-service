namespace EPR.Calculator.Service.Function.Interface
{
    using System.Threading.Tasks;

    public interface IRunNameService
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="runId">runId is number.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<string?> GetRunNameAsync(int runId);
    }
}