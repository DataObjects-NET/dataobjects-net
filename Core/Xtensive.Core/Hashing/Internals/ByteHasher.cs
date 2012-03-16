// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class ByteHasher : HasherBase<Byte>
  {
    /// <inheritdoc/>
    public override long GetHash(Byte value)
    {
      return value;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(Byte value, int count)
    {
      return HashingUtils.GetHashes(new[] { value }, count, value);
    }


    // Constructors

    public ByteHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}