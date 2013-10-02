// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.11

using System;
using System.Diagnostics;

namespace Xtensive.Collections
{
  internal struct TypePair : IEquatable<TypePair>
  {
    public readonly Type First;
    public readonly Type Second;

    #region Equality members

    public bool Equals(TypePair other)
    {
      return other.First==First && other.Second==Second;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (obj.GetType()!=typeof (TypePair))
        return false;
      return Equals((TypePair) obj);
    }

    public override int GetHashCode()
    {
      unchecked {
        return ((First!=null ? First.GetHashCode() : 0) * 397) ^ (Second!=null ? Second.GetHashCode() : 0);
      }
    }

    public static bool operator ==(TypePair left, TypePair right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(TypePair left, TypePair right)
    {
      return !left.Equals(right);
    }

    #endregion


    // Constructors

    public TypePair(Type first, Type second)
    {
      First = first;
      Second = second;
    }
  }
}