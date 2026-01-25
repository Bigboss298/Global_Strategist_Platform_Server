using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Global_Strategist_Platform_Server.Model.Enum;
using Microsoft.AspNetCore.Http;

namespace Global_Strategist_Platform_Server.Gateway.FileManager
{
    public class FileManager(IConfiguration configuration, IWebHostEnvironment env) : IFileManager
    {
        private readonly string _connectionString = configuration.GetConnectionString("AzureBlobStorage") ?? throw new InvalidOperationException("AzureBlobStorage connection string not configured");
        private readonly IWebHostEnvironment _env = env;

        private string GetContainerName(FileCategory category)
        {
            return category switch
            {
                FileCategory.ProfilePicture => "strategistplatformprofilepictures",
                FileCategory.ProjectImage => "strategistplatformprojectimages",
                FileCategory.CVFile => "strategistplatformcvfile",
                _ => throw new ArgumentException($"Invalid file category: {category}")
            };
        }

        private List<string> GetAllowedExtensions(FileCategory category)
        {
            return category switch
            {
                FileCategory.ProjectImage => [".jpg", ".jpeg", ".png"],
                FileCategory.ProfilePicture => [".jpg", ".jpeg", ".png"],
                FileCategory.CVFile => [".pdf"],
                _ => []
            };
        }

        private string GetContentType(string fileExtension)
        {
            return fileExtension.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }

        public async Task<(bool success, string message, string fileUrl)> UploadFile(IFormFile formFile, FileCategory category)
        {
            try
            {
                if (formFile == null || formFile.Length <= 0)
                    return (false, "File not found", "");

                var acceptableExtension = GetAllowedExtensions(category);
                var fileExtension = Path.GetExtension(formFile.FileName).ToLower();

                if (!acceptableExtension.Contains(fileExtension))
                    return (false, $"File format not supported. Allowed formats: {string.Join(", ", acceptableExtension)}", "");

                const long maxFileSizeInBytes = 5 * 1024 * 1024; // 5MB
                if (formFile.Length > maxFileSizeInBytes)
                    return (false, "File size exceeds the 5MB limit.", "");

                var fileName = $"{Guid.NewGuid().ToString()[..8]}_{Path.GetFileName(formFile.FileName)}";
                var containerName = GetContainerName(category);

                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                
                // Create container with public access if it doesn't exist
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                
                // Ensure existing container has public access
                await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);
                
                var blobClient = containerClient.GetBlobClient(fileName);

                // Set blob upload options with proper content type
                var blobUploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = GetContentType(fileExtension),
                        // Set to inline for PDFs so they display in browser, not force download
                        ContentDisposition = category == FileCategory.CVFile 
                            ? "inline" 
                            : $"attachment; filename={fileName}"
                    }
                };

                using var stream = formFile.OpenReadStream();
                await blobClient.UploadAsync(stream, blobUploadOptions);

                var fileUrl = blobClient.Uri.ToString();
                return (true, "File uploaded successfully", fileUrl);
            }
            catch (Azure.RequestFailedException azEx)
            {
                return (false, $"Azure Blob Storage error: {azEx.Message} (Status: {azEx.Status}, ErrorCode: {azEx.ErrorCode})", "");
            }
            catch (Exception ex)
            {
                return (false, $"Error uploading file: {ex.GetType().Name} - {ex.Message}", "");
            }
        }

        public async Task<(bool success, string message, List<string> uploadedFileUrls)> UploadFiles(List<IFormFile> files, FileCategory category)
        {
            var uploadedFileUrls = new List<string>();

            if (files == null || files.Count == 0)
                return (false, "No files provided", uploadedFileUrls);

            var acceptableExtension = GetAllowedExtensions(category);
            const long maxFileSizeInBytes = 5 * 1024 * 1024; // 5MB

            try
            {
                var containerName = GetContainerName(category);
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                
                // Create container with public access if it doesn't exist
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                
                // Ensure existing container has public access
                await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

                foreach (var file in files)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!acceptableExtension.Contains(fileExtension) || file.Length > maxFileSizeInBytes)
                        continue;

                    var fileName = $"{Guid.NewGuid().ToString()[..8]}_{Path.GetFileName(file.FileName)}";
                    var blobClient = containerClient.GetBlobClient(fileName);

                    // Set blob upload options with proper content type
                    var blobUploadOptions = new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = GetContentType(fileExtension),
                            // Set to inline for PDFs so they display in browser, not force download
                            ContentDisposition = category == FileCategory.CVFile 
                                ? "inline" 
                                : $"attachment; filename={fileName}"
                        }
                    };

                    using var stream = file.OpenReadStream();
                    await blobClient.UploadAsync(stream, blobUploadOptions);
                    var fileUrl = blobClient.Uri.ToString();
                    uploadedFileUrls.Add(fileUrl);
                }

                return (true, "Bulk upload completed", uploadedFileUrls);
            }
            catch (Exception ex)
            {
                return (false, $"Error during bulk upload: {ex.Message}", uploadedFileUrls);
            }
        }

        public async Task<(bool success, string message)> DeleteFile(string fileName, FileCategory category)
        {
            try
            {
                var containerName = GetContainerName(category);
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);
                var result = await blobClient.DeleteIfExistsAsync();

                return result
                    ? (true, "File deleted successfully.")
                    : (false, "File not found or already deleted.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting file: {ex.Message}");
            }
        }

        public async Task<(bool success, string message, List<string> deletedFiles)> DeleteFiles(List<string> fileNames, FileCategory category)
        {
            var deletedFiles = new List<string>();

            if (fileNames == null || fileNames.Count == 0)
                return (false, "No file names provided.", deletedFiles);

            try
            {
                var containerName = GetContainerName(category);
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                foreach (var fileName in fileNames)
                {
                    var blobClient = containerClient.GetBlobClient(fileName);
                    var result = await blobClient.DeleteIfExistsAsync();

                    if (result)
                        deletedFiles.Add(fileName);
                }

                return (true, "Bulk delete completed.", deletedFiles);
            }
            catch (Exception ex)
            {
                return (false, $"Error during bulk delete: {ex.Message}", deletedFiles);
            }
        }
    }
}