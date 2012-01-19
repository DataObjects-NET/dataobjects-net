// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Reflection;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.ObjectMapping.Model
{
  /// <summary>
  /// Descriptor of a property of a source mapped type.
  /// </summary>
  [Serializable]
  public sealed class SourcePropertyDescription : PropertyDescription
  {
    /// <inheritdoc/>
    public new SourceTypeDescription ReflectedType {
      get { return (SourceTypeDescription) base.ReflectedType; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="systemProperty">The system property.</param>
    /// <param name="reflectedType">The the type that was used to obtain this description.</param>
    public SourcePropertyDescription(PropertyInfo systemProperty, SourceTypeDescription reflectedType)
      : base(systemProperty, reflectedType)
    {}
  }
}