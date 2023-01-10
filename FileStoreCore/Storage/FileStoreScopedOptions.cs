namespace FileStoreCore.Storage;

public class FileStoreScopedOptions : IFileStoreScopedOptions
{
    public FileStoreScopedOptions(string databaseName = null, string location = null)
    {
        DatabaseName = databaseName;
        Location = location;
    }

    public string DatabaseName { get; }
    public string Location { get; }
}