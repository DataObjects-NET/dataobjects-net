// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2008.01.23

using Xtensive.Indexing.Hashing;

namespace Xtensive.Indexing
{
  internal class EntireHasher<T> : WrappingHasher<Entire<T>, T>
  {
    /// <inheritdoc/>
    public override long GetHash(Entire<T> value)
    {
      return BaseHasher.GetHash(value.Value);
    }

    /// <inheritdoc/>
    public override long[] GetHashes(Entire<T> value, int count)
    {
      return BaseHasher.GetHashes(value.Value, count);
    }
   
    // Constructors

    public EntireHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}