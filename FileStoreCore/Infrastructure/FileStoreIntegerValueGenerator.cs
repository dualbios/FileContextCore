﻿using System.Globalization;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace kDg.FileBaseContext.Infrastructure;

public class FileStoreIntegerValueGenerator<TValue> : ValueGenerator<TValue>, IFileStoreIntegerValueGenerator
{
    private readonly int _propertyIndex;
    private long _current;

    public FileStoreIntegerValueGenerator(int propertyIndex)
    {
        _propertyIndex = propertyIndex;
    }

    public override bool GeneratesTemporaryValues
    {
        get { return false; }
    }

    public virtual void Bump(object[] row)
    {
        long newValue = (long)Convert.ChangeType(row[_propertyIndex], typeof(long));

        if (_current < newValue)
        {
            Interlocked.Exchange(ref _current, newValue);
        }
    }

    public override TValue Next(EntityEntry entry)
    {
        return (TValue)Convert.ChangeType(Interlocked.Increment(ref _current), typeof(TValue), CultureInfo.InvariantCulture);
    }
}