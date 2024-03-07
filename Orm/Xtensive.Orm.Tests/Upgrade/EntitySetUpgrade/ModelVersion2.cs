// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System.Collections;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade.EntitySetUpgradeTest.Model.Version2
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
  }
}