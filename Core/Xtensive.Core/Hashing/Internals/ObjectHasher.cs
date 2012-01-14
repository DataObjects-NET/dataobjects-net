// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class ObjectHasher : WrappingHasher<object, int>
  {
    /// <inheritdoc/>
    public override long GetHash(object value)
    {
      return BaseHasher.GetHash(value==null ? 0 : value.GetHashCode());
    }

    /// <inheritdoc/>
    public override long[] GetHashes(object value, int count)
    {
      return BaseHasher.GetHashes(value==null ? 0 : value.GetHashCode(), count);
    }


    // Constructors

    public ObjectHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}