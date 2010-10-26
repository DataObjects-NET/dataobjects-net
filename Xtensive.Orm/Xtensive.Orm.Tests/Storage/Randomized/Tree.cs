// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.11.26

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Storage.Randomized
{
  [Serializable]
  [HierarchyRoot]
  public sealed class Tree : Entity,
    IEnumerable<TreeNode>
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int NodeCount { get; set; }

    [Field]
    public int Depth { get; set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade)]
    public TreeNode Root { get; set; }

    /// <inheritdoc/>
    public IEnumerator<TreeNode> GetEnumerator()
    {
      return EnumerableUtils.One(Root).Flatten(node => node.Children, null, true).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}