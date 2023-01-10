using FileStoreCore.Serializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.String;

namespace FileStoreCore.Storage;

public class FileStoreFileManager : IFileStoreFileManager
{
    private string _databasename = "";
    private string _filetype = "json";
    private string? _location;

    public FileStoreFileManager()
    {
    }

    public void Init(IFileStoreScopedOptions options)
    {
        _databasename = options.DatabaseName;
        _location = options.Location;
    }

    public string GetFileName(IEntityType _entityType)
    {
        string name = _entityType.GetTableName().GetValidFileName();

        string path = string.IsNullOrEmpty(_location)
            ? Path.Combine(AppContext.BaseDirectory, "appdata", _databasename)
            : _location;

        Directory.CreateDirectory(path);

        return Path.Combine(path, name + "." + _filetype);
    }

    public Dictionary<TKey, object[]> Load<TKey>(IEntityType _entityType, ISerializer serializer)
    {
        string path = GetFileName(_entityType);

        string content = "";
        if (File.Exists(path))
        {
            content = File.ReadAllText(path);
        }

        Dictionary<TKey, object[]> rows = new Dictionary<TKey, object[]>();
        serializer.Deserialize(content, rows);

        return rows;
    }

    public void Save<TKey>(IEntityType _entityType, Dictionary<TKey, object[]> objectsMap, ISerializer serializer)
    {
        string content = serializer.Serialize(objectsMap);
        string path = GetFileName(_entityType);
        File.WriteAllText(path, content);
    }
}