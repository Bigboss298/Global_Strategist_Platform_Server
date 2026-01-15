
namespace Global_Strategist_Platform_Server.Gateway.FileManager
{
    public interface IFileManager
    {
        Task<(bool success, string message, string filename)> UploadFile(IFormFile formFile);
        Task<(bool success, string message)> DeleteFile(string fileName);
        Task<(bool success, string message, List<string> deletedFiles)> DeleteFiles(List<string> fileNames);
        Task<(bool success, string message, List<string> uploadedFiles)> UploadFiles(List<IFormFile> files);
    }
}
