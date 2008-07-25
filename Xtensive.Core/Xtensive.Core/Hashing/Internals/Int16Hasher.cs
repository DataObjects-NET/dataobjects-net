// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Core.Hashing
{
  [Serializable]
  internal class Int16Hasher : HasherBase<Int16>
  {
    /// <inheritdoc/>
    public override long GetHash(Int16 value)
    {
      return value;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(Int16 value, int count)
    {
      return HashingUtils.GetHashes(Int16ToByteArray(value), count, GetHash(value));
    }

    private static byte[] Int16ToByteArray(Int16 value)
    {
      var data = new byte[sizeof(Int16)];
      for (int i = 0; i < sizeof(Int16); i++)
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