// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Implementation
{
  [Serializable]
  public abstract class PropertyHandler
  {
    /// <summary>
    /// The name of the property.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// Gets the type of the owner.
    /// </summary>
    public abstract Type OwnerType { get; }

    /// <summary>
    /// Gets the type of the value.
    /// </summary>
    public abstract Type ValueType { get; }
    
    /// <summary>
    /// Gets the untyped property getter.
    /// </summary>
    public abstract Func<object, object> UntypedGetter { get; }

    /// <summary>
    /// Gets the untyped property setter.
    /// </summary>
    public abstract Action<object, object> UntypedSetter { get; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The <see cref="Name"/> property value.</param>
    protected PropertyHandler(string name)
    {
      Name = name;
    }
  }
}