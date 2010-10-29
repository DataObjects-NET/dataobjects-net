// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

namespace Xtensive.Hashing
{
  internal class BaseHasherWrapper<T, TBase> : WrappingHasher<T, TBase>
    where T : TBase
  {
    /// <inheritdoc/>
    public override long GetHash(T value)
    {
      if (BaseHasher.Hasher != null)
        return BaseHasher.GetHash(value);
      return Provider.GetHasherByInstance(value).GetInstanceHash(value);
    }

    /// <inheritdoc/>
    public override long[] GetHashes(T value, int count)
    {
      return BaseHasher.GetHashes(value, count);
    }


    // Constructors

    public BaseHasherWrapper(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}