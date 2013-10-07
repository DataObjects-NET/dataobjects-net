// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.10

using System;
using System.Collections.Generic;


namespace Xtensive.Sorting
{
  /// <summary>
  /// Base type for <see cref="TopologicalSortResult{NodeItem,TConnectionItem}"/>.
  /// </summary>
  /// <typeparam name="TNodeItem">The type of the node item.</typeparam>
  [Serializable]
  public class TopologicalSortResult<TNodeItem>
  {
    /// <summary>
    /// Gets or sets the sorting result.
    /// </summary>
    public List<TNodeItem> Result { get; private set;}

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="result">The sorting result.</param>
    public TopologicalSortResult(List<TNodeItem> result)
    {
      Result = result;
    }
  }
}