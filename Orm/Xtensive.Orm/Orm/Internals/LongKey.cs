// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.20

using System;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  [Serializable]
  internal sealed class LongKey : Key
  {
    /// <inheritdoc/>
    protected override Tuple GetValue()
    {
      return value;
    }

    /// <inheritdoc/>
    protected override int CalculateHashCode()
    {
      return Tuple.HashCodeMultiplier ^ value.GetHashCode() ^ TypeReference.Type.Key.EqualityIdentifier.GetHashCode();
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

    internal LongKey(string nodeId, TypeInfo type, TypeReferenceAccuracy accuracy, Tuple value)
      : base(nodeId, type, accuracy, value)
    {
    }
  }
}