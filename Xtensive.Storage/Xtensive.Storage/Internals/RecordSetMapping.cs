// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.31

using System.Collections.Generic;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Internals
{
  internal class RecordSetMapping
  {
    public IList<HierarchyMapping> HierarchyMappings { get; private set; }


    // Constructors

    public RecordSetMapping(IList<HierarchyMapping> hierarchyMappings)
    {
      HierarchyMappings = new ReadOnlyList<HierarchyMapping>(hierarchyMappings);
    }
  }
}