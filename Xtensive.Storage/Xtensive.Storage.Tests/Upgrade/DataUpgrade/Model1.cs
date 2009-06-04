// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.30

using System;

namespace Xtensive.Storage.Tests.Upgrade.DataUpgrade.Model.Version1
{
  [Serializable]
  [HierarchyRoot]
  public class A : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }
  }

  public class B : A
  {
    
  }

  public class C : B
  {
    [Field]
    public A RefA { get; set; }

    [Field]
    public B RefB { get; set; }
  }

  public class D : A
  {
    [Field]
    public EntitySet<A> RefA { get; private set; }
  }
}