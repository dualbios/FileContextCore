using kDg.FileBaseContext.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace kDg.FileBaseContext;

public interface IFileStoreSingletonOptions : ISingletonOptions
{
    FileStoreDatabaseRoot DatabaseRoot { get; }
    bool IsNullabilityCheckEnabled { get; }
}