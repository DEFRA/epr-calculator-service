namespace EPR.Calculator.Service.Function.Interface
{
    using System.Threading.Tasks;

    public interface IStorageService
    {
        Task<string> UploadFileContentAsync(
            (string FileName, string Content, string RunName, string ContainerName) args);
    }
}
