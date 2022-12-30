using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics.CodeAnalysis;

namespace FileStoreCore.Infrastructure;

public class FileSoreTypeMapping : CoreTypeMapping
{
    public FileSoreTypeMapping(
        Type clrType,
        ValueComparer comparer = null,
        ValueComparer keyComparer = null,
        ValueComparer structuralComparer = null)
        : base(
            new CoreTypeMappingParameters(
                clrType,
                converter: null,
                comparer,
                keyComparer,
                structuralComparer,
                valueGeneratorFactory: null))
    {
    }

    private FileSoreTypeMapping(CoreTypeMappingParameters parameters)
        : base(parameters)
    {
    }


    public override CoreTypeMapping Clone(ValueConverter converter)
    {
        return new FileSoreTypeMapping(Parameters.WithComposedConverter(converter));
    }
}