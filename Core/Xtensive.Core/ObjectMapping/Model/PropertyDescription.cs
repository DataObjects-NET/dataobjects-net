// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.ObjectMapping.Model
{
  /// <summary>
  /// Description of a property of a mapped class.
  /// </summary>
  [Serializable]
  public abstract class PropertyDescription : LockableBase
  {
    private bool isCollection;
    private PropertyInfo countProperty;
    private MethodInfo addMethod;
    private readonly string stringDescription;

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
      return stringDescription;
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
      stringDescription = String.Format(Strings.XInY, systemProperty, reflectedType);
    }
  }
}