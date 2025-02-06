namespace EPR.Calculator.API.Services
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using global::EPR.Calculator.Service.Function.Interface;
    using Microsoft.AspNetCore.Mvc;

    public class LocalFileStorageService : IStorageService
    {
        public Task<IActionResult> DownloadFile(string fileName, string blobUri)
        {
            throw new NotImplementedException();
        }

        public Task<string> UploadResultFileContentAsync(string fileName, string content)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            File.WriteAllText(path, content, Encoding.UTF8);
            return Task.FromResult(path);
        }
    }
}
