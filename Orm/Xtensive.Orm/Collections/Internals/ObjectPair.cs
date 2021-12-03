// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
