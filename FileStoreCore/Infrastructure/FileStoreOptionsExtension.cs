using FileStoreCore.Extensions;
using FileStoreCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FileStoreCore.Infrastructure;

public class FileStoreOptionsExtension : IDbContextOptionsExtension
{
    private readonly bool _nullabilityCheckEnabled;
    private readonly FileStoreScopedOptions _options = new();

    public FileStoreOptionsExtension(string databaseName = null, string location = null)
    {
        _options = new(databaseName, location);
    }

    public FileStoreDatabaseRoot? DatabaseRoot { get; set; }

    public DbContextOptionsExtensionInfo Info => new FileStoreOptionsExtensionInfo(this);

    public virtual bool IsNullabilityCheckEnabled => _nullabilityCheckEnabled;

    public FileStoreScopedOptions Options => _options;

    public void ApplyServices(IServiceCollection services)
    {
        services.AddEntityFrameworkFileStoreDatabase();
    }

    public void Validate(IDbContextOptions options)
    {
    }

    public class FileStoreOptionsExtensionInfo : DbContextOptionsExtensionInfo
    {
        public FileStoreOptionsExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
        {
        }

        public override bool IsDatabaseProvider { get; } = true;

        public override string LogFragment { get; } = nameof(FileStoreOptionsExtensionInfo);

        public override int GetServiceProviderHashCode()
        {
            return this.GetHashCode();
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["FileStoreOptionsExtensionInfo:DebugInfo"] = GetServiceProviderHashCode().ToString();
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return false;
        }
    }
}