using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using model = Xtensive.Orm.Tests.Storage.ChangedPersistOrderTestModel;

namespace Xtensive.Orm.Tests.Storage.ChangedPersistOrderTestModel
{
  [HierarchyRoot]
  public class Worker : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public EntitySet<EmploymentContract> EmploymentContracts { get; set; }
  }

  [HierarchyRoot]
  [Index("ContractNumber", Unique = true)]
  public class EmploymentContract : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 10)]
    public string ContractNumber { get; set; }

    [Field]
    public DateTime CreationDate { get; set; }

    [Field]
    public DateTime TakesEffectFrom { get; set; }

    [Field]
    public DateTime ContractEnds { get; set; }

    [Field]
    [Association(PairTo = "EmploymentContracts")]
    public Worker Worker { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.ChangedPersistOrderTest
{
  [TestFixture]
  public class ChangedPersistOrderTest : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      FillData(ForeignKeyMode.None);
      var domain = BuildDomain(ForeignKeyMode.None, DomainUpgradeMode.Perform);
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      {
        var contract = session.Query.All<model.EmploymentContract>().First(el => el.ContractNumber == "w123456");
        var worker = contract.Worker;
        contract.Remove();
        var newContract = new model.EmploymentContract { ContractNumber = "w123456", CreationDate = DateTime.Now, TakesEffectFrom = DateTime.Now.Date, Worker = worker };
        transaction.Complete();
      }
    }

    private void FillData(ForeignKeyMode mode)
    {
      var domain = BuildDomain(mode, DomainUpgradeMode.Recreate);
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      {
        var worker = new model.Worker { FirstName = "John", LastName = "Smith" };
        new model.EmploymentContract { ContractNumber = "w123456", CreationDate = DateTime.Now, TakesEffectFrom = DateTime.Now.Date };
        transaction.Complete();
      }
    }

    private Domain BuildDomain(ForeignKeyMode foreignKeyMode, DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof(model.EmploymentContract).Assembly, typeof(model.EmploymentContract).Namespace);
      configuration.UpgradeMode = upgradeMode;
      configuration.ForeignKeyMode = foreignKeyMode;
      return Domain.Build(configuration);
    }
  }
}