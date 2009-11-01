// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Core.Hashing
{
  [Serializable]
  internal class DateTimeHasher : WrappingHasher<DateTime, long>
  {
    /// <inheritdoc/>
    public override long GetHash(DateTime value)
    {
      return value.Ticks;
    }

    /// <inheritdoc/>
    public override long[] GetHashes(DateTime value, int count)
    {
      return BaseHasher.GetHashes(value.Ticks, count);
    }

    // Constructors

    public DateTimeHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}