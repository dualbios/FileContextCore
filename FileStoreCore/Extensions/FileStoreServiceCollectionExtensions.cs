using FileStoreCore.Infrastructure;
using FileStoreCore.Infrastructure.Query.Internal;
using FileStoreCore.Storage;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace FileStoreCore.Extensions;

public static class FileStoreServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkFileStoreDatabase(this IServiceCollection serviceCollection)
    {
        var builder = new EntityFrameworkServicesBuilder(serviceCollection)
            .TryAdd<LoggingDefinitions, FileStoreLoggingDefinitions>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<FileStoreOptionsExtension>>()

            //.TryAdd<IValueGeneratorSelector, FileContextValueGeneratorSelector>()
            .TryAdd<IDatabase>(p => p.GetService<IFileStoreDatabase>())
            //.TryAdd<IDatabase, FileStoreDatabase>()
            //.TryAdd<ISingletonOptionsInitializer, SingletonOptionsInitializer>()

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
                    //.TryAddSingleton<FileStoreTablesManager, FileStoreTablesManager>()
                    //.TryAddSingleton<IFileStoreStore, FileStoreStore>()

                    .TryAddSingleton<IFileStoreSingletonOptions, FileStoreSingletonOptions>()
                    .TryAddSingleton<IFileStoreStoreCache, FileStoreStoreCache>()
                    //.TryAddSingleton<IFileStoreTableFactory, FileStoreTableFactory>()
                    .TryAddScoped<IFileStoreDatabase, FileStoreDatabase>()
                );
                    //.TryAddSingleton<FileStoreTableFactory, FileStoreTableFactory>()
                    //.GetInfrastructure()
                    //.AddDependencyScoped<RelationalQueryableMethodTranslatingExpressionVisitorDependencies>());


            //.TryAdd<ISingletonOptions, IFileContextSingletonOptions>(p => p.GetService<IFileContextSingletonOptions>())
            //.TryAddProviderSpecificServices(
            //    b => b
            //        .TryAddSingleton<IFileContextSingletonOptions, FileContextSingletonOptions>()
            //        .TryAddSingleton<IFileContextStoreCache, FileContextStoreCache>()
            //        .TryAddScoped<IFileContextDatabase, FileContextDatabase>()
            //        .TryAddTransient<EXCELStoreManager, EXCELStoreManager>()
            //        .TryAddTransient<DefaultStoreManager, DefaultStoreManager>()
            //        .TryAddTransient<BSONSerializer, BSONSerializer>()
            //        .TryAddTransient<CSVSerializer, CSVSerializer>()
            //        .TryAddTransient<JSONSerializer, JSONSerializer>()
            //        .TryAddTransient<XMLSerializer, XMLSerializer>()
            //        .TryAddTransient<DefaultFileManager, DefaultFileManager>()
            //        .TryAddTransient<EncryptedFileManager, EncryptedFileManager>()
            //        .TryAddTransient<PrivateFileManager, PrivateFileManager>())
            ;

        builder.TryAddCoreServices();

        return serviceCollection;
    }
}