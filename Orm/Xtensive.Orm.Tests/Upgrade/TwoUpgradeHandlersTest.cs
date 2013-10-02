// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.06

using NUnit.Framework;
using Xtensive.Orm.Tests.Upgrade.TwoUpgradeHandlersTestModel;
using Xtensive.Orm.Upgrade;
using Xtensive.Testing;

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
      configuration.Types.Register(typeof (UpgradeHandler1));
      configuration.Types.Register(typeof (UpgradeHandler2));
      AssertEx.Throws<DomainBuilderException>(() => Domain.Build(configuration));
    }
  }
}