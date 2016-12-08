// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.09

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Upgrade.BuildOnEmptySchemaModel.CleanUpUpgrader;
using Xtensive.Orm.Tests.Upgrade.BuildOnEmptySchemaModel.TestModel;
using Xtensive.Orm.Tests.Upgrade.BuildOnEmptySchemaModel.TestModel2;
using Xtensive.Orm.Upgrade;
using Xtensive.Reflection;

namespace Xtensive.Orm.Tests.Upgrade.BuildOnEmptySchemaModel
{
  namespace TestModel
  {
    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    [Index("Name", Unique = true)]
    public class Symbol : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 255)]
      public string Name { get; private set; }

      static public Symbol Intern(Session session, string name)
      {
        return session.Query.All<Symbol>().Where(s => s.Name==name).SingleOrDefault()
            ?? new Symbol(session) { Name = name };
      }

      private Symbol(Session session)
        : base(session)
      {
      }
    }
  }

  namespace TestModel2
  {
    [HierarchyRoot]
    public class Symbol2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; private set; }
    }
  }

  namespace CleanUpUpgrader
  {
    public class CleanupUpgradeHandler : UpgradeHandler
    {
      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }

      public override void OnComplete(Domain domain)
      {
        var cleanUpWorker = SqlWorker.Create(this.UpgradeContext.Services, SqlWorkerTask.DropSchema);
        cleanUpWorker.Invoke();
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture]
  public sealed class BuildOnEmptySchemaTest
  {
    private const string ErrorInTestFixtureSetup = "Error in TestFixtureSetUp:\r\n{0}";

    [Test]
    public void MainTest()
    {
      var configuration = BuildSimpleConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      using (var domain = RebuildDomain(configuration)) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          foreach (var intValue in Enumerable.Range(1000, 10)) {
            Symbol.Intern(session, intValue.ToString());
          }
          transaction.Complete();
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var symbols = session.Query.All<Symbol>().ToArray();
          Assert.That(symbols.Length, Is.EqualTo(10));
        }
      }
    }

    [Test]
    public void PerformTest()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.Multidatabase);
      var configuration = BuildSimpleConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Perform;
      Assert.DoesNotThrow(() => RebuildDomain(configuration));
    }

    [Test]
    public void RecreateTest()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.Multidatabase);
      var configuration = BuildSimpleConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      Assert.DoesNotThrow(() => RebuildDomain(configuration));
    }

    [Test]
    public void SkipTest()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.Multidatabase);
      var configuration = BuildSimpleConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      Assert.DoesNotThrow(() => RebuildDomain(configuration));
    }

    [Test]
    public void MultiSchemaPerformTest()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.Multidatabase);
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
      var configuration = BuildMultischemaConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Perform;
      Assert.DoesNotThrow(() => RebuildDomain(configuration));
    }

    [Test]
    public void MultiSchemaRecreateTest()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.Multidatabase);
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
      var configuration = BuildMultischemaConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      Assert.DoesNotThrow(() => RebuildDomain(configuration));
    }

    [Test]
    public void MultiSchemaSkipTest()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.Multidatabase);
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
      var configuration = BuildMultischemaConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      Assert.DoesNotThrow(() => RebuildDomain(configuration));
    }

    [Test]
    public void MultiDatabasePerformTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
      var configuration = BuildMultiDataBaseConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Perform;
      Assert.DoesNotThrow(() => RebuildDomain(configuration));
    }

    [Test]
    public void MultiDatabaseRecreateTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
      var configuraton = BuildMultiDataBaseConfiguration();
      configuraton.UpgradeMode = DomainUpgradeMode.Recreate;
      Assert.DoesNotThrow(() => RebuildDomain(configuraton));
    }

    [Test]
    public void MultiDatabaseSkipTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
      var configuration = BuildMultiDataBaseConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      Assert.DoesNotThrow(() => RebuildDomain(configuration));
    }

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      try {
        ClearSchema();
      }
      catch (IgnoreException) {
        throw;
      }
      catch (Exception e) {
        Debug.WriteLine(ErrorInTestFixtureSetup, e);
        throw;
      }
    }

    private Domain RebuildDomain(DomainConfiguration configuration)
    {
      try {
        ClearSchema();
        return Domain.Build(configuration);
      }
      catch (Exception e) {
        TestLog.Error(GetType().GetFullName());
        TestLog.Error(e);
        throw;
      }
    }

    private void ClearSchema()
    {
      using (Domain.Build(BuildInitialConfiguration())) {}
    }

    private void RegisterTypes(DomainConfiguration configuration)
    {
      configuration.Types.Register(typeof (Symbol));
      configuration.Types.Register(typeof (Symbol2));
    }

    private DomainConfiguration BuildSimpleConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      RegisterTypes(configuration);
      return configuration;
    }

    private DomainConfiguration BuildMultischemaConfiguration()
    {
      var configuration = BuildSimpleConfiguration();
      configuration.DefaultSchema = "Model1";
      configuration.MappingRules.Map(typeof (Symbol).Namespace).ToSchema("Model1");
      configuration.MappingRules.Map(typeof (Symbol2).Namespace).ToSchema("Model2");
      return configuration;
    }

    private DomainConfiguration BuildMultiDataBaseConfiguration()
    {
      var configuration = BuildSimpleConfiguration();
      configuration.DefaultDatabase = "DO-Tests";
      configuration.DefaultSchema = "Model1";
      configuration.MappingRules.Map(typeof (Symbol).Namespace).ToDatabase("DO-Tests");
      configuration.MappingRules.Map(typeof (Symbol2).Namespace).ToSchema("Model1");
      return configuration;
    }

    private DomainConfiguration BuildInitialConfiguration()
    {
      var configruation = DomainConfigurationFactory.Create();
      configruation.Types.Register(typeof (CleanupUpgradeHandler));
      configruation.UpgradeMode = DomainUpgradeMode.Recreate;
      return configruation;
    }
  }
}