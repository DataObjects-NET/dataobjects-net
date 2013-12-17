// Copyright (C) 2003-2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.12.17

using System;
using Xtensive.Core;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  internal sealed class QueryParameterIdentity : IEquatable<QueryParameterIdentity>
  {
    public TypeMapping Mapping { get; private set; }

    public object ClosureObject { get; private set; }

    public string FieldName { get; private set; }

    public QueryParameterBindingType BindingType { get; private set; }

    public bool Equals(QueryParameterIdentity other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return string.Equals(FieldName, other.FieldName)
        && ClosureObject.Equals(other.ClosureObject)
        && BindingType==other.BindingType
        && Mapping.Equals(other.Mapping);
    }

    public override int GetHashCode()
    {
      unchecked {
        var hashCode = FieldName.GetHashCode();
        hashCode = (hashCode * 397) ^ ClosureObject.GetHashCode();
        hashCode = (hashCode * 397) ^ (int) BindingType;
        hashCode = (hashCode * 397) ^ Mapping.GetHashCode();
        return hashCode;
      }
    }

    public static bool operator ==(QueryParameterIdentity left, QueryParameterIdentity right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(QueryParameterIdentity left, QueryParameterIdentity right)
    {
      return !Equals(left, right);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return obj is QueryParameterIdentity && Equals((QueryParameterIdentity) obj);
    }

    // Constructors

    public QueryParameterIdentity(TypeMapping mapping, object closureObject, string fieldName, QueryParameterBindingType bindingType)
    {
      ArgumentValidator.EnsureArgumentNotNull(mapping, "mapping");
      ArgumentValidator.EnsureArgumentNotNull(closureObject, "closureObject");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(fieldName, "fieldName");

      Mapping = mapping;
      ClosureObject = closureObject;
      FieldName = fieldName;
      BindingType = bindingType;
    }
  }
}