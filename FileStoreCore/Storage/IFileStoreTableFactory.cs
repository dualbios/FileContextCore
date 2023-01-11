using Microsoft.EntityFrameworkCore.Metadata;

namespace kDg.FileBaseContext.Storage;

public interface IFileStoreTableFactory
{
    IFileStoreTable Create(IEntityType entityType, IFileStoreTable baseTable);
}