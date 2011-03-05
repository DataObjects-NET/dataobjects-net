// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.23

using System;
using Xtensive.IoC;
using Xtensive.Reflection;

namespace Xtensive.Hashing
{
  /// <summary>
  /// Base class for any wrapping <see cref="IHasher{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to calculate <see cref="long"/> hashes for.</typeparam>
  /// <typeparam name="TBase1">First base (wrapped) type.</typeparam>
  /// <typeparam name="TBase2">Second base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingHasher<T, TBase1, TBase2> : HasherBase<T>
  {
    /// <summary>
    /// Hasher for the first base (wrapped) type.
    /// </summary>
    protected HasherStruct<TBase1> BaseHasher1;

    /// <summary>
    /// Hasher for the second base (wrapped) type.
    /// </summary>
    protected HasherStruct<TBase2> BaseHasher2;


    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="WrappingHasher{T,TBase1,TBase2}"/>.
    /// </summary>
    /// <param name="provider">Hasher provider.</param>
    protected WrappingHasher(IHasherProvider provider)
      : base(provider)
    {
      BaseHasher1 = provider.GetHasher<TBase1>();
      if (!TypeHelper.IsFinal<TBase1>() && !(BaseHasher1.Hasher.Implementation is IFinalAssociate))
        BaseHasher1 = null;
      BaseHasher2 = provider.GetHasher<TBase2>();
      if (!TypeHelper.IsFinal<TBase2>() && !(BaseHasher2.Hasher.Implementation is IFinalAssociate))
        BaseHasher2 = null;
    }
  }
}