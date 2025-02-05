namespace EPR.Calculator.Service.Function.Interface
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    public interface IStorageService
    {
        Task<bool> UploadResultFileContentAsync(string fileName, string content);
        // Task<bool> DownloadFile(string fileName, string blobUri);
    }
}
