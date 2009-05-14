// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.12.28

using System;

namespace Xtensive.Storage
{
  /// <summary>
  /// Indicates that materialized view should be created for
  /// the interface type it is applied on.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
  public class MaterializedViewAttribute : MappingAttribute
  {
    // Constructors

    /// <inheritdoc/>
    public MaterializedViewAttribute()
    {
    }

    /// <inheritdoc/>
    public MaterializedViewAttribute(string mappingName)
      : base(mappingName)
    {
    }
  }
}