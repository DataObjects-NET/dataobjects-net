// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.01.14

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal interface IHasNestedNodes
  {
    ReadOnlyCollection<BaseFieldNode> NestedNodes { get; }

    IEnumerable<Key> ExtractKeys(object target);

    IHasNestedNodes ReplaceNestedNodes(ReadOnlyCollection<BaseFieldNode> nestedNodes);
  }
}