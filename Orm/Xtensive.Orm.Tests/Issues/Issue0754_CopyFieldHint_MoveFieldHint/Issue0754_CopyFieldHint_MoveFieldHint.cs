// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2010.11.17

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Providers;


namespace Xtensive.Orm.Tests.Issues.Issue0754_CopyFieldHint_MoveFieldHint
{
  [TestFixture]
  public class Issue0754_CopyFieldHint_MoveFieldHint
  {
    [SetUp]
    public void SetUp()
    {
      using (var domain = BuildDomain(typeof(ModelVersion1.A), DomainUpgradeMode.Recreate))
      using (var session = domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var acticle = new ModelVersion1.B() { Reference = new ModelVersion1.X() };
          tx.Complete();
        }
      }
    }

    [Test]
    public void UpgradeTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.UpdateFrom);
      using (var domain = BuildDomain(typeof(ModelVersion2.A), DomainUpgradeMode.PerformSafely))
      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var result = session.Query.All<ModelVersion2.B>().ToList();
          Assert.That(result.Count, Is.EqualTo(1));
          Assert.That(result[0].Reference, Is.Not.Null);
        }
      }
    }

    private Domain BuildDomain(Type type, DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.RegisterCaching(Assembly.GetExecutingAssembly(), type.Namespace);
      configuration.Types.Register(typeof (Upgrader));

      return Domain.Build(configuration);
    }
  }
}