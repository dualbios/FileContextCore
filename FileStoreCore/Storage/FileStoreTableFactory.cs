using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Concurrent;
using System.Reflection;

namespace FileStoreCore.Storage;

public interface IFileStoreTableFactory
{
    IFileStoreTable Create(IEntityType entityType, IFileStoreTable? baseTable);
}

public class FileStoreTableFactory : IFileStoreTableFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileStoreScopedOptions _options;
    private readonly bool _sensitiveLoggingEnabled;
    private readonly bool _nullabilityCheckEnabled;

    private readonly ConcurrentDictionary<(IEntityType EntityType, IFileStoreTable? BaseTable), Func<IFileStoreTable>> _factories = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public FileStoreTableFactory(IServiceProvider serviceProvider, IFileStoreScopedOptions options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _sensitiveLoggingEnabled = /*loggingOptions.IsSensitiveDataLoggingEnabled*/false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IFileStoreTable Create(IEntityType entityType, IFileStoreTable? baseTable)
        => _factories.GetOrAdd((entityType, baseTable), e => CreateTable(e.EntityType, e.BaseTable))();

    private Func<IFileStoreTable> CreateTable(IEntityType entityType, IFileStoreTable? baseTable)
        => (Func<IFileStoreTable>)typeof(FileStoreTableFactory).GetTypeInfo()
            .GetDeclaredMethod(nameof(CreateFactory))!
            .MakeGenericMethod(entityType.FindPrimaryKey()!.GetKeyType())
            .Invoke(null, new object?[] { entityType, _serviceProvider, _options})!;

    private static Func<IFileStoreTable> CreateFactory<TKey>(
        IEntityType entityType,
        IServiceProvider serviceProvider,
        IFileStoreScopedOptions options)
        where TKey : notnull
        => () => new FileStoreTable<TKey>(entityType, serviceProvider, options);
}