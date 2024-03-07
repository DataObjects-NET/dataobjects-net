// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.05.30


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