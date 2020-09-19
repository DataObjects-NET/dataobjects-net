// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;

namespace Xtensive.Orm.Tests.Upgrade.EntitySetUpgradeTest.Model.Version1
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    [Association(PairTo = "Person")]
    public EntitySet<Address> Addresses { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Address : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public Person Person { get; set;  }
  }
}