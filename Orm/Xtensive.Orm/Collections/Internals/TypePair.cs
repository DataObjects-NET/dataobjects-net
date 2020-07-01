// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.11

using System;

namespace Xtensive.Collections
{
  internal readonly struct TypePair : IEquatable<TypePair>
  {
    private readonly Type first;
    private readonly Type second;

    #region Equality members

    public bool Equals(TypePair other) =>
      ReferenceEquals(first, other.first) && ReferenceEquals(second, other.second);

    public override bool Equals(object obj) => obj is TypePair other && Equals(other);

    public override int GetHashCode()
    {
      unchecked {
        return ((first != null ? first.GetHashCode() : 0) * 397) ^ (second != null ? second.GetHashCode() : 0);
      }
    }

    public static bool operator ==(TypePair left, TypePair right) => left.Equals(right);

    public static bool operator !=(TypePair left, TypePair right) => !left.Equals(right);

    #endregion


    // Constructors

    public TypePair(Type first, Type second)
    {
      this.first = first;
      this.second = second;
    }
  }
}