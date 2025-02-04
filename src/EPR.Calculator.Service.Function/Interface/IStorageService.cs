namespace EPR.Calculator.Service.Function.Interface
{
    using System.Threading.Tasks;

    public interface IStorageService
    {
        Task<bool> UploadResultFileContentAsync(string fileName, string content);
    }
}
