// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.05.30

using System;

namespace Xtensive.Orm.Tests.Upgrade.DataUpgrade.Model.Version1
{
  [Serializable]
  [HierarchyRoot]
  public class A : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [Serializable]
  public class B : A
  {
  }

  [Serializable]
  public class C : B
  {
    [Field]
    public A RefA { get; set; }

    [Field]
    public B RefB { get; set; }
  }

  [Serializable]
  public class D : A
  {
    [Field]
    public EntitySet<A> RefA { get; private set; }
  }
}