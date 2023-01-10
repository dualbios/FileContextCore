using FileStoreCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace FileStoreCore.Storage;

public interface IFileStoreTable
{
    IEnumerable<object?[]> Rows { get; }

    void Create(IUpdateEntry entry);

    void Save();

    //IReadOnlyList<InMemoryTableSnapshot> GetTables(IEntityType entityType);
    IReadOnlyList<object[]> SnapshotRows();
    void Delete(IUpdateEntry entry);
    void Update(IUpdateEntry entry);

    FileStoreIntegerValueGenerator<TProperty> GetIntegerValueGenerator<TProperty>(
        IProperty property,
        IReadOnlyList<IFileStoreTable> tables);
}