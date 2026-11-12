// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.26

using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  public sealed class PropertyInfo
  {
    public TypeInfo DeclaringType { get; private set; }

    public PropertyDefinition Definition { get; private set; }

    #region Definition projections

    public string Name => Definition.Name;

    public bool IsInstance => Definition.HasThis;

    public bool HasDefaultValue => Definition.HasDefault;

    public bool IsReadOnly => Definition.SetMethod is null;

    public MethodDefinition AnyAccessor => Definition.GetMethod ?? Definition.SetMethod;
    #endregion

    public bool IsAutomatic { get; set; }

    public bool IsNew { get; set; }

    public bool IsOverride { get; set; }

    public bool IsExplicitInterfaceImplementation { get; set; }

    public bool IsPersistent { get; set; }

    public bool IsKey { get; set; }

    public string PersistentName { get; set; }

    public PropertyInfo BaseProperty { get; set; }

    public IList<PropertyInfo> ImplementedProperties { get; set; }

    

    public override string ToString()
    {
      var resultBuilder = new StringBuilder()
        .Append(Name)
        .AppendFormat($" [type({Definition.PropertyType.FullName})]");
      if (IsInstance)
        _ = resultBuilder.Append(" [instance]");
      if (IsAutomatic)
        _ = resultBuilder.Append(" [automatic]");
      if (IsNew)
        _ = resultBuilder.Append(" [new]");
      if (IsOverride)
        _ = resultBuilder.Append(" [override]");
      if (IsExplicitInterfaceImplementation)
        _ = resultBuilder.Append(" [explicit_implementation]");
      if (IsKey)
        _ = resultBuilder.Append(" [key]");
      if (IsPersistent) {
        if (PersistentName is not null)
          _ = resultBuilder.AppendFormat($" [persistent({PersistentName})]");
        else
          _ = resultBuilder.Append(" [persistent]");
      }
      foreach (var implementedProperty in ImplementedProperties)
        _ = resultBuilder.AppendFormat($" [implements({implementedProperty.DeclaringType.FullName}::{implementedProperty.Name})]");
      return resultBuilder.ToString();
    }

    public PropertyInfo FindBase(Func<PropertyInfo, bool> predicate)
    {
      var current = BaseProperty;
      while (current is not null) {
        if (predicate.Invoke(current))
          return current;
        current = current.BaseProperty;
      }
      return null;
    }

    public PropertyInfo(TypeInfo declaringType, PropertyDefinition definition)
    {
      DeclaringType = declaringType ?? throw new ArgumentNullException(nameof(declaringType));
      Definition = definition ?? throw new ArgumentNullException(nameof(definition));
      ImplementedProperties = new List<PropertyInfo>();
    }
  }
}