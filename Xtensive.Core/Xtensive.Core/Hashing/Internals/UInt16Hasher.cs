// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Core.Hashing
{
  [Serializable]
  internal class UInt16Hasher : HasherBase<UInt16>
  {
    /// <inheritdoc/>
    public override long GetHash(UInt16 value)
    {
      return value;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(UInt16 value, int count)
    {
      return HashingUtils.GetHashes(UInt16ToByteArray(value), count, GetHash(value));
    }

    private static byte[] UInt16ToByteArray(UInt16 value)
    {
      var data = new byte[sizeof(UInt16)];
      for (int i = 0; i < sizeof(UInt16); i++)
        data[i] = ((byte)(value >> (i << 3)));
      return data;
    }


    // Constructors

    public UInt16Hasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}