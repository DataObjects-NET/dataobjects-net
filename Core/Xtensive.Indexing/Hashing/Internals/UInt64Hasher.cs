// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Indexing.Hashing
{
  [Serializable]
  internal class UInt64Hasher : HasherBase<ulong>
  {
    /// <inheritdoc/>
    public override long GetHash(ulong value)
    {
      return unchecked ((long) value);
    }

    /// <inheritdoc/>
    public override long[] GetHashes(ulong value, int count)
    {
      return HashingUtils.GetHashes(UInt64ToByteArray(value), count, GetHash(value));
    }

    private static byte[] UInt64ToByteArray(ulong value)
    {
      var data = new byte[sizeof(ulong)];
      for (int i = 0; i < sizeof(ulong); i++)
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