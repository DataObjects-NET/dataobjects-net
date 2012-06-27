// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Indexing.Hashing
{
  [Serializable]
  internal class GuidHasher : HasherBase<Guid>
  {
    /// <inheritdoc/>
    public override long GetHash(Guid value)
    {
      return HashingUtils.GetHash(value.ToByteArray());
    }

    /// <inheritdoc/>
    public override long[] GetHashes(Guid value, int count)
    {
      return HashingUtils.GetHashes(value.ToByteArray(), count);      
    }


    // Constructors

    public GuidHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}