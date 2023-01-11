using System.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace kDg.FileBaseContext.Infrastructure;

public class FileStoreTypeMappingSource : TypeMappingSource
{
    public FileStoreTypeMappingSource(TypeMappingSourceDependencies dependencies) : base(dependencies)
    {
    }

    protected override CoreTypeMapping FindMapping(in TypeMappingInfo mappingInfo)
    {
        Type clrType = mappingInfo.ClrType;
        Debug.Assert(clrType != null);

        if (clrType.IsValueType
            || clrType == typeof(string))
        {
            return new FileSoreTypeMapping(clrType);
        }

        if (clrType == typeof(byte[]))
        {
            return new FileSoreTypeMapping(clrType, structuralComparer: new ArrayStructuralComparer<byte>());
        }

        if (clrType.FullName == "NetTopologySuite.Geometries.Geometry"
            || clrType.GetBaseTypes().Any(t => t.FullName == "NetTopologySuite.Geometries.Geometry"))
        {
            ValueComparer comparer = (ValueComparer)Activator.CreateInstance(typeof(GeometryValueComparer<>).MakeGenericType(clrType));

            return new FileSoreTypeMapping(
                clrType,
                comparer,
                comparer,
                comparer);
        }

        return base.FindMapping(mappingInfo);
    }
}