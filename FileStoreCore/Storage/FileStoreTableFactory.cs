using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Concurrent;
using System.Reflection;

namespace FileStoreCore.Storage;

public class FileStoreTableFactory : IFileStoreTableFactory
{
    private readonly ConcurrentDictionary<(IEntityType EntityType, IFileStoreTable BaseTable), Func<IFileStoreTable>> _factories = new();
    private readonly IFileStoreScopedOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public FileStoreTableFactory(IServiceProvider serviceProvider, IFileStoreScopedOptions options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    public virtual IFileStoreTable Create(IEntityType entityType, IFileStoreTable baseTable)
        => _factories.GetOrAdd((entityType, baseTable), e => CreateTable(e.EntityType, e.BaseTable))();

    private static Func<IFileStoreTable> CreateFactory<TKey>(
        IEntityType entityType,
        IServiceProvider serviceProvider,
        IFileStoreScopedOptions options)
        where TKey : notnull
    {
        return () => new FileStoreTable<TKey>(entityType, serviceProvider, options);
    }

    private Func<IFileStoreTable> CreateTable(IEntityType entityType, IFileStoreTable baseTable)
    {
        return (Func<IFileStoreTable>)typeof(FileStoreTableFactory).GetTypeInfo()
            .GetDeclaredMethod(nameof(CreateFactory))!
            .MakeGenericMethod(entityType.FindPrimaryKey()!.GetKeyType())
            .Invoke(null, new object[] { entityType, _serviceProvider, _options })!;
    }
}