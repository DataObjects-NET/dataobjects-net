// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.06

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Hashing
{
  /// <summary>
  /// Base class for any <see cref="IHasher{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to calculate <see cref="long"/> hashes for.</typeparam>
  [Serializable]
  public abstract class HasherBase<T> : IHasher<T>,
    IDeserializationCallback
  {
    private IHasherProvider provider;

    /// <inheritdoc/>
    public IHasherProvider Provider
    {
      [DebuggerStepThrough]
      get { return provider; }
    }

    #region IHasherBase Members

    /// <inheritdoc/>
    public long GetInstanceHash(object value)
    {
      return GetHash((T)value);
    }

    #endregion

    /// <inheritdoc/>
    public abstract long GetHash(T value);

    /// <inheritdoc/>
    public abstract long[] GetHashes(T value, int count);

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Hasher provider this hasher is bound to.</param>
    protected HasherBase(IHasherProvider provider)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      this.provider = provider;
    }

    // IDeserializationCallback methods

    /// <see cref="SerializableDocTemplate.OnDeserialization" copy="true" />
    public virtual void OnDeserialization(object sender)
    {
      if (provider==null || provider.GetType()==typeof (HasherProvider))
        provider = HasherProvider.Default;
    }
  }
}
