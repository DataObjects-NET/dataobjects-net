// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.11

using System;
using System.Diagnostics;

namespace Xtensive.Collections
{
  internal struct ObjectPair : IEquatable<ObjectPair>
  {
    public readonly object First;
    public readonly object Second;

    #region Equality members

    public bool Equals(ObjectPair other)
    {
      return Equals(First, other.First) && Equals(Second, other.Second);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (obj.GetType()!=typeof (ObjectPair))
        return false;
      return Equals((ObjectPair) obj);
    }

    public override int GetHashCode()
    {
      unchecked {
        return ((First!=null ? First.GetHashCode() : 0) * 397) ^ (Second!=null ? Second.GetHashCode() : 0);
      }
    }

    public static bool operator ==(ObjectPair left, ObjectPair right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(ObjectPair left, ObjectPair right)
    {
      return !left.Equals(right);
    }

    #endregion


    // Constructors

    public ObjectPair(object first, object second)
    {
      First = first;
      Second = second;
    }
  }
}