namespace FileStoreCore.Storage;

public class FileStoreScopedOptions : IFileStoreScopedOptions
{
    public string DatabaseName { get; set; }
    public string Location { get; }

}