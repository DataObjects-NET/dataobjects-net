// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.30

using System;
using System.Diagnostics;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Tests.Upgrade.DataUpgrade.Model.Version2
{
  [Serializable]
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
  public class A : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }
  }

  public class C : A
  {
    [Field]
    public A RefA { get; set; }
  }

  public class D : A
  {
    [Field]
    public EntitySet<A> RefA { get; private set; }
  }
}