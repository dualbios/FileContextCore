using FileStoreCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace FileStoreCore.Storage;

public interface IFileStoreStore
{
    bool EnsureCreated(
        IUpdateAdapterFactory updateAdapterFactory,
        IModel designModel);

    bool Clear();

    IReadOnlyList<FileStoreTableSnapshot> GetTables(IEntityType entityType);

    FileStoreIntegerValueGenerator<TProperty> GetIntegerValueGenerator<TProperty>(IProperty property);

    int ExecuteTransaction(IList<IUpdateEntry> entries);
}