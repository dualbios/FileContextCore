using kDg.FileBaseContext.Infrastructure;
using kDg.FileBaseContext.Infrastructure.Query;
using kDg.FileBaseContext.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace kDg.FileBaseContext.Extensions;

public static class FileStoreServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkFileStoreDatabase(this IServiceCollection serviceCollection)
    {
        var builder = new EntityFrameworkServicesBuilder(serviceCollection)
            .TryAdd<LoggingDefinitions, FileStoreLoggingDefinitions>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<FileStoreOptionsExtension>>()
            .TryAdd<IDatabase>(p => p.GetService<IFileStoreDatabase>())
            .TryAdd<IDbContextTransactionManager, FileStoreTransactionManager>()
            .TryAdd<IDatabaseCreator, FileStoreDatabaseCreator>()
            .TryAdd<IQueryContextFactory, FileStoreQueryContextFactory>()
            .TryAdd<IProviderConventionSetBuilder, FileStoreConventionSetBuilder>()
            .TryAdd<ITypeMappingSource, FileStoreTypeMappingSource>()

            //// New Query pipeline
            .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, FileStoreShapedQueryCompilingExpressionVisitorFactory>()
            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, FileStoreQueryableMethodTranslatingExpressionVisitorFactory>()
            .TryAdd<IQueryTranslationPostprocessorFactory, FileStoreQueryTranslationPostprocessorFactory>()

            .TryAddProviderSpecificServices(
                b => b
                    .TryAddSingleton<IFileStoreFileManager, FileStoreFileManager>()
                    .TryAddSingleton<IFileStoreSingletonOptions, FileStoreSingletonOptions>()
                    .TryAddSingleton<IFileStoreStoreCache, FileStoreStoreCache>()
                    .TryAddScoped<IFileStoreDatabase, FileStoreDatabase>()
                );

        builder.TryAddCoreServices();

        return serviceCollection;
    }
}