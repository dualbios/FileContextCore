using FileStoreCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FileStoreCore.Extensions;

public static class FileStoreDbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UseFileStoreDatabase(
        this DbContextOptionsBuilder optionsBuilder,
        string databaseName = "",
        string location = null,
        string password = null)
    {
        var extension = optionsBuilder.Options.FindExtension<FileStoreOptionsExtension>() ?? new FileStoreOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }
}