// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.11.26

using Xtensive.Core;

namespace Xtensive.Storage.Tests.Storage.Randomized
{
  [HierarchyRoot]
  public sealed class TreeNode : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Tree Tree { get; private set; }

    [Field]
    public TreeNode Parent { get; set; }

    [Field]
    [Association(PairTo = "Parent", OnOwnerRemove = OnRemoveAction.Deny,
      OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<TreeNode> Children { get; private set; }


    // Constructors

    public TreeNode(Tree tree)
    {
      ArgumentValidator.EnsureArgumentNotNull(tree, "tree");
      Tree = tree;
    }
  }
}