// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Core.Hashing
{
  [Serializable]
  internal class UInt64Hasher : HasherBase<UInt64>
  {
    /// <inheritdoc/>
    public override long GetHash(UInt64 value)
    {
      return unchecked ((long) value);
    }

    /// <inheritdoc/>
    public override long[] GetHashes(UInt64 value, int count)
    {
      return HashingUtils.GetHashes(UInt64ToByteArray(value), count, GetHash(value));
    }

    private static byte[] UInt64ToByteArray(UInt64 value)
    {
      var data = new byte[sizeof(UInt64)];
      for (int i = 0; i < sizeof(UInt64); i++)
        data[i] = ((byte)(value >> (i << 3)));
      return data;
    }


    // Constructors

    public UInt64Hasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}