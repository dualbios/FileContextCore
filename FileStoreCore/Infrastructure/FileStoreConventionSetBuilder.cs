using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace kDg.FileBaseContext.Infrastructure;

public class FileStoreConventionSetBuilder : ProviderConventionSetBuilder
{
    public FileStoreConventionSetBuilder(ProviderConventionSetBuilderDependencies dependencies) : base(dependencies)
    {
    }
}