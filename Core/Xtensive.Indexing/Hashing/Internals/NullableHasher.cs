// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;

namespace Xtensive.Indexing.Hashing
{
  [Serializable]
  internal class NullableHasher<T> : WrappingHasher<T?, T, object>
    where T : struct
  {
    /// <inheritdoc/>
    public override long GetHash(T? value)
    {
      if (!value.HasValue)        
        return BaseHasher2.GetHash(null);
      return BaseHasher1.GetHash(value.Value);
    }

    /// <inheritdoc/>
    public override long[] GetHashes(T? value, int count)
    {
      if (!value.HasValue)
        return BaseHasher2.GetHashes(value, count);
      return BaseHasher1.GetHashes(value.Value, count);
    }


    // Constructors

    public NullableHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}