namespace kDg.FileBaseContext.Infrastructure;

public interface IFileStoreIntegerValueGenerator
{
    void Bump(object[] row);
}