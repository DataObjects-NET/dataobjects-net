// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.ObjectMapping.Model
{
  /// <summary>
  /// Description of source mapped type.
  /// </summary>
  [Serializable]
  public sealed class SourceTypeDescription : TypeDescription
  {
    private TargetTypeDescription targetType;

    /// <summary>
    /// Gets the corresponding target type.
    /// </summary>
    public TargetTypeDescription TargetType
    {
      get { return targetType; }
      internal set{
        this.EnsureNotLocked();
        targetType = value;
      }
    }

    /// <inheritdoc/>
    public new SourcePropertyDescription GetProperty(PropertyInfo propertyInfo)
    {
      return (SourcePropertyDescription) base.GetProperty(propertyInfo);
    }

    /// <summary>
    /// Adds the property.
    /// </summary>
    /// <param name="property">The property.</param>
    public void AddProperty(SourcePropertyDescription property)
    {
      base.AddProperty(property);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="systemType">The system type.</param>
    /// <param name="keyExtractor">The key extractor.</param>
    public SourceTypeDescription(Type systemType, Func<object, object> keyExtractor)
      : base(systemType, keyExtractor)
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="systemType">The system type.</param>
    public SourceTypeDescription(Type systemType)
      : base(systemType)
    {}
  }
}