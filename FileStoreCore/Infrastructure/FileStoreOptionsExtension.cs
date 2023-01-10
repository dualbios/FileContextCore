using FileStoreCore.Extensions;
using FileStoreCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FileStoreCore.Infrastructure;

public class FileStoreOptionsExtension : IDbContextOptionsExtension
{
    private bool _nullabilityCheckEnabled;
    private FileStoreScopedOptions _options;

    public FileStoreOptionsExtension()
    {
        _options = new FileStoreScopedOptions();
    }

    public void ApplyServices(IServiceCollection services)
    {
        services.AddEntityFrameworkFileStoreDatabase();
    }

    public void Validate(IDbContextOptions options)
    {
        
    }

    public DbContextOptionsExtensionInfo Info
    {
        get { return new FileStoreOptionsExtensionInfo(this); }
    }

    public string StoreName {
        get
        {
            return Options.DatabaseName;
        }
        set
        {
            Options.DatabaseName = value;
        }
    }
    public string Location { get; set; }
    public virtual bool IsNullabilityCheckEnabled => _nullabilityCheckEnabled;

    public FileStoreDatabaseRoot? DatabaseRoot { get; set; }
    public virtual FileStoreScopedOptions Options => _options;

    public class FileStoreOptionsExtensionInfo : DbContextOptionsExtensionInfo
    {
        public FileStoreOptionsExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
        {
        }

        public override int GetServiceProviderHashCode()
        {
            return this.GetHashCode();
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return false;
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["FileStoreOptionsExtensionInfo:DebugInfo"] = GetServiceProviderHashCode().ToString();
        }

        public override bool IsDatabaseProvider { get; } = true;
        public override string LogFragment { get; } = nameof(FileStoreOptionsExtensionInfo);
    }
}