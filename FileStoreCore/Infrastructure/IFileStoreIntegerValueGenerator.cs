namespace FileStoreCore.Infrastructure;

public interface IFileStoreIntegerValueGenerator
{
    void Bump(object[] row);
}