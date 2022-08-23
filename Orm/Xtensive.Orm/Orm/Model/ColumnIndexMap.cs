// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.04

using System;
using System.Collections.Generic;
using Xtensive.Core;


namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A map of useful column indexes.
  /// </summary>
  [Serializable]
  public readonly struct ColumnIndexMap
  {
    /// <summary>
    /// Gets or sets positions of system columns within <see cref="IndexInfo"/>.
    /// </summary>
    public IReadOnlyList<int> System { get; }

    /// <summary>
    /// Gets or sets positions of lazy load columns within <see cref="IndexInfo"/>.
    /// </summary>
    public IReadOnlyList<int> LazyLoad { get; }

    /// <summary>
    /// Gets or sets positions of regular columns (not system and not lazy load) within <see cref="IndexInfo"/>.
    /// </summary>
    public IReadOnlyList<int> Regular { get; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="system">The system columns.</param>
    /// <param name="lazyLoad">The regular columns.</param>
    /// <param name="regular">The lazy load columns.</param>
    public ColumnIndexMap(IReadOnlyList<int> system, IReadOnlyList<int> regular, IReadOnlyList<int> lazyLoad)
    {
      System = system.AsSafeWrapper();
      LazyLoad = lazyLoad.AsSafeWrapper();
      Regular = regular.AsSafeWrapper();
    }
  }
}
