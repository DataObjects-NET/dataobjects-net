// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  public struct TypeIdentity : IEquatable<TypeIdentity>
  {
    public readonly string AssemblyName { get; }

    public readonly string TypeName { get; }

    public override bool Equals(object obj)
    {
      if (obj is null)
        return false;
      return obj is TypeIdentity && Equals((TypeIdentity) obj);
    }

    public bool Equals(TypeIdentity other)
    {
      return WeavingHelper.AssemblyNameComparer.Equals(AssemblyName, other.AssemblyName)
        && WeavingHelper.TypeNameComparer.Equals(TypeName, other.TypeName);
    }

    public override int GetHashCode()
    {
      unchecked {
        var typeNameHash = TypeName is not null ? WeavingHelper.TypeNameComparer.GetHashCode(TypeName) : 0;
        var assemblyNameHash = AssemblyName is not null ? WeavingHelper.AssemblyNameComparer.GetHashCode(AssemblyName) : 0;
        return assemblyNameHash * 397 ^ typeNameHash;
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

    public TypeIdentity(TypeReference type)
    {
      ArgumentNullException.ThrowIfNull(type);

      AssemblyName = type.Scope.Name;
      TypeName = type.FullName;
    }
  }
}