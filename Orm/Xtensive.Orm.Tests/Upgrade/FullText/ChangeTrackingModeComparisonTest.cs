// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.07.17

using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Upgrade.FullText.Model.Version2;

namespace Xtensive.Orm.Tests.Upgrade.FullText
{
  [TestFixture]
  public class ChangeTrackingModeComparisonTest
  {
    [Test]
    public void MainTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.FullText);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.FullTextChangeTrackingMode = FullTextChangeTrackingMode.Auto;
      configuration.Types.Register(typeof (Article));
      using (var domain = Domain.Build(configuration)) { }

      configuration = configuration.Clone();
      configuration.UpgradeMode = DomainUpgradeMode.Validate;
      configuration.FullTextChangeTrackingMode = FullTextChangeTrackingMode.Off;
      using (var domain = Domain.Build(configuration)){ }
    }
  }
}