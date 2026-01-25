using Global_Strategist_Platform_Server.Model.Enum;
using Microsoft.AspNetCore.Http;

namespace Global_Strategist_Platform_Server.Gateway.FileManager
{
    public interface IFileManager
    {
        Task<(bool success, string message, string fileUrl)> UploadFile(IFormFile formFile, FileCategory category);
        Task<(bool success, string message)> DeleteFile(string fileName, FileCategory category);
        Task<(bool success, string message, List<string> deletedFiles)> DeleteFiles(List<string> fileNames, FileCategory category);
        Task<(bool success, string message, List<string> uploadedFileUrls)> UploadFiles(List<IFormFile> files, FileCategory category);
    }
}
