namespace FileStoreCore;

public interface IFileStoreIntegerValueGenerator
{
    void Bump(object[] row);
}