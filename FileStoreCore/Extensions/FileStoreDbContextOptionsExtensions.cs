using FileStoreCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FileStoreCore.Extensions;

public static class FileStoreDbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UseFileStoreDatabase(
        this DbContextOptionsBuilder optionsBuilder,
        string databaseName = "",
        string location = null)
    {
        var extension = optionsBuilder.Options.FindExtension<FileStoreOptionsExtension>() ?? new FileStoreOptionsExtension(databaseName, location);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}