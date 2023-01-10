using Microsoft.EntityFrameworkCore.Metadata;

namespace FileStoreCore.Storage;

public class FileStoreTablesManager
{
    //private readonly FileStoreTableFactory _fileStoreTableFactory;
    //private IDictionary<IEntityType, IFileStoreTable> _tables = new Dictionary<IEntityType, IFileStoreTable>();

    //public FileStoreTablesManager(FileStoreTableFactory fileStoreTableFactory)
    //{
    //    _fileStoreTableFactory = fileStoreTableFactory;
    //}

    //public IFileStoreTable GetorCreateTable(IEntityType entityType)
    //{
    //    if (!_tables.ContainsKey(entityType))
    //    {
    //        _tables[entityType] = _fileStoreTableFactory.Create(entityType);
    //    }

    //    return _tables[entityType];
    //}
}