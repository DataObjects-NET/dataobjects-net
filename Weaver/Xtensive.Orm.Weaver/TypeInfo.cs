// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  internal sealed class TypeInfo
  {
    public PersistentTypeKind Kind { get; set; }

    public TypeDefinition Definition { get; set; }

    public TypeInfo BaseType { get; set; }

    public IDictionary<string, PropertyInfo> Properties { get; set; }

    public PropertyInfo FindProperty(string name)
    {
      var current = this;
      while (current!=null) {
        PropertyInfo result;
        if (current.Properties.TryGetValue(name, out result))
          return result;
        current = current.BaseType;
      }
      return null;
    }

    public TypeInfo(PersistentTypeKind kind, TypeDefinition definition = null, TypeInfo baseType = null)
    {
      Kind = kind;
      BaseType = baseType;
      Definition = definition;
      Properties = new Dictionary<string, PropertyInfo>(WeavingHelper.TypeNameComparer);
    }
  }
}