// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping.Model
{
  [Serializable]
  public abstract class TypeDescription : LockableBase
  {
    private readonly Dictionary<PropertyInfo, PropertyDescription> properties =
      new Dictionary<PropertyInfo, PropertyDescription>();

    public readonly Func<object, object> KeyExtractor;

    public readonly ReadOnlyDictionary<PropertyInfo, PropertyDescription> Properties;

    public readonly Type SystemType;

    public PropertyDescription GetProperty(PropertyInfo systemProperty)
    {
      return properties[systemProperty];
    }

    public override void Lock(bool recursive)
    {
      foreach (var propertyDescription in Properties.Values)
        propertyDescription.Lock(true);
      base.Lock(recursive);
    }

    protected void AddProperty(PropertyDescription property)
    {
      this.EnsureNotLocked();
      if (properties.ContainsKey(property.SystemProperty))
        throw new InvalidOperationException(
          String.Format(Strings.ExMappingForPropertyXHasAlreadyBeenRegistered, property.SystemProperty));
      properties.Add(property.SystemProperty, property);
    }


    // Constructors

    protected TypeDescription(Type systemType, Func<object, object> keyExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNull(systemType, "systemType");
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");

      SystemType = systemType;
      KeyExtractor = keyExtractor;
      Properties = new ReadOnlyDictionary<PropertyInfo, PropertyDescription>(properties, false);
    }
  }
}