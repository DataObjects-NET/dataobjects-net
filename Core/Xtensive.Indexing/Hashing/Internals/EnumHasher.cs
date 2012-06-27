// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.13

using System;

namespace Xtensive.Indexing.Hashing
{
  internal class EnumHasher<TEnum, TUnderlyingType> : WrappingHasher<TEnum, TUnderlyingType>
    where TEnum : struct
    where TUnderlyingType : struct
  {
    /// <inheritdoc/>
    public override long GetHash(TEnum value)
    {
      return BaseHasher.GetHash((TUnderlyingType)(ValueType)value);
    }

    /// <inheritdoc/>
    public override long[] GetHashes(TEnum value, int count)
    {
      return BaseHasher.GetHashes((TUnderlyingType)(ValueType)value, count);
    }


    // Constructors

    public EnumHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}