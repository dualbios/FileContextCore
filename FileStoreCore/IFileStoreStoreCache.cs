using kDg.FileBaseContext.Storage;

namespace kDg.FileBaseContext;

public interface IFileStoreStoreCache
{
    IFileStoreStore GetStore(IFileStoreScopedOptions options);
}