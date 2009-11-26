// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.11.26

namespace Xtensive.Storage.Tests.Storage.Randomized
{
  [HierarchyRoot]
  public sealed class Tree : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int NodeCount { get; set; }

    [Field]
    public int Depth { get; set; }

    [Field]
    public TreeNode Root { get; set; }
  }
}