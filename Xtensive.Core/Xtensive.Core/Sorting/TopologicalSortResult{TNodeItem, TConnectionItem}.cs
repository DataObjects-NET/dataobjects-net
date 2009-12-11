// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.10

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xtensive.Core.Sorting
{
  [Serializable]
  public class TopologicalSortResult<TNodeItem, TConnectionItem> : TopologicalSortResult<TNodeItem>
  {
    public List<Node<TNodeItem, TConnectionItem>> Loops { get; private set;}

    public TopologicalSortResult(List<TNodeItem> result, List<Node<TNodeItem, TConnectionItem>> loops)
      : base(result)
    {
      Loops = loops;
    }
  }
}