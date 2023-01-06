using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace FileStoreCore.Storage;

public interface IFileStoreTable
{
    void Create(IUpdateEntry entry);

    void Save();

    //IReadOnlyList<InMemoryTableSnapshot> GetTables(IEntityType entityType);
    IReadOnlyList<object[]> SnapshotRows();
    void Delete(IUpdateEntry entry);
    void Update(IUpdateEntry entry);
}