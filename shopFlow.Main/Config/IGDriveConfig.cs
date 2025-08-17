namespace shopFlow.Config
{
    public interface IGDriveConfig
    {
        string CredentialPath { get; }
        string ImportedFolderId { get; }
        string PendingFolderId { get; }
    }
}