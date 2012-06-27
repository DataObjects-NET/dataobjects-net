// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;
using Xtensive.Core;

namespace Xtensive.Indexing.Hashing
{
  [Serializable]
  internal class PairHasher<TFirst, TSecond> : WrappingHasher<Pair<TFirst, TSecond>, TFirst, TSecond>
  {
    /// <inheritdoc/>
    public override long GetHash(Pair<TFirst, TSecond> value)
    {
      return BaseHasher1.GetHash(value.First) ^ BaseHasher2.GetHash(value.Second);
    }

    /// <inheritdoc/>
    public override long[] GetHashes(Pair<TFirst, TSecond> value, int count)
    {
      long[] firstHashes = BaseHasher1.GetHashes(value.First, count);
      long[] secondHashes = BaseHasher2.GetHashes(value.Second, count);
      for (int i = 0; i < count; i++) {
        firstHashes[i] ^= secondHashes[i];
      }
      return firstHashes;
    }


    // Constructors

    public PairHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}