// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.29

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Metadata;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class CustomMetadataUpgrade
  {
    [Test]
    public void MainTest()
    {
      var name = "CustomExtension";
      var text = "Hello world";
      var data = new[] {(byte) 1, (byte) 100};

      using (var initialDomain = BuildDomain(DomainUpgradeMode.Recreate))
      using (var session = initialDomain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new Extension(name) {Text = text, Data = data};
        tx.Complete();
      }

      using (var upgradedDomain = BuildDomain(DomainUpgradeMode.PerformSafely))
      using (var session = upgradedDomain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var extension = session.Query.All<Extension>().FirstOrDefault(e => e.Name==name);
        Assert.That(extension, Is.Not.Null);
        Assert.That(extension.Text, Is.EqualTo(text));
        Assert.That(extension.Data, Is.EquivalentTo(data));
        tx.Complete();
      }
    }

    private Domain BuildDomain(DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      return Domain.Build(configuration);
    }
  }
}