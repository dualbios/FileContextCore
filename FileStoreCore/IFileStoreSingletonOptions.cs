using FileStoreCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FileStoreCore;

public interface IFileStoreSingletonOptions : ISingletonOptions
{
    FileStoreDatabaseRoot DatabaseRoot { get; }
    bool IsNullabilityCheckEnabled { get; }
}