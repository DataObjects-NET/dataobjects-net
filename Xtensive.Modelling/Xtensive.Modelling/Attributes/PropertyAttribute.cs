// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Attributes
{
  /// <summary>
  /// Node property marker.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public class PropertyAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the comparison \ modification priority.
    /// The lower priority the less dependent property is.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether underlying property must be ignored in comparison.
    /// </summary>
    public bool IgnoreInComparison { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether underlying property value must be re-created
    /// rather than created &amp; processed as usual.
    /// </summary>
    public bool IsImmutable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether underlying property must be 
    /// ignored during recreation of parent immutable property.
    /// </summary>
    public bool IsMutable { get; set; }

    /// <summary>
    /// Gets or sets the dependency root type.
    /// </summary>
    public Type DependencyRootType { get; set; }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public PropertyAttribute()
    {
    }
  }
}