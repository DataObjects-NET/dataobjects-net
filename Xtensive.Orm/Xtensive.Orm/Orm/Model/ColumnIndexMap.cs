// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.04

using System;
using System.Collections.Generic;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A map of useful column indexes.
  /// </summary>
  [Serializable]
  public sealed class ColumnIndexMap
  {
    /// <summary>
    /// Gets or sets positions of system columns within <see cref="IndexInfo"/>.
    /// </summary>
    public IList<int> System { get; private set; }

    /// <summary>
    /// Gets or sets positions of lazy load columns within <see cref="IndexInfo"/>.
    /// </summary>
    public IList<int> LazyLoad { get; private set; }

    /// <summary>
    /// Gets or sets positions of regular columns (not system and not lazy load) within <see cref="IndexInfo"/>.
    /// </summary>
    public IList<int> Regular { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="system">The system columns.</param>
    /// <param name="lazyLoad">The regular columns.</param>
    /// <param name="regular">The lazy load columns.</param>
    public ColumnIndexMap(List<int> system, List<int> regular, List<int> lazyLoad)
    {
      System = system.AsReadOnly();
      LazyLoad = lazyLoad.AsReadOnly();
      Regular = regular.AsReadOnly();
    }
  }
}