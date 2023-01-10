using Microsoft.EntityFrameworkCore.Metadata;

namespace FileStoreCore.Storage;

public interface IFileStoreTableFactory
{
    IFileStoreTable Create(IEntityType entityType, IFileStoreTable? baseTable);
}