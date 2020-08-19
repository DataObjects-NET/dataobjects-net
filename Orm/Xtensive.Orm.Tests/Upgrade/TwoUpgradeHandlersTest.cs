// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.12.06

using NUnit.Framework;
using Xtensive.Orm.Tests.Upgrade.TwoUpgradeHandlersTestModel;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace TwoUpgradeHandlersTestModel
  {
    public class UpgradeHandler1 : UpgradeHandler
    {
    }

    public class UpgradeHandler2 : UpgradeHandler
    {
    }
  }

  [TestFixture]
  public class TwoUpgradeHandlersTest
  {
    [Test]
    public void MainTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(UpgradeHandler1));
      configuration.Types.Register(typeof(UpgradeHandler2));
      _ = Assert.Throws<DomainBuilderException>(() => Domain.Build(configuration));
    }

    [Test]
    public void MainAsyncTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(UpgradeHandler1));
      configuration.Types.Register(typeof(UpgradeHandler2));
      _ = Assert.ThrowsAsync<DomainBuilderException>(async () => await Domain.BuildAsync(configuration));
    }
  }
}