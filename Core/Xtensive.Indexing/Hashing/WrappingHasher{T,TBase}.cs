// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.21

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Reflection;

namespace Xtensive.Indexing.Hashing
{
  /// <summary>
  /// Base class for any wrapping <see cref="IHasher{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to calculate <see cref="long"/> hashes for.</typeparam>
  /// <typeparam name="TBase">Base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingHasher<T, TBase>: HasherBase<T>
  {
    /// <summary>
    /// Hasher for base (wrapped) type.
    /// </summary>
    protected HasherStruct<TBase> BaseHasher;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Hashing provider this hasher is bound to.</param>
    protected WrappingHasher(IHasherProvider provider)
      : base(provider)
    {
      BaseHasher = provider.GetHasher<TBase>();
      if (!TypeHelper.IsFinal<TBase>() && !(BaseHasher.Hasher.Implementation is IFinalAssociate) && typeof(TBase) != typeof(object))
        BaseHasher = null;
    }
  }
}