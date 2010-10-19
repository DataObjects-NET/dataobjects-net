// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class Int16Hasher : HasherBase<short>
  {
    /// <inheritdoc/>
    public override long GetHash(short value)
    {
      return value;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(short value, int count)
    {
      return HashingUtils.GetHashes(Int16ToByteArray(value), count, GetHash(value));
    }

    private static byte[] Int16ToByteArray(short value)
    {
      var data = new byte[sizeof(short)];
      for (int i = 0; i < sizeof(short); i++)
        data[i] = ((byte)(value >> (i << 3)));
      return data;
    }


    // Constructors

    public Int16Hasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}