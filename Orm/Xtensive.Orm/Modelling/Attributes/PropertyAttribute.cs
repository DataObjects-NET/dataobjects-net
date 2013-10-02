// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;

namespace Xtensive.Modelling.Attributes
{
  /// <summary>
  /// Node property marker.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public sealed class PropertyAttribute : Attribute
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
    /// Gets or sets a value indicating whether property values are compared case insensitively.
    /// </summary>
    public bool CaseInsensitiveComparison { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether underlying property value must be re-created
    /// rather than created &amp; processed as usual.
    /// </summary>
    public bool IsImmutable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether underlying property must be 
    /// ignored during recreation of parent atomic property.
    /// </summary>
    public bool IsVolatile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether property owner should be recreated on property value change.
    /// </summary>
    public bool RecreateParent { get; set; }

    /// <summary>
    /// Gets or sets the dependency root type.
    /// </summary>
    public Type DependencyRootType { get; set; }
  }
}