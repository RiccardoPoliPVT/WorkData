using Google.Apis.Drive.v3.Data;

namespace WorkData.Services.Interfaces
{
    public interface IGoogleDriveService
    {
        // Metodi CRUD/File
        Task<IList<Google.Apis.Drive.v3.Data.File>> ListFilesAsync();
        Task<Google.Apis.Drive.v3.Data.File> UploadFileAsync(string fileName, string fileContent);
        Task<Stream> DownloadFileAsync(string fileId);
        Task DeleteFileAsync(string fileId);

        // Modello semplificato per ListFiles
        Task<object> ListFilesSimplifiedAsync();
    }
}