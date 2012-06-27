// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.16

using System;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.Hashing
{
  [Serializable]
  internal class ArrayHasher<T> : WrappingHasher<T[], T>
  {
    /// <inheritdoc/>
    public override long GetHash(T[] value)
    {
      if (value==null)
        return 0;
      long result = 0;
      for (int i = 0; i < value.Length; i++) {
        result ^= BaseHasher.GetHash(value[i]);
      }
      return result;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(T[] value, int count)
    {
      if (count <= 0)
        throw new ArgumentOutOfRangeException("count", count,
          string.Format(Strings.ExArgumentShouldBeInRange, 0, int.MaxValue));
      if (value==null)
        return null;
      var result = new long[count];
      int lenght = value.Length;
      for (int i = 0; i < lenght; i++) {
        long[] hashes = BaseHasher.GetHashes(value[i], count);
        for (int j = 0; j < count; j++)
          result[j] ^= hashes[j];
      }
      return result;
    }


    // Constructors

    public ArrayHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}