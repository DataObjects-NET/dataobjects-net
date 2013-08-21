// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  internal struct TypeIdentity : IEquatable<TypeIdentity>
  {
    public static readonly StringComparer TypeNameComparer = StringComparer.InvariantCulture;

    private readonly IMetadataScope scope;
    private readonly string fullName;

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      return obj is TypeIdentity && Equals((TypeIdentity) obj);
    }

    public override int GetHashCode()
    {
      unchecked {
        return ((fullName!=null ? fullName.GetHashCode() : 0) * 397) ^ (scope!=null ? scope.GetHashCode() : 0);
      }
    }

    public static bool operator ==(TypeIdentity left, TypeIdentity right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(TypeIdentity left, TypeIdentity right)
    {
      return !left.Equals(right);
    }

    public bool Equals(TypeIdentity other)
    {
      return scope==other.scope && TypeNameComparer.Equals(fullName, other.fullName);
    }

    public TypeIdentity(TypeDefinition type)
    {
      if (type==null)
        throw new ArgumentNullException("type");

      scope = type.Module;
      fullName = type.FullName;
    }

    public TypeIdentity(TypeReference type)
    {
      if (type==null)
        throw new ArgumentNullException("type");

      scope = type.Scope;
      fullName = type.FullName;
    }
  }
}