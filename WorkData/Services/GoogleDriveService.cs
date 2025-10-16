using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.Extensions.Options;
using System.Text;
using WorkData.Data.GenericModels;
using WorkData.Services.Interfaces;

namespace WorkData.Services
{
    public class GoogleDriveService : IGoogleDriveService
    {
        #region VARS
        private readonly DriveService _driveService;
        private readonly string _credentialsPath;
        private readonly string _applicationName;
        #endregion

        #region CONSTRUCTOR
        // Accetta IOptions<GoogleDriveSettings> per le credenziali.
        public GoogleDriveService(IOptions<GoogleDriveSettings> options)
        {
            // Nota: Ho rimosso l'uso di email/pw non standard
            // e mantenuto _credentialsPath per l'Account di Servizio,
            // che dovresti spostare in configurazione o usare variabili d'ambiente.

            // Nel tuo caso, se usi l'email per un'identificazione, potresti salvarla.
            var settings = options.Value;
            _applicationName = settings.ServiceName ?? "WorkDataApp";

            // Ad esempio, potresti leggere il percorso del file JSON da una chiave di configurazione
            // Esempio: _credentialsPath = settings.CredentialsFilePath;

            // Per il momento mantengo il tuo hardcode per le credenziali:
            _credentialsPath = "path/al/tuo/client_secret.json"; // <-- RIMANI UN PROMEMORIA!

            _driveService = InitializeDriveService();
        }
        #endregion

        #region INIT
        private DriveService InitializeDriveService()
        {
            GoogleCredential credential;

            using (var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(new[] { DriveService.ScopeConstants.DriveFile, DriveService.ScopeConstants.Drive });
            }

            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName,
            });
        }
        #endregion

        // -----------------------------------------------------------------------------------------
        // METODI DEL SERVIZIO (Trasformati da IActionResult a Task<T>)
        // -----------------------------------------------------------------------------------------

        public async Task<IList<Google.Apis.Drive.v3.Data.File>> ListFilesAsync()
        {
            var fileList = _driveService.Files.List();
            fileList.PageSize = 10;
            fileList.Fields = "nextPageToken, files(id, name, mimeType, size)";

            var filesResult = await fileList.ExecuteAsync();
            return filesResult.Files;
        }

        // Metodo di supporto per restituire oggetti anonimi al Controller
        public async Task<object> ListFilesSimplifiedAsync()
        {
            var files = await ListFilesAsync();

            return files.Select(f => new
            {
                Id = f.Id,
                Name = f.Name,
                MimeType = f.MimeType,
                SizeKB = f.Size / 1024
            }).ToList();
        }


        public async Task<Google.Apis.Drive.v3.Data.File> UploadFileAsync(string fileName, string fileContent)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                MimeType = "text/plain"
            };

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent)))
            {
                var request = _driveService.Files.Create(fileMetadata, stream, fileMetadata.MimeType);
                request.Fields = "id, name";

                // Esegui l'upload
                var uploadProgress = await Task.Run(() => request.Upload());

                if (uploadProgress.Status != UploadStatus.Completed)
                {
                    throw new Exception("Upload non completato correttamente: " + uploadProgress.Exception?.Message);
                }

                // Recupera i metadati del file caricato
                var uploadedFile = request.ResponseBody;
                if (uploadedFile == null || string.IsNullOrEmpty(uploadedFile.Id))
                {
                    throw new Exception("Upload completato, ma non è stato possibile recuperare l'ID del file.");
                }

                return uploadedFile;
            }
        }

        public async Task<Stream> DownloadFileAsync(string fileId)
        {
            var request = _driveService.Files.Get(fileId);
            var stream = new MemoryStream();

            await request.DownloadAsync(stream);
            stream.Position = 0;
            return stream;
        }

        public async Task DeleteFileAsync(string fileId)
        {
            var request = _driveService.Files.Delete(fileId);
            await request.ExecuteAsync();
        }
    }
}