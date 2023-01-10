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
        var extension = optionsBuilder.Options.FindExtension<FileStoreOptionsExtension>()
                        ?? new FileStoreOptionsExtension()
                        {
                            StoreName = databaseName,
                            Location = location
                        };

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }

    //public static DbContextOptionsBuilder<TContext> UseInMemoryDatabase<TContext>(
    //    this DbContextOptionsBuilder<TContext> optionsBuilder,
    //    string databaseName,
    //    Action<InMemoryDbContextOptionsBuilder>? inMemoryOptionsAction = null)
    //    where TContext : DbContext
        //=> (DbContextOptionsBuilder<TContext>)UseInMemoryDatabase(
        //    (DbContextOptionsBuilder)optionsBuilder, databaseName, inMemoryOptionsAction);
}