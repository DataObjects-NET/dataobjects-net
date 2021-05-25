// Copyright (C) 2016-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2016.02.09

using System;
using System.Linq;
using System.Threading.Tasks;
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
      public override bool CanUpgradeFrom(string oldVersion) => true;

      public override void OnComplete(Domain domain)
      {
        var cleanUpWorker = SqlWorker.Create(this.UpgradeContext.Services, SqlWorkerTask.DropSchema);
        _ = cleanUpWorker.Invoke();
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade
{
  public sealed class SimpleTest : BuildOnEmptySchemaBase
  {
    protected override void EnsureDomainWorksCorrectly(Domain domain)
    {
      using (domain) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          foreach (var intValue in Enumerable.Range(1000, 10)) {
            _ = Symbol.Intern(session, intValue.ToString());
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
  }

  public sealed class MultischemaTest : BuildOnEmptySchemaBase
  {
    private const string Schema1 = WellKnownSchemas.Schema1;
    private const string Schema2 = WellKnownSchemas.Schema2;

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
    }

    protected override void EnsureDomainWorksCorrectly(Domain domain)
    {
      using (domain) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          foreach (var intValue in Enumerable.Range(1000, 10)) {
            _ = Symbol.Intern(session, intValue.ToString());
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

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.DefaultSchema = Schema1;
      configuration.MappingRules.Map(typeof(Symbol).Namespace).ToSchema(Schema1);
      configuration.MappingRules.Map(typeof(Symbol2).Namespace).ToSchema(Schema2);
      return configuration;
    }

    protected override DomainConfiguration BuildInitialConfiguration()
    {
      var configuration = base.BuildInitialConfiguration();
      configuration.DefaultSchema = Schema1;
      configuration.MappingRules.Map(typeof(Symbol).Namespace).ToSchema(Schema1);
      configuration.MappingRules.Map(typeof(Symbol2).Namespace).ToSchema(Schema2);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }

  public sealed class MultiDatabaseTest : BuildOnEmptySchemaBase
  {
    private const string DOTests2Db = WellKnownDatabases.MultiDatabase.AdditionalDb1;
    private const string DOTests1Db = WellKnownDatabases.MultiDatabase.AdditionalDb2;

    private const string DefauldSchema = WellKnownSchemas.Schema1;

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }

    protected override void EnsureDomainWorksCorrectly(Domain domain)
    {
      using (domain) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          foreach (var intValue in Enumerable.Range(1000, 10)) {
            _ = Symbol.Intern(session, intValue.ToString());
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

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.DefaultDatabase = DOTests2Db;
      configuration.DefaultSchema = DefauldSchema;
      configuration.MappingRules.Map(typeof(Symbol).Namespace).ToDatabase(DOTests2Db);
      configuration.MappingRules.Map(typeof(Symbol2).Namespace).ToDatabase(DOTests1Db);
      return configuration;
    }

    protected override DomainConfiguration BuildInitialConfiguration()
    {
      var configuration = base.BuildInitialConfiguration();
      configuration.DefaultDatabase = DOTests2Db;
      configuration.DefaultSchema = DefauldSchema;
      configuration.MappingRules.Map(typeof(Symbol).Namespace).ToDatabase(DOTests2Db);
      configuration.MappingRules.Map(typeof(Symbol2).Namespace).ToDatabase(DOTests1Db);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }

  [TestFixture]
  public abstract class BuildOnEmptySchemaBase
  {
    [OneTimeSetUp]
    protected void TestFixtureSetUp() => CheckRequirements();

    [Test]
    public void PerformTest()
    {
      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Perform;
      using var domain = BuildDomain(configuration);
      EnsureDomainWorksCorrectly(domain);
    }

    [Test]
    public async Task PerformAsyncTest()
    {
      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Perform;
      using var domain = await BuildDomainAsync(configuration);
      EnsureDomainWorksCorrectly(domain);
    }

    [Test]
    public void RecreateTest()
    {
      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      using var domain = BuildDomain(configuration);
      EnsureDomainWorksCorrectly(domain);
    }

    [Test]
    public async Task RecreateAsyncTest()
    {
      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      using var domain = await BuildDomainAsync(configuration);
      EnsureDomainWorksCorrectly(domain);
    }

    [Test]
    public void SkipTest()
    {
      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      using var domain = BuildDomain(configuration);
    }

    [Test]
    public async Task SkipAsyncTest()
    {
      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      using var domain = await BuildDomainAsync(configuration);
    }

    protected Domain BuildDomain(DomainConfiguration configuration)
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

    protected async Task<Domain> BuildDomainAsync(DomainConfiguration configuration)
    {
      try {
        ClearSchema();
        return await Domain.BuildAsync(configuration);
      }
      catch (Exception e) {
        TestLog.Error(GetType().GetFullName());
        TestLog.Error(e);
        throw;
      }
    }

    protected abstract void EnsureDomainWorksCorrectly(Domain domain);

    protected virtual void RegisterTypes(DomainConfiguration configuration)
    {
      configuration.Types.Register(typeof(Symbol));
      configuration.Types.Register(typeof(Symbol2));
    }

    protected virtual void CheckRequirements()
    {
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      RegisterTypes(configuration);
      return configuration;
    }

    protected virtual DomainConfiguration BuildInitialConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (CleanupUpgradeHandler));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    private void ClearSchema() => Domain.Build(BuildInitialConfiguration()).Dispose();
  }
}
