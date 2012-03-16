// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.16

using System;
using System.Collections.Generic;
using Xtensive.Resources;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class EnumerableInterfaceHasher<TEnumerable, T> : WrappingHasher<TEnumerable, T>
    where TEnumerable : IEnumerable<T>
  {
    private readonly Hasher<object> objectHasher;
    private static readonly bool isClass = typeof (TEnumerable).IsClass;

    /// <inheritdoc/>
    public override long GetHash(TEnumerable value)
    {
      if (isClass && ReferenceEquals(value, null))
        return objectHasher.GetHash(null);
      long result = 0;
      foreach (T instance in value) {
        result ^= BaseHasher.GetHash(instance);
      }
      return result;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(TEnumerable value, int count)
    {
      if (count <= 0)
        throw new ArgumentOutOfRangeException("count", value,
          string.Format(Strings.ExArgumentShouldBeInRange, 0, int.MaxValue));

      if (isClass && ReferenceEquals(value, null))
        return objectHasher.GetHashes(null, count);

      var result = new long[count];
      foreach (T instance in value) {
        long[] hashes = BaseHasher.GetHashes(instance, count);
        for (int j = 0; j < count; j++)
          result[j] ^= hashes[j];
      }
      return result;
    }


    // Constructors

    public EnumerableInterfaceHasher(IHasherProvider provider)
      : base(provider)
    {
      objectHasher = provider.GetHasher<object>();
    }
  }
}