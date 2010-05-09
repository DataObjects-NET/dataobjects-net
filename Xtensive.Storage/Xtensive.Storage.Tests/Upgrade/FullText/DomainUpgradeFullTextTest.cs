// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.21

using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Core;

namespace Xtensive.Storage.Tests.Upgrade.FullText
{
  [TestFixture, Category("Upgrade")]
  public class DomainUpgradeFullTextTest
  {
    private Domain domain;

    [TestFixtureSetUp]
    public void TestSetUp()
    {
      Require.ProviderIsNot(StorageProvider.Memory);
    }

    [SetUp]
    public void SetUp()
    {
      BuildDomain("Version1", DomainUpgradeMode.Recreate);
    }

    [Test]
    public void UpgradeTest()
    {
      BuildDomain("Version2", DomainUpgradeMode.Perform);
      BuildDomain("Version3", DomainUpgradeMode.Perform);
      BuildDomain("Version4", DomainUpgradeMode.Perform);
      BuildDomain("Version5", DomainUpgradeMode.Perform);
      BuildDomain("Version6", DomainUpgradeMode.Perform);
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(),
        "Xtensive.Storage.Tests.Upgrade.FullText.Model." + version);
      using (Upgrader.Enable(version))
        domain = Domain.Build(configuration);
    }
  }
}