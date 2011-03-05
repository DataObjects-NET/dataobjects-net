// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class Int64Hasher : HasherBase<long>
  {
    /// <inheritdoc/>
    public override long GetHash(long value)
    {
      return value;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(long value, int count)
    {
      return HashingUtils.GetHashes(Int64ToByteArray(value), count, value);
    }

    private static byte[] Int64ToByteArray(long value)
    {
      var data = new byte[sizeof(long)];
      for (int i = 0; i < sizeof(long); i++)
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