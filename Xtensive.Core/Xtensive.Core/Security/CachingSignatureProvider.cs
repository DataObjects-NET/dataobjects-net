// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

using System;
using System.Runtime.Serialization;
using Xtensive.Caching;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Security
{
  /// <summary>
  /// Caching signature provider: caches decrypted signatures
  /// to improve performance of signature checks.
  /// </summary>
  [Serializable]
  public class CachingSignatureProvider : ISignatureProvider,
    IDeserializationCallback
  {
    private int cacheSize;
    private ISignatureProvider signatureProvider;
    [NonSerialized]
    private ICache<string, Pair<string, string>> cache;
    [NonSerialized]
    private object _lock = new object();

    #region Properties: CacheSize, Cache, SignatureProvider

    /// <summary>
    /// Gets the <see cref="Cache"/> capacity.
    /// </summary>
    public int CacheSize {
      get { return cacheSize; }
    }

    /// <summary>
    /// Gets the cache.
    /// </summary>
    protected ICache<string, Pair<string, string>> Cache {
      get { return cache; }
      set { cache = value; }
    }

    /// <summary>
    /// Gets the underlying signature provider.
    /// </summary>
    protected ISignatureProvider SignatureProvider {
      get { return signatureProvider; }
    }

    #endregion

    /// <inheritdoc/>
    public string AddSignature(string token)
    {
      return signatureProvider.AddSignature(token);
    }

    /// <inheritdoc/>
    public string RemoveSignature(string signedToken)
    {
      Pair<string, string> cached;
      lock (_lock) {
        if (Cache.TryGetItem(signedToken, true, out cached))
          return cached.Second;
      }
      string token = SignatureProvider.RemoveSignature(signedToken);
      if (token.IsNullOrEmpty())
        return null;
      lock (_lock) {
        Cache.Add(new Pair<string, string>(signedToken, token));
        return token;
      }
    }

    /// <summary>
    /// Initializes this instance (creates <see cref="Cache"/> object).
    /// </summary>
    protected virtual void Initialize()
    {
      Cache = new LruCache<string, Pair<string, string>>(CacheSize, p => p.First);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="cacheSize">Size of the <see cref="Cache"/>.</param>
    /// <param name="signatureProvider">The underlying signature provider.</param>
    public CachingSignatureProvider(int cacheSize, ISignatureProvider signatureProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(signatureProvider, "signatureProvider");

      this.cacheSize = cacheSize;
      this.signatureProvider = signatureProvider;
      Cache = new LruCache<string, Pair<string, string>>(CacheSize, p => p.First);
    }

    // Deserialization

    /// <inheritdoc/>
    public void OnDeserialization(object sender)
    {
      _lock = new object();
      Initialize();
    }
  }
}