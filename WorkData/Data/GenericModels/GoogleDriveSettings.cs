namespace WorkData.Data.GenericModels
{
    public class GoogleDriveSettings
    {
        // Il nome della sezione nel config
        public const string SectionName = "GoogleDrive";

        public string ServiceName { get; set; }
        public string Email { get; set; }
        public string Pw { get; set; } // O serviceAccountKey, come dovrebbe essere per Drive API
    }
}
