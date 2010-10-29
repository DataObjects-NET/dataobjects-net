// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.ObjectMapping.Model
{
  /// <summary>
  /// Descriptor of a property of a target mapped type.
  /// </summary>
  [Serializable]
  public sealed class TargetPropertyDescription : PropertyDescription
  {
    private SourcePropertyDescription sourceProperty;
    private Func<object, SourcePropertyDescription, object> converter;
    private bool isChangeTrackingDisabled;
    private bool isIgnored;
    private TargetTypeDescription valueType;    

    /// <inheritdoc/>
    public new TargetTypeDescription ReflectedType {
      get { return (TargetTypeDescription) base.ReflectedType; }
    }

    /// <summary>
    /// Gets the type of the value.
    /// </summary>
    public TargetTypeDescription ValueType {
      get { return valueType; }
      internal set{
        this.EnsureNotLocked();
        valueType = value;
      }
    }

    /// <summary>
    /// Gets the source property bound to this instance.
    /// </summary>
    public SourcePropertyDescription SourceProperty
    {
      get { return sourceProperty; }
      internal set{
        this.EnsureNotLocked();
        sourceProperty = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether modifications of this property are ignored.
    /// </summary>
    public bool IsIgnored {
      get { return isIgnored; }
      internal set {
        this.EnsureNotLocked();
        isIgnored = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether change tracking for this property is disabled.
    /// </summary>
    public bool IsChangeTrackingDisabled {
      get { return isChangeTrackingDisabled; }
      internal set {
        this.EnsureNotLocked();
        isChangeTrackingDisabled = value;
      }
    }

    internal Func<object, SourcePropertyDescription, object> Converter
    {
      get { return converter; }
      set{
        this.EnsureNotLocked();
        converter = value;
      }
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="systemProperty">The system property.</param>
    /// <param name="reflectedType">The the type that was used to obtain this description.</param>
    public TargetPropertyDescription(PropertyInfo systemProperty, TargetTypeDescription reflectedType)
      : base(systemProperty, reflectedType)
    {}
  }
}