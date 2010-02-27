// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.20

using System;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  public sealed class LongKey : Key
  {
    /// <inheritdoc/>
    protected override Tuple GetValue()
    {
      return value;
    }

    /// <inheritdoc/>
    protected override int CalculateHashCode()
    {
      return Tuple.HashCodeMultiplier ^ value.GetHashCode() ^ TypeRef.Type.Key.GetHashCode();
    }

    /// <inheritdoc/>
    protected override bool ValueEquals(Key other)
    {
      var otherKey = other as LongKey;
      if (otherKey == null)
        return false;

      return value.Equals(otherKey.value);
    }


    // Constructors

    internal LongKey(TypeInfo type, TypeReferenceAccuracy accuracy, Tuple value)
      : base(type, accuracy, value)
    {
    }
  }
}