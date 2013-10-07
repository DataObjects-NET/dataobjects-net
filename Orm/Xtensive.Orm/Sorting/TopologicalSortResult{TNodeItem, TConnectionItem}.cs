// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.10

using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Xtensive.Sorting
{
  /// <summary>
  /// Describes result of <see cref="TopologicalSorter"/> operations.
  /// </summary>
  /// <typeparam name="TNodeItem">The type of the node item.</typeparam>
  /// <typeparam name="TConnectionItem">The type of the connection item.</typeparam>
  [Serializable]
  public class TopologicalSortResult<TNodeItem, TConnectionItem> : TopologicalSortResult<TNodeItem>
  {
    /// <summary>
    /// Gets or sets the found loops.
    /// </summary>
    public List<Node<TNodeItem, TConnectionItem>> Loops { get; private set;}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="result">The sorting result.</param>
    /// <param name="loops">The found loops.</param>
    public TopologicalSortResult(List<TNodeItem> result, List<Node<TNodeItem, TConnectionItem>> loops)
      : base(result)
    {
      Loops = loops;
    }
  }
}