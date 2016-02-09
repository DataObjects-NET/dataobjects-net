using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Upgrade.BuildOnEmptySchemaModel.CleanUpUpgrader;
using Xtensive.Orm.Tests.Upgrade.BuildOnEmptySchemaModel.TestModel;
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
      using (var domain = BuildDomain(BuildConfiguration())) {
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

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      try {
        ClearSchema();
      }
      catch (IgnoreException) {
        throw;
      }
      catch (Exception e)
      {
        Debug.WriteLine(ErrorInTestFixtureSetup, e);
        throw;
      }
    }

    private void ClearSchema()
    {
      using (var domain = BuildDomain(BuildInitialConfiguration())) {}
    }

    protected Domain BuildDomain(DomainConfiguration configuration)
    {
      try {
        return Domain.Build(configuration);
      }
      catch (Exception e)
      {
        TestLog.Error(GetType().GetFullName());
        TestLog.Error(e);
        throw;
      }
    }

    private DomainConfiguration BuildInitialConfiguration()
    {
      var configruation = DomainConfigurationFactory.Create();
      configruation.Types.Register(typeof (CleanupUpgradeHandler));
      configruation.UpgradeMode = DomainUpgradeMode.Recreate;
      return configruation;
    }

    private DomainConfiguration BuildConfiguration()
    {
      var configruation = DomainConfigurationFactory.Create();
      configruation.Types.Register(typeof(Symbol));
      configruation.UpgradeMode = DomainUpgradeMode.PerformSafely;
      return configruation;
    }
  }
}
