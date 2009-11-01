// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Core.Hashing
{
  [Serializable]
  internal class Int32Hasher : HasherBase<Int32>
  {
    /// <inheritdoc/>
    public override long GetHash(Int32 value)
    {
      return value;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(Int32 value, int count)
    {
      return HashingUtils.GetHashes(Int32ToByteArray(value), count, value);
    }

    private static byte[] Int32ToByteArray(Int32 value)
    {
      var data = new byte[sizeof (Int32)];
      for (int i = 0; i < sizeof (Int32); i++)
        data[i] = ((byte) (value >> (i << 3)));
      return data;
    }

    // Constructors

    public Int32Hasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}