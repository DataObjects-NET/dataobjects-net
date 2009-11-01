// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.16

using System;

namespace Xtensive.Core.Hashing
{
  [Serializable]
  internal class SingleHasher : WrappingHasher<float, ulong>
  {
    /// <inheritdoc/>
    public override long GetHash(Single value)
    {
      return BaseHasher.GetHash(GetULongHash(value));
    }

    /// <inheritdoc/>
    public override long[] GetHashes(Single value, int count)
    {
      return BaseHasher.GetHashes(GetULongHash(value), count);
    }

    private static ulong GetULongHash(Single value)
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