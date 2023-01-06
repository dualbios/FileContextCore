using FileStoreCore.Serializers;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FileStoreCore.Storage;

public interface IFileStoreFileManager
{
    void Init();
    string GetFileName(IEntityType _entityType);
    Dictionary<TKey, object[]> Load<TKey>(IEntityType _entityType, ISerializer serializer);
    void Save<TKey>(IEntityType _entityType, Dictionary<TKey, object[]> objectsMap, ISerializer serializer);
}