// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class Int32Hasher : HasherBase<int>
  {
    /// <inheritdoc/>
    public override long GetHash(int value)
    {
      return value;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(int value, int count)
    {
      return HashingUtils.GetHashes(Int32ToByteArray(value), count, value);
    }

    private static byte[] Int32ToByteArray(int value)
    {
      var data = new byte[sizeof (int)];
      for (int i = 0; i < sizeof (int); i++)
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