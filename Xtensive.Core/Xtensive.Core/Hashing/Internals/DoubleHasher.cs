// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.16

using System;

namespace Xtensive.Core.Hashing
{
  [Serializable]
  internal class DoubleHasher : WrappingHasher<double, ulong>
  {
    /// <inheritdoc/>
    public override long GetHash(Double value)
    {
      return BaseHasher.GetHash(GetLongHash(value));
    }

    /// <inheritdoc/>
    public override long[] GetHashes(Double value, int count)
    {
      return BaseHasher.GetHashes(GetLongHash(value), count);
    }

    private static ulong GetLongHash(Double value)
    {
      byte[] byteArray = BitConverter.GetBytes(value);
      ulong value1 = (((ulong) byteArray[0]) << 24) | (((ulong) byteArray[1]) << 16) | (((ulong) byteArray[2]) << 8) | byteArray[3];
      ulong value2 = (((ulong) byteArray[4]) << 24) | (((ulong) byteArray[5]) << 16) | (((ulong) byteArray[6]) << 8) | byteArray[7];
      return value1 ^ value2;
    }


    // Constructors

    public DoubleHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}