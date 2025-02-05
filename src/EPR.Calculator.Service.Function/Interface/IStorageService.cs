namespace EPR.Calculator.Service.Function.Interface
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    public interface IStorageService
    {
        Task<string> UploadResultFileContentAsync(string fileName, string content);
    }
}
