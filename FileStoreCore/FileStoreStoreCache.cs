﻿using FileStoreCore.Storage;
using System.Collections.Concurrent;

namespace FileStoreCore;

public class FileStoreStoreCache : IFileStoreStoreCache
{
    private readonly IServiceProvider _serviceProvider;

    //private readonly IFileStoreTableFactory _tableFactory;
    private readonly bool _useNameMatching;
    private readonly ConcurrentDictionary<IFileStoreScopedOptions, IFileStoreStore> _namedStores;

    public FileStoreStoreCache(
        IServiceProvider serviceProvider,
        IFileStoreSingletonOptions? options)
    {
        _serviceProvider = serviceProvider;
        //_tableFactory = tableFactory;

        if (options?.DatabaseRoot != null)
        {
            _useNameMatching = true;

            LazyInitializer.EnsureInitialized(
                ref options.DatabaseRoot.Instance,
                () => new ConcurrentDictionary<IFileStoreScopedOptions, IFileStoreStore>());

            _namedStores = (ConcurrentDictionary<IFileStoreScopedOptions, IFileStoreStore>)options.DatabaseRoot.Instance;
        }
        else
        {
            _namedStores = new ConcurrentDictionary<IFileStoreScopedOptions, IFileStoreStore>();
        }
    }

    public virtual IFileStoreStore GetStore(IFileStoreScopedOptions options)
    {
        return _namedStores.GetOrAdd(options, _ => new FileStoreStore(new FileStoreTableFactory(_serviceProvider, options), _useNameMatching));
    }
}