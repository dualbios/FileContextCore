using FileStoreCore.Infrastructure;
using FileStoreCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FileStoreCore.Extensions;

public static class FileStoreCacheExtensions
{
    public static IFileStoreStore GetStore(this IFileStoreStoreCache storeCache, IDbContextOptions options)
    {
        return storeCache.GetStore(options.Extensions.OfType<FileStoreOptionsExtension>().First().Options);
    }
}