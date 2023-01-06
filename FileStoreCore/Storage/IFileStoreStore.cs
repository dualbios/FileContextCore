using FileStoreCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace FileStoreCore.Storage;

public interface IFileStoreStore
{
    IReadOnlyList<FileStoreTableSnapshot> GetTables(IEntityType entityType);
    int Execute(IList<IUpdateEntry> entries);
}