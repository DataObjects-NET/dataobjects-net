// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Defines the root type of hierarchy of persistent types.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class HierarchyRootAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets the inheritance schema for this hierarchy.
    /// </summary>
    public InheritanceSchema InheritanceSchema { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether key should include TypeId field.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if TypeId field should be included into key; otherwise, <see langword="false"/>.
    /// </value>
    public bool IncludeTypeId { get; set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public HierarchyRootAttribute()
      : this(InheritanceSchema.Default)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="schema">The inheritance schema for the hierarchy.</param>
    public HierarchyRootAttribute(InheritanceSchema schema)
    {
      InheritanceSchema = schema;
    }
  }
}