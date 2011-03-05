// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.11.26

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Storage.Randomized
{
  [Serializable]
  [HierarchyRoot]
  public sealed class TreeNode : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    [Association(OnTargetRemove = OnRemoveAction.Cascade)]
    public Tree Tree { get; set; }

    [Field]
    public TreeNode Parent { get; set; }

    [Field]
    [Association(PairTo = "Parent", OnOwnerRemove = OnRemoveAction.Cascade,
      OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<TreeNode> Children { get; private set; }
  }
}