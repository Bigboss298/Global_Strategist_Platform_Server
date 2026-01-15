using Azure.Storage.Blobs;

namespace Global_Strategist_Platform_Server.Gateway.FileManager
{
    public class FileManager(IConfiguration configuration, IWebHostEnvironment env) : IFileManager
    {
        private readonly string _connectionString = configuration.GetConnectionString("AzureBlobStorage");
        private readonly string _containerName = "";
        private readonly IWebHostEnvironment _env = env;

        public async Task<(bool success, string message, string filename)> UploadFile(IFormFile formFile)
        {
            try
            {
                if (formFile == null || formFile.Length <= 0)
                    return (false, "File not found", "");

                var acceptableExtension = new List<string> { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(formFile.FileName).ToLower();

                if (!acceptableExtension.Contains(fileExtension))
                    return (false, $"File format not supported.", "");

                const long maxFileSizeInBytes = 1 * 1024 * 1024;
                if (formFile.Length > maxFileSizeInBytes)
                    return (false, "File size exceeds the 1MB limit.", "");

                var fileName = $"{Guid.NewGuid().ToString()[..4]}{Path.GetFileName(formFile.FileName)}";

                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync();
                var blobClient = containerClient.GetBlobClient(fileName);

                using var stream = formFile.OpenReadStream();
                await blobClient.UploadAsync(stream, overwrite: true);

                return (true, "File uploaded successfully", fileName);
            }
            catch (Exception ex)
            {
                return (false, $"Error uploading file: {ex.Message}", "");
            }
        }

        public async Task<(bool success, string message, List<string> uploadedFiles)> UploadFiles(List<IFormFile> files)
        {
            var uploadedFiles = new List<string>();

            if (files == null || files.Count == 0)
                return (false, "No files provided", uploadedFiles);

            var acceptableExtension = new List<string> { ".jpg", ".jpeg", ".png" };

            const long maxFileSizeInBytes = 1 * 1024 * 1024;

            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync();

                foreach (var file in files)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!acceptableExtension.Contains(fileExtension) || file.Length > maxFileSizeInBytes)
                        continue;

                    var fileName = $"{Guid.NewGuid().ToString()[..4]}{Path.GetFileName(file.FileName)}";
                    var blobClient = containerClient.GetBlobClient(fileName);

                    using var stream = file.OpenReadStream();
                    await blobClient.UploadAsync(stream, overwrite: true);
                    uploadedFiles.Add(fileName);
                }

                return (true, "Bulk upload completed", uploadedFiles);
            }
            catch (Exception ex)
            {
                return (false, $"Error during bulk upload: {ex.Message}", uploadedFiles);
            }
        }

        public async Task<(bool success, string message)> DeleteFile(string fileName)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
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

        public async Task<(bool success, string message, List<string> deletedFiles)> DeleteFiles(List<string> fileNames)
        {
            var deletedFiles = new List<string>();

            if (fileNames == null || fileNames.Count == 0)
                return (false, "No file names provided.", deletedFiles);

            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

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