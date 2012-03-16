// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.30

using System;

namespace Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version2
{
  [Serializable]
  [HierarchyRoot]
  public class A : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [Serializable]
  public class C : A
  {
    [Field]
    public A RefA { get; set; }
  }

  [Serializable]
  public class D : A
  {
    [Field]
    public EntitySet<A> RefA { get; private set; }
  }
}