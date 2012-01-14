// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.16

using System;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class SingleHasher : WrappingHasher<float, ulong>
  {
    /// <inheritdoc/>
    public override long GetHash(float value)
    {
      return BaseHasher.GetHash(GetULongHash(value));
    }

    /// <inheritdoc/>
    public override long[] GetHashes(float value, int count)
    {
      return BaseHasher.GetHashes(GetULongHash(value), count);
    }

    private static ulong GetULongHash(float value)
    {
      byte[] byteArray = BitConverter.GetBytes(value);
      return (((ulong) byteArray[0]) << 24) | (((ulong) byteArray[1]) << 16) | (((ulong) byteArray[2]) << 8) | byteArray[3];
    }


    // Constructors

    public SingleHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}