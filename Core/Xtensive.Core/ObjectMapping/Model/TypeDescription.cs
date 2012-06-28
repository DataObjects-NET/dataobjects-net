// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.ObjectMapping.Model
{
  /// <summary>
  /// Description of mapped type.
  /// </summary>
  [Serializable]
  public abstract class TypeDescription : LockableBase
  {
    private readonly Dictionary<PropertyInfo, PropertyDescription> properties =
      new Dictionary<PropertyInfo, PropertyDescription>();

    /// <summary>
    /// Delegate that can be used to extract a key of an object.
    /// </summary>
    public readonly Func<object, object> KeyExtractor;

    /// <summary>
    /// Collection of properties contained in the type.
    /// </summary>
    public readonly ReadOnlyDictionary<PropertyInfo, PropertyDescription> Properties;

    /// <summary>
    /// Underlying system type.
    /// </summary>
    public readonly Type SystemType;

    /// <summary>
    /// Kind of an object whose type described by this instance.
    /// </summary>
    public readonly ObjectKind ObjectKind;

    /// <summary>
    /// Gets the property.
    /// </summary>
    /// <param name="systemProperty">The system property.</param>
    /// <returns>The <see cref="PropertyDescription"/> for the specified <see cref="PropertyInfo"/>.</returns>
    public PropertyDescription GetProperty(PropertyInfo systemProperty)
    {
      return properties[systemProperty];
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      foreach (var propertyDescription in Properties.Values)
        propertyDescription.Lock(true);
      base.Lock(recursive);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return SystemType.ToString();
    }

    /// <summary>
    /// Adds the property.
    /// </summary>
    /// <param name="property">The property.</param>
    protected void AddProperty(PropertyDescription property)
    {
      this.EnsureNotLocked();
      if (properties.ContainsKey(property.SystemProperty))
        throw new InvalidOperationException(
          String.Format(Strings.ExMappingForPropertyXHasAlreadyBeenRegistered, property));
      properties.Add(property.SystemProperty, property);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="systemType">The system type.</param>
    /// <param name="keyExtractor">The key extractor.</param>
    protected TypeDescription(Type systemType, Func<object, object> keyExtractor)
      : this(systemType)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");

      KeyExtractor = keyExtractor;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="systemType">The system type.</param>
    protected TypeDescription(Type systemType)
    {
      ArgumentValidator.EnsureArgumentNotNull(systemType, "systemType");

      SystemType = systemType;
      Properties = new ReadOnlyDictionary<PropertyInfo, PropertyDescription>(properties, false);
      if (MappingHelper.IsTypePrimitive(systemType))
        ObjectKind = ObjectKind.Primitive;
      else if (systemType.IsValueType)
        ObjectKind = ObjectKind.UserStructure;
      else
        ObjectKind = ObjectKind.Entity;
    }
  }
}