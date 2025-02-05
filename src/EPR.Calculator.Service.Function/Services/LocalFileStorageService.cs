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
        public Task<string?> GetResultFileContentAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> UploadResultFileContentAsync(string fileName, string content)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            File.WriteAllText(path, content, Encoding.UTF8); 
            var result = Task.FromResult(true);
            return result;
        }
    }
}
