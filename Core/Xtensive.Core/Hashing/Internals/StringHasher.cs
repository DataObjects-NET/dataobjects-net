// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;
using System.Text;

namespace Xtensive.Hashing
{
  [Serializable]
  internal class StringHasher : HasherBase<string>
  {
    /// <inheritdoc/>
    public override long GetHash(string value)
    {
      return HashingUtils.GetHash(Encoding.Unicode.GetBytes(value));
    }

    /// <inheritdoc/>
    public override long[] GetHashes(string value, int count)
    {
      return HashingUtils.GetHashes(Encoding.Unicode.GetBytes(value), count);      
    }


    // Constructors

    public StringHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}