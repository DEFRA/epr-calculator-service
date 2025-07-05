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
        public Task<string> UploadFileContentAsync(
            (string FileName,
             string Content,
             string RunName,
             string ContainerName,
             bool Overwrite)
            args)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), args.FileName);
            File.WriteAllText(path, args.Content, Encoding.UTF8);
            return Task.FromResult(path);
        }
    }
}
