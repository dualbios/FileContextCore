using FileStoreCore.Storage;

namespace FileStoreCore;

public interface IFileStoreStoreCache
{
    IFileStoreStore GetStore(IFileStoreScopedOptions options);
}