// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class UInt16Hasher : HasherBase<ushort>
  {
    /// <inheritdoc/>
    public override long GetHash(ushort value)
    {
      return value;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(ushort value, int count)
    {
      return HashingUtils.GetHashes(UInt16ToByteArray(value), count, GetHash(value));
    }

    private static byte[] UInt16ToByteArray(ushort value)
    {
      var data = new byte[sizeof(ushort)];
      for (int i = 0; i < sizeof(ushort); i++)
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