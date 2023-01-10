// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace FileStoreCore.Infrastructure.Query.Internal;

public readonly struct AnonymousObject
{
    public static readonly ConstructorInfo AnonymousObjectCtor
        = typeof(AnonymousObject).GetTypeInfo()
            .DeclaredConstructors
            .Single(c => c.GetParameters().Length == 1);

    private readonly object[] _values;

    public AnonymousObject(object[] values)
    {
        _values = values;
    }

    public static bool operator !=(AnonymousObject x, AnonymousObject y)
    {
        return !x.Equals(y);
    }

    public static bool operator ==(AnonymousObject x, AnonymousObject y)
    {
        return x.Equals(y);
    }

    public override bool Equals(object obj)
    {
        return obj is not null
               && (obj is AnonymousObject anonymousObject
                   && _values.SequenceEqual(anonymousObject._values));
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var value in _values)
        {
            hash.Add(value);
        }

        return hash.ToHashCode();
    }
}