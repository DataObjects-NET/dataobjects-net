// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.29

using System;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.ObjectMapping
{
  /// <summary>
  /// Settings for O2O-mapper.
  /// </summary>
  [Serializable]
  public sealed class MapperSettings : LockableBase
  {
    private GraphTruncationType graphTruncationType;
    private int? graphDepthLimit;
    private bool enableDynamicSourceHierarchies;

    /// <summary>
    /// Gets or sets the action that is taken to truncate a graph.
    /// </summary>
    public GraphTruncationType GraphTruncationType {
      get { return graphTruncationType; }
      set {
        this.EnsureNotLocked();
        graphTruncationType = value;
      }
    }

    /// <summary>
    /// Gets the limit of a graph depth.
    /// </summary>
    public int? GraphDepthLimit
    {
      get { return graphDepthLimit; }
      set {
        this.EnsureNotLocked();
        if (value != null)
          ArgumentValidator.EnsureArgumentIsGreaterThan((int) value, -1, "value");
        graphDepthLimit = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether hierarchies of source types
    /// can be expanded by the mapper. If this option is enabled the mapper
    /// can transform instance of class which hasn't been registered in the mapping,
    /// but is descendant of a class that has been registered.
    /// </summary>
    public bool EnableDynamicSourceHierarchies
    {
      get { return enableDynamicSourceHierarchies; }
      set {
        this.EnsureNotLocked();
        enableDynamicSourceHierarchies = value;
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public MapperSettings()
    {
      graphTruncationType = GraphTruncationType.Default;
    }
  }
}