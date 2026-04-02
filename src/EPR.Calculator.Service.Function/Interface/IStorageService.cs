namespace EPR.Calculator.Service.Function.Interface
{
    public interface IStorageService
    {
        Task<string> UploadFileContentAsync(
            (string FileName,
             string Content,
             string RunName,
             string ContainerName,
             bool Overwrite
            )
            args);
    }
}
