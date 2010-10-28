// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.29

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Tests.ObjectModel.GraphModel
{
  [HierarchyRoot]
  public class Trunk : Entity
  {
    [Field,Key]
    public int Id { get; private set; }
    [Field]
    public Trunk Left { get; set; }
    [Field, Association(PairTo = "Right", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Clear)]
    public Trunk Right { get; set; }
    [Field, Association(PairTo = "Trunk")]
    public EntitySet<Branch> Branches { get; private set; }
  }

  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class Branch : Entity
  {
    [Field, Key(1)]
    public int Id { get; private set; }
    [Field, Key(0)]
    public Trunk Trunk { get; private set; }
    [Field, Association(PairTo = "Branch")]
    public EntitySet<Leaf> Leaves { get; private set; }

    public Branch(Trunk trunk, int id)
      : base(trunk, id)
    {}
  }

  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class Leaf : Entity
  {
    [Field, Key(2)]
    public int Id { get; private set; }
    [Field, Key(0)]
    public Trunk Trunk { get; private set; }
    [Field, Key(1)]
    public Branch Branch { get; private set; }

    public Leaf(Trunk trunk, Branch branch, int id)
      : base(trunk, branch, id)
    {}
  }

}