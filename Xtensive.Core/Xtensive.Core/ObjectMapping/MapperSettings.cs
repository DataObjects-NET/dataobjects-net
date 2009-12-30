// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.29

using System;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.ObjectMapping
{
  /// <summary>
  /// Settings for O2O-mapper.
  /// </summary>
  [Serializable]
  public sealed class MapperSettings : LockableBase
  {
    private GraphTruncationType graphTruncationType;
    private int? graphDepthLimit;

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