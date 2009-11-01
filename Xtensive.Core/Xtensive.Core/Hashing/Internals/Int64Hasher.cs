// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Core.Hashing
{
  [Serializable]
  internal class Int64Hasher : HasherBase<Int64>
  {
    /// <inheritdoc/>
    public override long GetHash(Int64 value)
    {
      return value;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(Int64 value, int count)
    {
      return HashingUtils.GetHashes(Int64ToByteArray(value), count, value);
    }

    private static byte[] Int64ToByteArray(Int64 value)
    {
      var data = new byte[sizeof(Int64)];
      for (int i = 0; i < sizeof(Int64); i++)
        data[i] = ((byte)(value >> (i << 3)));
      return data;
    }


    // Constructors

    public Int64Hasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}