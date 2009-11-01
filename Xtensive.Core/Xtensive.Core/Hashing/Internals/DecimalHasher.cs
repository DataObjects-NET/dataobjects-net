// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.01.16

using System;

namespace Xtensive.Core.Hashing
{
  [Serializable]
  internal class DecimalHasher : WrappingHasher<decimal, ulong>
  {
    /// <inheritdoc/>
    public override long GetHash(Decimal value)
    {
      return BaseHasher.GetHash(GetLongHash(value));
    }

    /// <inheritdoc/>
    public override long[] GetHashes(Decimal value, int count)
    {
      return BaseHasher.GetHashes(GetLongHash(value), count);
    }

    private static ulong GetLongHash(Decimal value)
    {
      int[] intArray = decimal.GetBits(value);
      ulong value1 = (((ulong) intArray[0]) << 32) | (ulong) intArray[1];
      ulong value2 = (((ulong) intArray[2]) << 32) | (ulong) intArray[3];
      return value1 ^ value2;
    }


    // Constructors

    public DecimalHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}