﻿namespace EPR.Calculator.API.Services
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using global::EPR.Calculator.Service.Function.Interface;

    public class LocalFileStorageService : IStorageService
    {
        public Task<string?> GetResultFileContentAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> UploadResultFileContentAsync(string fileName, string content)
        {
            var path = $"{Directory.GetCurrentDirectory()}\\{fileName}";
            File.WriteAllText(path, content, Encoding.UTF8);
            var result = Task.FromResult(true);
            return result;
        }
    }
}
