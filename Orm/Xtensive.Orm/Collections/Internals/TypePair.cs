// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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