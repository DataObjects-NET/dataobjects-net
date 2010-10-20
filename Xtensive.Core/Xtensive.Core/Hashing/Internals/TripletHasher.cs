// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;
using Xtensive.Core;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class TripletHasher<TFirst, TSecond, TThird> : WrappingHasher<Triplet<TFirst, TSecond, TThird>, TFirst, TSecond, TThird>
  {
    /// <inheritdoc/>
    public override long GetHash(Triplet<TFirst, TSecond, TThird> value)
    {
      return BaseHasher1.GetHash(value.First) ^ BaseHasher2.GetHash(value.Second) ^ BaseHasher3.GetHash(value.Third);
    }

    /// <inheritdoc/>
    public override long[] GetHashes(Triplet<TFirst, TSecond, TThird> value, int count)
    {
      long[] firstHashes = BaseHasher1.GetHashes(value.First, count);
      long[] secondHashes = BaseHasher2.GetHashes(value.Second, count);
      long[] thirdHashes = BaseHasher3.GetHashes(value.Third, count);
      for (int i = 0; i < count; i++) {
        firstHashes[i] = firstHashes[i] ^ secondHashes[i] ^ thirdHashes[i];
      }
      return firstHashes;
    }


    // Constructors

    public TripletHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}