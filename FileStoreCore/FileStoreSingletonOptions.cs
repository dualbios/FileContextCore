using kDg.FileBaseContext.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace kDg.FileBaseContext;

public class FileStoreSingletonOptions : IFileStoreSingletonOptions
{
    public virtual FileStoreDatabaseRoot DatabaseRoot { get; private set; }

    public virtual bool IsNullabilityCheckEnabled { get; private set; }

    public virtual void Initialize(IDbContextOptions options)
    {
        var inMemoryOptions = options.FindExtension<FileStoreOptionsExtension>();

        if (inMemoryOptions != null)
        {
            DatabaseRoot = inMemoryOptions.DatabaseRoot;
            IsNullabilityCheckEnabled = inMemoryOptions.IsNullabilityCheckEnabled;
        }
    }

    public virtual void Validate(IDbContextOptions options)
    {
    }
}