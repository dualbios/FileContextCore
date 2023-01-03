using FileStoreCore.Serializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Update;
using System.String;

namespace FileStoreCore.Storage;

public interface IFileStoreTable
{
    void Create(IUpdateEntry entry);

    void Save();
}

public class FileStoreTable<TKey> : IFileStoreTable
{
    private readonly IEntityType _entityType;
    private readonly FileStoreFileManager _fileManager;
    private readonly IKey _primaryKey;
    private readonly Dictionary<TKey, object[]> _rows = new Dictionary<TKey, object[]>();
    private readonly ISerializer _serializer;
    private IPrincipalKeyValueFactory<TKey> _keyValueFactory = null;

    public FileStoreTable(IEntityType entityType, FileStoreFileManager fileManager)
    {
        _entityType = entityType;
        _fileManager = fileManager;
        _primaryKey = entityType.FindPrimaryKey();
        _keyValueFactory = _primaryKey.GetPrincipalKeyValueFactory<TKey>();
        _serializer = new JsonDataSerializer(entityType, _keyValueFactory);

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
        _fileManager.Load(_entityType, _rows, _serializer);
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
}

public class FileStoreFileManager
{
    private string _databasename = "";
    private string _filetype = "json";
    private string? _location;

    public string GetFileName(IEntityType _entityType)
    {
        string name = _entityType.GetTableName().GetValidFileName();

        string path = string.IsNullOrEmpty(_location)
            ? Path.Combine(AppContext.BaseDirectory, "appdata", _databasename)
            : _location;

        Directory.CreateDirectory(path);

        return Path.Combine(path, name + "." + _filetype);
    }

    public void Load<TKey>(IEntityType _entityType, Dictionary<TKey, object[]> rows, ISerializer serializer)
    {
        string path = GetFileName(_entityType);

        string content = "";
        if (File.Exists(path))
        {
            content = File.ReadAllText(path);
        }

        rows = new Dictionary<TKey, object[]>();
        serializer.Deserialize(content, rows);
    }

    public void Save<TKey>(IEntityType _entityType, Dictionary<TKey, object[]> objectsMap, ISerializer serializer)
    {
        string content = serializer.Serialize(objectsMap);
        string path = GetFileName(_entityType);
        File.WriteAllText(path, content);
    }
}