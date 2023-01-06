using Microsoft.EntityFrameworkCore.Metadata;

namespace FileStoreCore.Infrastructure;

public class FileStoreTableSnapshot
{
    public FileStoreTableSnapshot(IEntityType entityType, IReadOnlyList<object[]> rows)
    {
        EntityType = entityType;
        Rows = rows;
    }

    public virtual IEntityType EntityType { get; }

    public virtual IReadOnlyList<object[]> Rows { get; }
}