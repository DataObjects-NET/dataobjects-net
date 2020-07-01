// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.11

using System;

namespace Xtensive.Collections
{
  internal readonly struct ObjectPair : IEquatable<ObjectPair>
  {
    private readonly object first;
    private readonly object second;

    #region Equality members

    public bool Equals(ObjectPair other) => Equals(first, other.first) && Equals(second, other.second);

    public override bool Equals(object obj) => obj is ObjectPair other && Equals(other);

    public override int GetHashCode()
    {
      unchecked {
        return ((first != null ? first.GetHashCode() : 0) * 397) ^ (second != null ? second.GetHashCode() : 0);
      }
    }

    public static bool operator ==(ObjectPair left, ObjectPair right) => left.Equals(right);

    public static bool operator !=(ObjectPair left, ObjectPair right) => !left.Equals(right);

    #endregion


    // Constructors

    public ObjectPair(object first, object second)
    {
      this.first = first;
      this.second = second;
    }
  }
}