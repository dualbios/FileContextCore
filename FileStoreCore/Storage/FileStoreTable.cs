﻿using FileStoreCore.Serializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Update;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using FileStoreCore.Infrastructure;

namespace FileStoreCore.Storage;

public class FileStoreTable<TKey> : IFileStoreTable
{
    private readonly IEntityType _entityType;
    private readonly IFileStoreFileManager _fileManager;
    private readonly IKey _primaryKey;
    private Dictionary<TKey, object[]> _rows = new Dictionary<TKey, object[]>();
    private readonly ISerializer _serializer;
    private IPrincipalKeyValueFactory<TKey> _keyValueFactory = null;

    private Dictionary<int, IFileStoreIntegerValueGenerator> _integerGenerators;


    public FileStoreTable(IEntityType entityType, IFileStoreFileManager fileManager)
    {
        _entityType = entityType;
        _fileManager = fileManager;
        _primaryKey = entityType.FindPrimaryKey();
        _keyValueFactory = _primaryKey.GetPrincipalKeyValueFactory<TKey>();
        _serializer = new JsonDataSerializer(entityType, _keyValueFactory);

        _fileManager.Init();

        Load();
    }

    //private Dictionary<TKey, object[]> Init()
    //{
    //    //Dictionary<TKey, object[]> newList = new Dictionary<TKey, object[]>(_keyValueFactory.EqualityComparer);
    //    //return ConvertFromProvider(_storeManager.Deserialize(newList));
    //}

    public void Create(IUpdateEntry entry)
    {
        var row = entry.EntityType.GetProperties()
            .Select(p => SnapshotValue(p, GetStructuralComparer(p), entry))
            .ToArray();

        _rows.Add(CreateKey(entry), row);
    }

    public void Load()
    {
        _rows = _fileManager.Load<TKey>(_entityType, _serializer);
    }

    public void Save()
    {
        _fileManager.Save(_entityType, ConvertToProvider(_rows), _serializer);
    }

    private static ValueComparer GetStructuralComparer(IProperty p)
    {
        return p.GetValueComparer();
        //return p.GetStructuralValueComparer() ?? p.FindTypeMapping()?.StructuralComparer;
    }

    private static object SnapshotValue(IProperty property, ValueComparer comparer, IUpdateEntry entry)
    {
        return SnapshotValue(comparer, entry.GetCurrentValue(property));
    }

    private static object SnapshotValue(ValueComparer comparer, object value)
    {
        return comparer == null ? value : comparer.Snapshot(value);
    }

    private Dictionary<TKey, object[]> ApplyValueConverter(Dictionary<TKey, object[]> list, Func<ValueConverter, Func<object, object>> conversionFunc)
    {
        var result = new Dictionary<TKey, object[]>(_keyValueFactory.EqualityComparer);
        var converters = _entityType.GetProperties().Select(p => p.GetValueConverter()).ToArray();
        foreach (var keyValuePair in list)
        {
            result[keyValuePair.Key] = keyValuePair.Value.Select((value, index) =>
            {
                var converter = converters[index];
                return converter == null ? value : conversionFunc(converter)(value);
            }).ToArray();
        }

        return result;
    }

    private Dictionary<TKey, object[]> ConvertToProvider(Dictionary<TKey, object[]> list)
    {
        return ApplyValueConverter(list, converter => converter.ConvertToProvider);
    }

    private TKey CreateKey(IUpdateEntry entry)
    {
        return _keyValueFactory.CreateFromCurrentValues(entry);
    }

    public virtual IReadOnlyList<object[]> SnapshotRows()
    {
        return new ReadOnlyCollection<object[]>(_rows.Values.ToList());
    }

    public void Delete(IUpdateEntry entry)
    {
        var key = CreateKey(entry);

        if (_rows.TryGetValue(key, out object[] value))
        {
            var properties = entry.EntityType.GetProperties().ToList();
            var concurrencyConflicts = new Dictionary<IProperty, object>();

            for (var index = 0; index < properties.Count; index++)
            {
                IsConcurrencyConflict(entry, properties[index], value[index], concurrencyConflicts);
            }

            if (concurrencyConflicts.Count > 0)
            {
                ThrowUpdateConcurrencyException(entry, concurrencyConflicts);
            }

            _rows.Remove(key);
        }
        else
        {
            throw new DbUpdateConcurrencyException("UpdateConcurrencyException", new[] { entry });
            //throw new DbUpdateConcurrencyException(FileContextStrings.UpdateConcurrencyException, new[] { entry });
        }
    }

    public void Update(IUpdateEntry entry)
    {
        var key = CreateKey(entry);

        if (_rows.ContainsKey(key))
        {
            var properties = entry.EntityType.GetProperties().ToList();
            var comparers = GetStructuralComparers(properties);
            var valueBuffer = new object[properties.Count];
            var concurrencyConflicts = new Dictionary<IProperty, object>();

            for (var index = 0; index < valueBuffer.Length; index++)
            {
                if (IsConcurrencyConflict(entry, properties[index], _rows[key][index], concurrencyConflicts))
                {
                    continue;
                }

                valueBuffer[index] = entry.IsModified(properties[index])
                    ? SnapshotValue(properties[index], comparers[index], entry)
                    : _rows[key][index];
            }

            if (concurrencyConflicts.Count > 0)
            {
                ThrowUpdateConcurrencyException(entry, concurrencyConflicts);
            }

            _rows[key] = valueBuffer;

            //BumpValueGenerators(valueBuffer);
        }
        else
        {
            throw new DbUpdateConcurrencyException("FileContextStrings.UpdateConcurrencyException", new[] { entry });
        }
    }

    private void BumpValueGenerators(object[] row)
    {
        if (_integerGenerators != null)
        {
            foreach (var generator in _integerGenerators.Values)
            {
                generator.Bump(row);
            }
        }
    }

    private static List<ValueComparer> GetStructuralComparers(IEnumerable<IProperty> properties)
    {
        return properties.Select(GetStructuralComparer).ToList();
    }


    private static bool IsConcurrencyConflict(
        IUpdateEntry entry,
        IProperty property,
        object rowValue,
        Dictionary<IProperty, object> concurrencyConflicts)
    {
        if (property.IsConcurrencyToken
            && !StructuralComparisons.StructuralEqualityComparer.Equals(
                rowValue,
                entry.GetOriginalValue(property)))
        {
            concurrencyConflicts.Add(property, rowValue);

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Throws an exception indicating that concurrency conflicts were detected.
    /// </summary>
    /// <param name="entry"> The update entry which resulted in the conflict(s). </param>
    /// <param name="concurrencyConflicts"> The conflicting properties with their associated database values. </param>
    protected virtual void ThrowUpdateConcurrencyException([NotNull] IUpdateEntry entry, [NotNull] Dictionary<IProperty, object> concurrencyConflicts)
    {
        //Check.NotNull(entry, nameof(entry));
        //Check.NotNull(concurrencyConflicts, nameof(concurrencyConflicts));

        //if (_sensitiveLoggingEnabled)
        //{
        //    throw new DbUpdateConcurrencyException(
        //        FileContextStrings.UpdateConcurrencyTokenExceptionSensitive(
        //            entry.EntityType.DisplayName(),
        //            entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey().Properties),
        //            entry.BuildOriginalValuesString(concurrencyConflicts.Keys),
        //            "{" + string.Join(", ", concurrencyConflicts.Select(c => c.Key.Name + ": " + Convert.ToString(c.Value, CultureInfo.InvariantCulture))) + "}"),
        //        new[] { entry });
        //}

        throw new DbUpdateConcurrencyException(
            //FileContextStrings.UpdateConcurrencyTokenException(entry.EntityType.DisplayName(), concurrencyConflicts.Keys.Format()),
            $"{entry.EntityType.DisplayName()},{concurrencyConflicts.Keys.Format()}",
            new[] { entry });
    }
}