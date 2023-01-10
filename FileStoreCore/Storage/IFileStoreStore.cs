using FileStoreCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace FileStoreCore.Storage;

public interface IFileStoreStore
{
    bool Clear();

    bool EnsureCreated(IUpdateAdapterFactory updateAdapterFactory, IModel designModel);

    int ExecuteTransaction(IList<IUpdateEntry> entries);

    FileStoreIntegerValueGenerator<TProperty> GetIntegerValueGenerator<TProperty>(IProperty property);

    IReadOnlyList<FileStoreTableSnapshot> GetTables(IEntityType entityType);
}