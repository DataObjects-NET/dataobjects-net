// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.10

using System;
using System.Collections.Generic;

namespace Xtensive.Core.Sorting
{
  [Serializable]
  public class TopologicalSortResult<TNodeItem>
  {
    public List<TNodeItem> Result{ get; private set;}

    public TopologicalSortResult(List<TNodeItem> result)
    {
      Result = result;
    }
  }
}