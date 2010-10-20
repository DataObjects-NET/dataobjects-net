// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.24

using System;
using Xtensive.Core;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class HashableInterfaceHasher<T> : WrappingHasher<T, long>
    where T : IHashable
  {
    private readonly bool isClass = typeof (T).IsClass;

    /// <inheritdoc/>
    public override long GetHash(T value)
    {
      if (isClass)
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
      return BaseHasher.GetHash(value.Hash);
    }

    /// <inheritdoc/>
    public override long[] GetHashes(T value, int count)
    {
      if (isClass)
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
      return BaseHasher.GetHashes(value.Hash, count);
    }


    // Constructors

    public HashableInterfaceHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}