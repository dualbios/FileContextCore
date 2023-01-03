using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using System.Diagnostics;

namespace FileStoreCore.Storage;

public class FileStoreDatabase : Database
{
    private readonly FileStoreTablesManager _fileStoreTablesManager;

    public FileStoreDatabase(
        DatabaseDependencies dependencies,
        FileStoreTablesManager fileStoreTablesManager) : base(dependencies)
    {
        _fileStoreTablesManager = fileStoreTablesManager;
    }

    public override int SaveChanges(IList<IUpdateEntry> entries)
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
                    //case EntityState.Deleted:
                    //    table.Delete(entry);
                    //    break;
                    //case EntityState.Modified:
                    //    table.Update(entry);
                        break;
                }

                rowsAffected++;
            }

            foreach (IFileStoreTable table in entries
                         .Select(x=>_fileStoreTablesManager.GetorCreateTable(x.EntityType))
                         .GroupBy(x=>x)
                         .Select(x=>x.Key))
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


    public override Task<int> SaveChangesAsync(IList<IUpdateEntry> entries, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}