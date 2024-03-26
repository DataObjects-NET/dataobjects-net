// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Upgrade.EntitySetUpgrade
{
  [TestFixture]
  public class EntitySetUpgradeTestBase
  {
    protected static DomainConfiguration CreateInitConfiguration(Type[] types) =>
      CreateConfiguration(types, DomainUpgradeMode.Recreate);

    protected static DomainConfiguration CreateSafeUpgradeConfiguration(Type[] types) =>
      CreateConfiguration(types, DomainUpgradeMode.PerformSafely);

    protected static DomainConfiguration CreateUnsafeUpgradeConfiguration(Type[] types) =>
      CreateConfiguration(types, DomainUpgradeMode.Perform);

    private static DomainConfiguration CreateConfiguration(Type[] types, in DomainUpgradeMode upgradeMode)
    {
      var config = DomainConfigurationFactory.Create();
      foreach (var type in types) {
        config.Types.Register(type);
      }
      config.UpgradeMode = upgradeMode;

      return config;
    }
  }
}