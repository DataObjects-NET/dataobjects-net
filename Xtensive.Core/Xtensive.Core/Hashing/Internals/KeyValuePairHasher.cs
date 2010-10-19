// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;
using System.Collections.Generic;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class KeyValuePairHasher<TKey, TValue> : WrappingHasher<KeyValuePair<TKey, TValue>, TKey, TValue>
  {
    /// <inheritdoc/>
    public override long GetHash(KeyValuePair<TKey, TValue> value)
    {
      return BaseHasher1.GetHash(value.Key) ^ BaseHasher2.GetHash(value.Value);
    }

    /// <inheritdoc/>
    public override long[] GetHashes(KeyValuePair<TKey, TValue> value, int count)
    {
      long[] firstHashes = BaseHasher1.GetHashes(value.Key, count);
      long[] secondHashes = BaseHasher2.GetHashes(value.Value, count);
      for (int i = 0; i < firstHashes.Length; i++) {
        firstHashes[i] ^= secondHashes[i];
      }
      return firstHashes;
    }


    // Constructors

    public KeyValuePairHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}