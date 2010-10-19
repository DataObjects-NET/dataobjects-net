// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class SByteHasher : HasherBase<SByte>
  {    
    /// <inheritdoc/>
    public override long GetHash(SByte value)
    {
      return value;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(SByte value, int count)
    {
      return HashingUtils.GetHashes(new[] {(byte)value}, count, value);
    }


    // Constructors

    public SByteHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}