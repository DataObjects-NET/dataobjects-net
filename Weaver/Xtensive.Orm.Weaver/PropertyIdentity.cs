// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.01

using System;

namespace Xtensive.Orm.Weaver
{
  internal struct PropertyIdentity : IEquatable<PropertyIdentity>
  {
    private readonly TypeIdentity type;
    private readonly string name;

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      return obj is PropertyIdentity && Equals((PropertyIdentity) obj);
    }

    public bool Equals(PropertyIdentity other)
    {
      return WeavingHelper.TypeNameComparer.Equals(name, other.name) && type.Equals(other.type);
    }

    public override int GetHashCode()
    {
      unchecked {
        return ((name!=null ? WeavingHelper.TypeNameComparer.GetHashCode(name) : 0) * 397) ^ type.GetHashCode();
      }
    }

    public static bool operator ==(PropertyIdentity left, PropertyIdentity right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(PropertyIdentity left, PropertyIdentity right)
    {
      return !left.Equals(right);
    }

    public PropertyIdentity(TypeIdentity type, string name)
    {
      if (name==null)
        throw new ArgumentNullException("name");

      this.type = type;
      this.name = name;
    }
  }
}