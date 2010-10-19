// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.01.16

using System;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class DecimalHasher : WrappingHasher<decimal, ulong>
  {
    /// <inheritdoc/>
    public override long GetHash(decimal value)
    {
      return BaseHasher.GetHash(GetLongHash(value));
    }

    /// <inheritdoc/>
    public override long[] GetHashes(decimal value, int count)
    {
      return BaseHasher.GetHashes(GetLongHash(value), count);
    }

    private static ulong GetLongHash(decimal value)
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