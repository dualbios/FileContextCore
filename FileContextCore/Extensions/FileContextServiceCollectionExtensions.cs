// Copyright (c) morrisjdev. All rights reserved.
// Original copyright (c) .NET Foundation. All rights reserved.
// Modified version by morrisjdev
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FileContextCore.Diagnostics.Internal;
using FileContextCore.FileManager;
using FileContextCore.Infrastructure.Internal;
using FileContextCore.Metadata.Conventions;
using FileContextCore.Query.Internal;
using FileContextCore.Serializer;
using FileContextCore.Storage.Internal;
using FileContextCore.StoreManager;
using FileContextCore.Utilities;
using FileContextCore.ValueGeneration.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     In-memory specific extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class FileContextServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Adds the services required by the in-memory database provider for Entity Framework
        ///         to an <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Calling this method is no longer necessary when building most applications, including those that
        ///         use dependency injection in ASP.NET or elsewhere.
        ///         It is only needed when building the internal service provider for use with
        ///         the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider"/> method.
        ///         This is not recommend other than for some advanced scenarios.
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddEntityFrameworkFileContextDatabase([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkServicesBuilder(serviceCollection)
                .TryAdd<LoggingDefinitions, FileContextLoggingDefinitions>()
                .TryAdd<IDatabaseProvider, DatabaseProvider<FileContextOptionsExtension>>()
                .TryAdd<IValueGeneratorSelector, FileContextValueGeneratorSelector>()
                .TryAdd<IDatabase>(p => p.GetService<IFileContextDatabase>())
                .TryAdd<IDbContextTransactionManager, FileContextTransactionManager>()
                .TryAdd<IDatabaseCreator, FileContextDatabaseCreator>()
                .TryAdd<IQueryContextFactory, FileContextQueryContextFactory>()
                .TryAdd<IProviderConventionSetBuilder, FileContextConventionSetBuilder>()
                .TryAdd<ITypeMappingSource, FileContextTypeMappingSource>()

                // New Query pipeline
                .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, FileContextShapedQueryCompilingExpressionVisitorFactory>()
                .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, FileContextQueryableMethodTranslatingExpressionVisitorFactory>()
                .TryAdd<IQueryTranslationPostprocessorFactory, FileContextQueryTranslationPostprocessorFactory>()

                .TryAdd<ISingletonOptions, IFileContextSingletonOptions>(p => p.GetService<IFileContextSingletonOptions>())
                .TryAddProviderSpecificServices(
                    b => b
                        .TryAddSingleton<IFileContextSingletonOptions, FileContextSingletonOptions>()
                        .TryAddSingleton<IFileContextStoreCache, FileContextStoreCache>()
                        .TryAddScoped<IFileContextDatabase, FileContextDatabase>()
                        .TryAddTransient<EXCELStoreManager, EXCELStoreManager>()
                        .TryAddTransient<DefaultStoreManager, DefaultStoreManager>()
                        .TryAddTransient<BSONSerializer, BSONSerializer>()
                        .TryAddTransient<CSVSerializer, CSVSerializer>()
                        .TryAddTransient<JSONSerializer, JSONSerializer>()
                        .TryAddTransient<XMLSerializer, XMLSerializer>()
                        .TryAddTransient<DefaultFileManager, DefaultFileManager>()
                        .TryAddTransient<EncryptedFileManager, EncryptedFileManager>()
                        .TryAddTransient<PrivateFileManager, PrivateFileManager>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}