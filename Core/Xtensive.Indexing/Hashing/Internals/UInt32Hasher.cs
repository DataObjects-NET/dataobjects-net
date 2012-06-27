// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Indexing.Hashing
{
  [Serializable]
  internal class UInt32Hasher : HasherBase<uint>
  {
    public override long GetHash(uint value)
    {
      return value;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(uint value, int count)
    {
      return HashingUtils.GetHashes(UInt32ToByteArray(value), count, GetHash(value));
    }

    private static byte[] UInt32ToByteArray(uint value)
    {
      var data = new byte[sizeof(uint)];
      for (int i = 0; i < sizeof(uint); i++)
        data[i] = ((byte)(value >> (i << 3)));
      return data;
    }


    // Constructors

    public UInt32Hasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}