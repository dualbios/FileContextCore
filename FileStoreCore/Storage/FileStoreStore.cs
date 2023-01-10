using FileStoreCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;

namespace FileStoreCore.Storage;

internal class FileStoreStore : IFileStoreStore
{
    private object _lock = new();
    private Dictionary<object, IFileStoreTable>? _tables;
    private readonly bool _useNameMatching;
    private readonly IFileStoreTableFactory _tableFactory;

    public FileStoreStore(IFileStoreTableFactory fileStoreTableFactory, bool useNameMatching)
    {
        _tableFactory = fileStoreTableFactory;
        _useNameMatching = useNameMatching;
    }

    public bool EnsureCreated(IUpdateAdapterFactory updateAdapterFactory, IModel designModel)
    {
        lock (_lock)
        {
            var valuesSeeded = _tables == null;
            if (valuesSeeded)
            {
                // ReSharper disable once AssignmentIsFullyDiscarded
                _tables = CreateTables();

                var updateAdapter = updateAdapterFactory.CreateStandalone();
                var entries = new List<IUpdateEntry>();
                foreach (var entityType in updateAdapter.Model.GetEntityTypes())
                {
                    foreach (var targetSeed in entityType.GetSeedData())
                    {
                        var entry = updateAdapter.CreateEntry(targetSeed, entityType);
                        entry.EntityState = EntityState.Added;
                        entries.Add(entry);
                    }
                }

                ExecuteTransaction(entries);
            }

            return valuesSeeded;
        }
    }

    private static Dictionary<object, IFileStoreTable> CreateTables()
    {
        return new Dictionary<object, IFileStoreTable>();
    }

    public bool Clear()
    {
        lock (_lock)
        {
            if (_tables == null)
            {
                return false;
            }

            _tables = null;

            return true;
        }
    }

    private IFileStoreTable EnsureTable(IEntityType entityType)
    {
        _tables ??= CreateTables();

        IFileStoreTable? baseTable = null;

        var entityTypes = entityType.GetAllBaseTypesInclusive();
        foreach (var currentEntityType in entityTypes)
        {
            var key = _useNameMatching ? (object)currentEntityType.Name : currentEntityType;
            if (!_tables.TryGetValue(key, out var table))
            {
                _tables.Add(key, table = _tableFactory.Create(currentEntityType, baseTable));
            }

            baseTable = table;
        }

        return _tables[_useNameMatching ? entityType.Name : entityType];
    }

    public IReadOnlyList<FileStoreTableSnapshot> GetTables(IEntityType entityType)
    {
        var data = new List<FileStoreTableSnapshot>();
        lock (_lock)
        {
            foreach (var et in entityType.GetDerivedTypesInclusive().Where(et => !et.IsAbstract()))
            {
                var table = EnsureTable(et);
                data.Add(new FileStoreTableSnapshot(et, table.SnapshotRows()));
            }
        }

        return data;
    }

    public FileStoreIntegerValueGenerator<TProperty> GetIntegerValueGenerator<TProperty>(IProperty property)
    {
        lock (_lock)
        {
            var entityType = property.DeclaringEntityType;

            return EnsureTable(entityType).GetIntegerValueGenerator<TProperty>(
                property,
                entityType.GetDerivedTypesInclusive().Select(type => EnsureTable(type)).ToArray());
        }
    }

    public int ExecuteTransaction(IList<IUpdateEntry> entries)
    {
        var rowsAffected = 0;

        lock (_lock)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var entityType = entry.EntityType;

                var table = EnsureTable(entityType);

                if (entry.SharedIdentityEntry != null)
                {
                    if (entry.EntityState == EntityState.Deleted)
                    {
                        continue;
                    }

                    table.Delete(entry.SharedIdentityEntry);
                }

                switch (entry.EntityState)
                {
                    case EntityState.Added:
                        table.Create(entry);
                        break;

                    case EntityState.Deleted:
                        table.Delete(entry);
                        break;

                    case EntityState.Modified:
                        table.Update(entry);
                        break;
                }

                rowsAffected++;
            }
        }

        foreach (IFileStoreTable table in entries
                     .Select(x => EnsureTable(x.EntityType))
                     .GroupBy(x => x)
                     .Select(x => x.Key))
        {
            table.Save();
        }

        return rowsAffected;
    }

    //public int Execute(IList<IUpdateEntry> entries)
    //{
    //    var rowsAffected = 0;

    //    {
    //        for (var i = 0; i < entries.Count; i++)
    //        {
    //            var entry = entries[i];
    //            var entityType = entry.EntityType;

    //            Debug.Assert(!entityType.IsAbstract());

    //            IFileStoreTable table = _fileStoreTablesManager.GetorCreateTable(entityType);

    //            if (entry.SharedIdentityEntry != null)
    //            {
    //                if (entry.EntityState == EntityState.Deleted)
    //                {
    //                    continue;
    //                }

    //                table.Delete(entry);
    //            }

    //            switch (entry.EntityState)
    //            {
    //                case EntityState.Added:
    //                    table.Create(entry);
    //                    break;
    //                case EntityState.Deleted:
    //                    table.Delete(entry);
    //                    break;
    //                case EntityState.Modified:
    //                    table.Update(entry);
    //                    break;
    //            }

    //            rowsAffected++;
    //        }

    //        foreach (IFileStoreTable table in entries
    //                     .Select(x => _fileStoreTablesManager.GetorCreateTable(x.EntityType))
    //                     .GroupBy(x => x)
    //                     .Select(x => x.Key))
    //        {
    //            table.Save();
    //        }
    //    }

    //    return rowsAffected;
    //}
}