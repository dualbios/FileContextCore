using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace FileStoreCore.Infrastructure;

public class FileStoreConventionSetBuilder : ProviderConventionSetBuilder
{
    public FileStoreConventionSetBuilder(ProviderConventionSetBuilderDependencies dependencies) : base(dependencies)
    {
    }
}