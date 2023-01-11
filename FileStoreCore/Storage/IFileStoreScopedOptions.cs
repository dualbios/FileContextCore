namespace kDg.FileBaseContext.Storage;

public interface IFileStoreScopedOptions
{
    string DatabaseName { get; }
    string Location { get; }
}