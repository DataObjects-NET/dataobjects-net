// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Indexing.Hashing
{
  [Serializable]
  internal class ValueTypeHasher<T> : WrappingHasher<T, int>
    where T : struct
  {
    /// <inheritdoc/>
    public override long GetHash(T value)
    {
      return BaseHasher.GetHash(value.GetHashCode());
    }

    /// <inheritdoc/>
    public override long[] GetHashes(T value, int count)
    {
      return BaseHasher.GetHashes(value.GetHashCode(), count);
    }


    // Constructors

    public ValueTypeHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}