// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Core.Hashing
{
  [Serializable]
  internal class UInt32Hasher : HasherBase<UInt32>
  {
    public override long GetHash(UInt32 value)
    {
      return value;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(UInt32 value, int count)
    {
      return HashingUtils.GetHashes(UInt32ToByteArray(value), count, GetHash(value));
    }

    private static byte[] UInt32ToByteArray(UInt32 value)
    {
      var data = new byte[sizeof(UInt32)];
      for (int i = 0; i < sizeof(UInt32); i++)
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