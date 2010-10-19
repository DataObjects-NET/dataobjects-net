// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.16

using System;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class BooleanHasher : WrappingHasher<bool, int>
  {
    /// <inheritdoc/>
    public override long GetHash(Boolean value)
    {
      return BaseHasher.GetHash(value.GetHashCode());
    }

    /// <inheritdoc/>
    public override long[] GetHashes(Boolean value, int count)
    {
      return BaseHasher.GetHashes(value.GetHashCode(), count);
    }


    // Constructors

    public BooleanHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}