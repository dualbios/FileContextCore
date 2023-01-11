using kDg.FileBaseContext.Infrastructure;
using kDg.FileBaseContext.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace kDg.FileBaseContext.Extensions;

public static class FileStoreCacheExtensions
{
    public static IFileStoreStore GetStore(this IFileStoreStoreCache storeCache, IDbContextOptions options)
    {
        return storeCache.GetStore(options.Extensions.OfType<FileStoreOptionsExtension>().First().Options);
    }
}