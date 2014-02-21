// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  public sealed class TypeInfo
  {
    public string Name { get; private set; }

    public string FullName { get; private set; }

    public PersistentTypeKind Kind { get; private set; }

    public TypeDefinition Definition { get; private set; }

    public TypeInfo BaseType { get; set; }

    public IList<TypeInfo> Interfaces { get; set; }

    public IDictionary<string, PropertyInfo> Properties { get; set; }

    public override string ToString()
    {
      var resultBuilder = new StringBuilder();
      resultBuilder.AppendFormat("{0}", FullName);
      resultBuilder.AppendFormat(" [kind({0})]", Kind);
      if (BaseType!=null)
        resultBuilder.AppendFormat(" [inherits({0})]", BaseType.FullName);
      foreach (var @interface in Interfaces)
        resultBuilder.AppendFormat(" [implements({0})]", @interface.FullName);
      return resultBuilder.ToString();
    }

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

    private void InitializeCollections()
    {
      Interfaces = new List<TypeInfo>();
      Properties = new Dictionary<string, PropertyInfo>(WeavingHelper.TypeNameComparer);
    }

    public TypeInfo(TypeReference reference, PersistentTypeKind kind)
    {
      if (reference==null)
        throw new ArgumentNullException("reference");

      Name = reference.Name;
      FullName = reference.FullName;
      Kind = kind;

      InitializeCollections();
    }

    public TypeInfo(TypeDefinition definition, PersistentTypeKind kind, TypeInfo baseType = null)
    {
      if (definition==null)
        throw new ArgumentNullException("definition");

      Name = definition.Name;
      FullName = definition.FullName;
      Kind = kind;

      Definition = definition;
      BaseType = baseType;

      InitializeCollections();
    }
  }
}