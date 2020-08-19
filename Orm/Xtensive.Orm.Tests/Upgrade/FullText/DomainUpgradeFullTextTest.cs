// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.21

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

using Xtensive.Core;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Upgrade.FullText
{
  [TestFixture, Category("Upgrade")]
  public class DomainUpgradeFullTextTest
  {
    [OneTimeSetUp]
    public void TestFixtureSetUp() => Require.AllFeaturesSupported(ProviderFeatures.FullText);

    [SetUp]
    public void SetUp() => BuildDomain("Version1", DomainUpgradeMode.Recreate).Dispose();

    [Test]
    public void UpgradeTest()
    {
      BuildDomain("Version2", DomainUpgradeMode.Perform).Dispose();
      BuildDomain("Version3", DomainUpgradeMode.Perform).Dispose();
      BuildDomain("Version4", DomainUpgradeMode.Perform).Dispose();
      BuildDomain("Version5", DomainUpgradeMode.Perform).Dispose();
      BuildDomain("Version6", DomainUpgradeMode.Perform).Dispose();
    }

    public async Task UpgradeAsyncTest()
    {
      (await BuildDomainAsync("Version2", DomainUpgradeMode.Perform)).Dispose();
      (await BuildDomainAsync("Version3", DomainUpgradeMode.Perform)).Dispose();
      (await BuildDomainAsync("Version4", DomainUpgradeMode.Perform)).Dispose();
      (await BuildDomainAsync("Version5", DomainUpgradeMode.Perform)).Dispose();
      (await BuildDomainAsync("Version6", DomainUpgradeMode.Perform)).Dispose();
    }

    private Domain BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(),
        "Xtensive.Orm.Tests.Upgrade.FullText.Model." + version);
      using (Upgrader.Enable(version)) {
        var domain = Domain.Build(configuration);
        return domain;
      }
    }

    private async Task<Domain> BuildDomainAsync(string version, DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(),
        "Xtensive.Orm.Tests.Upgrade.FullText.Model." + version);
      using (Upgrader.Enable(version)) {
        var domain = await Domain.BuildAsync(configuration);
        return domain;
      }
    }
  }
}