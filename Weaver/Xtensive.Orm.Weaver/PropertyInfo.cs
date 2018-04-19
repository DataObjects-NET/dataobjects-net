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
    private readonly string signatureName;

    /// <summary>
    /// The type where the property is declared
    /// </summary>
    public TypeInfo DeclaringType { get; private set; }

    /// <summary>
    /// Underlying property definition
    /// </summary>
    public PropertyDefinition Definition { get; private set; }

    /// <summary>
    /// Gets short property name. This name is not unique and should not be used as a key of property
    /// </summary>
    public string Name { get { return Definition.Name; } }

    /// <summary>
    /// Gets full name of the property. This name is completely unique even over hierarchy.
    /// </summary>
    public string FullName { get { return Definition.FullName; } }

    /// <summary>
    /// Gets string representation of property signature and includes property return type of the property, its name and the types of its parameters.
    /// </summary>
    public string PropertySignatureName { get { return signatureName; } }

    public bool IsInstance { get { return Definition.HasThis; } }

    public bool IsAutomatic { get; set; }

    public bool IsNew { get; set; }

    public bool IsOverride { get; set; }

    public bool IsExplicitInterfaceImplementation { get; set; }

    public bool IsPersistent { get; set; }

    public bool IsKey { get; set; }

    public string PersistentName { get; set; }

    public PropertyInfo BaseProperty { get; set; }

    public IList<PropertyInfo> ImplementedProperties { get; set; }

    public MethodDefinition AnyAccessor
    {
      get { return Definition.GetMethod ?? Definition.SetMethod; }
    }

    public override string ToString()
    {
      var resultBuilder = new StringBuilder();
      resultBuilder.Append(Name);
      resultBuilder.AppendFormat(" [type({0})]", Definition.PropertyType.FullName);
      if (IsInstance)
        resultBuilder.Append(" [instance]");
      if (IsAutomatic)
        resultBuilder.Append(" [automatic]");
      if (IsNew)
        resultBuilder.Append(" [new]");
      if (IsOverride)
        resultBuilder.Append(" [override]");
      if (IsExplicitInterfaceImplementation)
        resultBuilder.Append(" [explicit_implementation]");
      if (IsKey)
        resultBuilder.Append(" [key]");
      if (IsPersistent) {
        if (PersistentName!=null)
          resultBuilder.AppendFormat(" [persistent({0})]", PersistentName);
        else
          resultBuilder.Append(" [persistent]");
      }
      foreach (var implementedProperty in ImplementedProperties)
        resultBuilder.AppendFormat(" [implements({0}::{1})]", implementedProperty.DeclaringType.FullName, implementedProperty.Name);
      return resultBuilder.ToString();
    }

    public PropertyInfo FindBase(Func<PropertyInfo, bool> predicate)
    {
      var current = BaseProperty;
      while (current!=null) {
        if (predicate.Invoke(current))
          return current;
        current = current.BaseProperty;
      }
      return null;
    }

    public PropertyInfo(TypeInfo declaringType, PropertyDefinition definition)
    {
      if (declaringType==null)
        throw new ArgumentNullException("declaringType");
      if (definition==null)
        throw new ArgumentNullException("definition");

      DeclaringType = declaringType;
      Definition = definition;
      ImplementedProperties = new List<PropertyInfo>();
      signatureName = WeavingHelper.GetPropertySignatureName(definition);
    }
  }
}