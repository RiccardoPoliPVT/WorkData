using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Text;
using WorkData.Data.GenericModels;
using Microsoft.Extensions.Options;
namespace WorkData.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class GoogleDriveController : ControllerBase
    {
        #region VARS
        private readonly DriveService _driveService;
        private readonly string _credentialsPath = "path/al/tuo/client_secret.json"; // CAMBIA QUESTO PERCORSO!
        private readonly string _applicationName = "WorkDataApp";
        private readonly GoogleDriveSettings _settings;
        #endregion

        #region CONSTRUCTOR
        public GoogleDriveController(IOptions<GoogleDriveSettings> options)
        {
            _settings = options.Value;
            _driveService = InitializeDriveService();
        }
        #endregion

        // Metodo privato per inizializzare il servizio di Google Drive tramite Service Account
        #region INIT
        private DriveService InitializeDriveService()
        {
            GoogleCredential credential;

            // 1. Carica le credenziali dal file JSON del Service Account
            using (var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
            {
                // Se devi impersonare un utente G Suite/Google Workspace (delegazione a livello di dominio), usa:
                // credential = GoogleCredential.FromStream(stream).CreateScoped(DriveService.ScopeConstants.Drive);

                // Per un Service Account standard che accede al proprio Drive (o Drive condiviso)
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(new[] { DriveService.ScopeConstants.DriveFile, DriveService.ScopeConstants.Drive });
            }

            // 2. Crea il DriveService
            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName,
            });
        }
        #endregion

        #region READ
        [HttpGet("ListFiles")]
        public async Task<IActionResult> ListFiles()
        {
            try
            {
                var fileList = _driveService.Files.List();
                fileList.PageSize = 10;
                fileList.Fields = "nextPageToken, files(id, name, mimeType, size)";

                var filesResult = await fileList.ExecuteAsync();

                var files = filesResult.Files.Select(f => new
                {
                    Id = f.Id,
                    Name = f.Name,
                    MimeType = f.MimeType,
                    SizeKB = f.Size / 1024
                }).ToList();

                return Ok(files);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il recupero dei file: {ex.Message}");
            }
        }
        #endregion

        #region UPLOAD
        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(string fileName, string fileContent)
        {
            try
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
                    var progress = await Task.Run(() => request.Upload());

                    // Recupera il file caricato tramite la proprietà ResponseBody
                    var uploadedFile = request.ResponseBody;

                    if (uploadedFile != null && uploadedFile.Id != null)
                    {
                        return Ok(new
                        {
                            Message = "File caricato con successo",
                            FileId = uploadedFile.Id
                        });
                    }

                    return StatusCode(500, "Upload completato, ma non è stato possibile recuperare l'ID del file.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante il caricamento del file: {ex.Message}");
            }
        }
        #endregion

        #region DOWNLOAD
        [HttpGet("DownloadFile/{fileId}")]
        public async Task<IActionResult> DownloadFile(string fileId)
        {
            try
            {
                var request = _driveService.Files.Get(fileId);
                var stream = new MemoryStream();

                // Scarica il contenuto nel MemoryStream
                await request.DownloadAsync(stream);
                stream.Position = 0;

                // Restituisce il file come risultato del download
                return File(stream, "application/octet-stream", "downloaded_file");
                // Il client ASP.NET dovrà salvare questo contenuto nel file SQLite locale
            }
            catch (Exception ex)
            {
                // Un file non trovato spesso lancia un'eccezione, gestiscila
                return StatusCode(500, $"Errore durante il download del file: {ex.Message}");
            }
        }
        #endregion

        #region DELETE
        [HttpDelete("DeleteFile/{fileId}")]
        public async Task<IActionResult> DeleteFile(string fileId)
        {
            try
            {
                var request = _driveService.Files.Delete(fileId);
                await request.ExecuteAsync();

                return Ok(new { Message = $"File con ID {fileId} eliminato con successo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante l'eliminazione del file: {ex.Message}");
            }
        }
        #endregion
    }
}