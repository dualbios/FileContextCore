using kDg.FileBaseContext.Serializers;
using Microsoft.EntityFrameworkCore.Metadata;

namespace kDg.FileBaseContext.Storage;

public interface IFileStoreFileManager
{
    void Init(IFileStoreScopedOptions _options);

    string GetFileName(IEntityType _entityType);

    Dictionary<TKey, object[]> Load<TKey>(IEntityType _entityType, ISerializer serializer);

    void Save<TKey>(IEntityType _entityType, Dictionary<TKey, object[]> objectsMap, ISerializer serializer);
}