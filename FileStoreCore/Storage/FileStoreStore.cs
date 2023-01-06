using FileStoreCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using System.Diagnostics;

namespace FileStoreCore.Storage;

class FileStoreStore : IFileStoreStore
{
    private readonly FileStoreTablesManager _fileStoreTablesManager;

    public FileStoreStore(FileStoreTablesManager fileStoreTablesManager)
    {
        _fileStoreTablesManager = fileStoreTablesManager;
    }

    public IReadOnlyList<FileStoreTableSnapshot> GetTables(IEntityType entityType)
    {
        var data = new List<FileStoreTableSnapshot>();
        //lock (_lock)
        {
            foreach (var et in entityType.GetDerivedTypesInclusive().Where(et => !et.IsAbstract()))
            {
                //var key = _useNameMatching ? (object)et.Name : et;
                //var table = EnsureTable(key, et);

                IFileStoreTable table = _fileStoreTablesManager.GetorCreateTable(entityType);
                data.Add(new FileStoreTableSnapshot(et, table.SnapshotRows()));
            }
        }

        return data;
    }

    public int Execute(IList<IUpdateEntry> entries)
    {
        var rowsAffected = 0;

        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var entityType = entry.EntityType;

                Debug.Assert(!entityType.IsAbstract());

                var key = /*_useNameMatching ? (object)entityType.Name :*/ entityType;
                //var table = EnsureTable(key, entityType);
                //string tableName = entityType.Name;
                IFileStoreTable table = _fileStoreTablesManager.GetorCreateTable(entityType);

                if (entry.SharedIdentityEntry != null)
                {
                    if (entry.EntityState == EntityState.Deleted)
                    {
                        continue;
                    }

                    //table.Delete(entry);
                }

                switch (entry.EntityState)
                {
                    case EntityState.Added:
                        table.Create(entry);
                        break;
                    case EntityState.Deleted:
                        table.Delete(entry);
                        break;
                        //case EntityState.Modified:
                        //    table.Update(entry);
                        break;
                }

                rowsAffected++;
            }

            foreach (IFileStoreTable table in entries
                         .Select(x => _fileStoreTablesManager.GetorCreateTable(x.EntityType))
                         .GroupBy(x => x)
                         .Select(x => x.Key))
            {
                table.Save();
            }

            //foreach (KeyValuePair<object, IFileContextTable> table in _tables)
            //{
            //    table.Value.Save();
            //}
        }


        return rowsAffected;
    }
}