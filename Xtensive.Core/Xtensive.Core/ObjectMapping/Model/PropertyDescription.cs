// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.ObjectMapping.Model
{
  /// <summary>
  /// Description of a property of a mapped class.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{SystemProperty} in {ReflectedType.SystemType}")]
  public abstract class PropertyDescription : LockableBase
  {
    private bool isCollection;
    private PropertyInfo countProperty;
    private MethodInfo addMethod;

    /// <summary>
    /// Gets the type that was used to obtain this description.
    /// </summary>
    public TypeDescription ReflectedType { get; private set; }

    /// <summary>
    /// Gets the underlying system property.
    /// </summary>
    public readonly PropertyInfo SystemProperty;

    /// <summary>
    /// Gets a value indicating whether this instance is collection property.
    /// </summary>
    public bool IsCollection {
      get { return isCollection; }
      internal set{
        this.EnsureNotLocked();
        isCollection = value;
      }
    }

    /// <summary>
    /// Gets the descriptor of the collection's "Count" property.
    /// </summary>
    public PropertyInfo CountProperty {
      get { return countProperty; }
      internal set{
        this.EnsureNotLocked();
        countProperty = value;
      }
    }

    /// <summary>
    /// Gets the descriptor of the collection's "Add" method.
    /// </summary>
    public MethodInfo AddMethod {
      get { return addMethod; }
      internal set{
        this.EnsureNotLocked();
        addMethod = value;
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return SystemProperty.ToString();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="systemProperty">The system property.</param>
    /// <param name="reflectedType">The the type that was used to obtain this description.</param>
    protected PropertyDescription(PropertyInfo systemProperty, TypeDescription reflectedType)
    {
      ArgumentValidator.EnsureArgumentNotNull(systemProperty, "systemProperty");
      ArgumentValidator.EnsureArgumentNotNull(reflectedType, "reflectedType");

      SystemProperty = systemProperty;
      ReflectedType = reflectedType;
    }
  }
}