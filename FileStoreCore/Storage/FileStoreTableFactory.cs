using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Concurrent;
using System.Reflection;

namespace FileStoreCore.Storage;

public class FileStoreTableFactory
{
    private readonly FileStoreFileManager _fileManager;
    private readonly ConcurrentDictionary<IKey, Func<IFileStoreTable>> _factories = new ConcurrentDictionary<IKey, Func<IFileStoreTable>>();
    public FileStoreTableFactory(FileStoreFileManager fileManager)
    {
        _fileManager = fileManager;
    }

    public virtual IFileStoreTable Create(IEntityType entityType)
    {
        return _factories.GetOrAdd(entityType.FindPrimaryKey(), key => Create(key))();
    }

    private Func<IFileStoreTable> Create(IKey key)
    {
        return (Func<IFileStoreTable>)typeof(FileStoreTableFactory).GetTypeInfo()
            .GetDeclaredMethod(nameof(CreateFactory))
            .MakeGenericMethod(key.GetKeyType())
            .Invoke(null, new object[] { key, key.DeclaringEntityType, _fileManager });
    }

    private static Func<IFileStoreTable> CreateFactory<TKey>(IKey key, IEntityType entityType, FileStoreFileManager fileManager)
    {
        return () => new FileStoreTable<TKey>(entityType, fileManager);
    }
}