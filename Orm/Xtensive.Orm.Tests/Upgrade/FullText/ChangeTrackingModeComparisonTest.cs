// Copyright (C) 2017-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.07.17

using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Upgrade.FullText.Model.Version2;

namespace Xtensive.Orm.Tests.Upgrade.FullText
{
  [TestFixture]
  public class ChangeTrackingModeComparisonTest
  {
    [OneTimeSetUp]
    public void TestFixtureSetUp() => Require.AllFeaturesSupported(ProviderFeatures.FullText);

    [SetUp]
    public void SetUp()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.FullTextChangeTrackingMode = FullTextChangeTrackingMode.Auto;
      configuration.Types.Register(typeof(Article));
      Domain.Build(configuration).Dispose();
    }

    [Mute]
    [Test]
    public void MainTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.FullText);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Validate;
      configuration.FullTextChangeTrackingMode = FullTextChangeTrackingMode.Off;
      configuration.Types.Register(typeof(Article));
      Domain.Build(configuration).Dispose();
    }

    [Mute]
    [Test]
    public async Task MainAsyncTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Validate;
      configuration.FullTextChangeTrackingMode = FullTextChangeTrackingMode.Off;
      configuration.Types.Register(typeof(Article));
      (await Domain.BuildAsync(configuration)).Dispose();
    }
  }
}