using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace FileStoreCore.Storage;

public class FileStoreDatabase : Database
{
    public FileStoreDatabase(DatabaseDependencies dependencies) : base(dependencies)
    {
    }

    public override int SaveChanges(IList<IUpdateEntry> entries)
    {
        throw new NotImplementedException();
    }

    public override Task<int> SaveChangesAsync(IList<IUpdateEntry> entries, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}