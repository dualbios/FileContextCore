using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using System.Diagnostics;

namespace FileStoreCore.Storage;

public class FileStoreDatabase : Database
{
    private readonly IFileStoreStore _fileStoreStore;

    public FileStoreDatabase(
        DatabaseDependencies dependencies,
        IFileStoreStore fileStoreStore) : base(dependencies)
    {
        _fileStoreStore = fileStoreStore;
    }

    public override int SaveChanges(IList<IUpdateEntry> entries)
    {
        return _fileStoreStore.Execute(entries);
    }


    public override Task<int> SaveChangesAsync(IList<IUpdateEntry> entries, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}