// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.25

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Indicates that entity has custom type discriminator value and exposes it.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class TypeDiscriminatorValueAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the value of type discriminator.
    /// </summary>
    /// <value>Custom type discriminator value.</value>
    public object Value { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether underlying type is default type in given hierarchy.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if underlying type is default in given hierarchy; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>Only one type at a time can be default in single hierarchy.</remarks>
    public bool Default { get; set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="value">The value.</param>
    public TypeDiscriminatorValueAttribute(object value)
    {
      Value = value;
    }
  }
}