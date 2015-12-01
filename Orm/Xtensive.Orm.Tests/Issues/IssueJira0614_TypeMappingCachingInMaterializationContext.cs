using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.IssueJira0614_MaterializationContextBugModel;
using Xtensive.Orm.Tests.Storage.VersionBehavior.Model;
using Xtensive.Orm.Upgrade;
using Node1 = Xtensive.Orm.Tests.Issues.IssueJira0614_MaterializationContextBugModel.Source.Node1;
using Node2 = Xtensive.Orm.Tests.Issues.IssueJira0614_MaterializationContextBugModel.Source.Node2;
using Target = Xtensive.Orm.Tests.Issues.IssueJira0614_MaterializationContextBugModel.Target;
using Xtensive.Reflection;

namespace Xtensive.Orm.Tests.Issues.IssueJira0614_MaterializationContextBugModel
{
  namespace Source.Node1
  {
    public abstract class BusinessEntityBase : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      public bool Active { get; set; }

      public BusinessEntityBase(Session session)
        : base(session)
      { }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    [Index("Code", Unique = true)]
    public class TimesheetCode : BusinessEntityBase
    {
      public const string Other = "Other";
      public const string Meal = "Meal";
      public const string Training = "Training";

      [Field(Length = 25)]
      public string EventName { get; set; }

      [Field(Length = 8, Nullable = false)]
      public string Code { get; set; }

      [Field(Length = 100)]
      public string Description { get; set; }

      [Field]
      public bool IsPaid { get; set; }

      public string GetDisplayName()
      {
        return string.IsNullOrEmpty(EventName) ? Code : EventName;
      }

      public TimesheetCode(Session session)
        : base(session)
      { }
    }

    public class CustomUpgradeHandler : UpgradeHandler
    {
      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }

      public override void OnPrepare()
      {
        UpgradeContext.UserDefinedTypeMap.Add(typeof(TimesheetCode).FullName, 288);
      }
    }
  }

  namespace Source.Node2
  {
    public abstract class BusinessEntityBase : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      public bool Active { get; set; }

      public BusinessEntityBase(Session session)
        : base(session)
      { }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    [Index("Code", Unique = true)]
    public class TimesheetCode : BusinessEntityBase
    {
      public const string Other = "Other";
      public const string Meal = "Meal";
      public const string Training = "Training";

      [Field(Length = 25)]
      public string EventName { get; set; }

      [Field(Length = 8, Nullable = false)]
      public string Code { get; set; }

      [Field(Length = 100)]
      public string Description { get; set; }

      [Field]
      public bool IsPaid { get; set; }

      public string GetDisplayName()
      {
        return string.IsNullOrEmpty(EventName) ? Code : EventName;
      }

      public TimesheetCode(Session session)
        : base(session)
      { }
    }

    public class CustomUpgradeHandler : UpgradeHandler
    {
      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }

      public override void OnPrepare()
      {
        UpgradeContext.UserDefinedTypeMap.Add(typeof(TimesheetCode).FullName, 284);
      }
    }
  }

  namespace Target
  {
    public abstract class BusinessEntityBase : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      public bool Active { get; set; }

      public BusinessEntityBase(Session session)
        : base(session)
      { }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    [Index("Code", Unique = true)]
    public class TimesheetCode : BusinessEntityBase
    {
      public const string Other = "Other";
      public const string Meal = "Meal";
      public const string Training = "Training";

      [Field(Length = 25)]
      public string EventName { get; set; }

      [Field(Length = 8, Nullable = false)]
      public string Code { get; set; }

      [Field(Length = 100)]
      public string Description { get; set; }

      [Field]
      public bool IsPaid { get; set; }

      public string GetDisplayName()
      {
        return string.IsNullOrEmpty(EventName) ? Code : EventName;
      }

      public TimesheetCode(Session session)
        : base(session)
      { }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0614_MaterializationContextModel
  {
    private const string Node2Name = "Node2";
    private const string Node1Name = "Node1";
    private const string ErrorInTestFixtureSetup = "Error in Text fixture setup {0}";

    [Test]
    public void Test01()
    {
      var configuration1 = CreateConfiguration(typeof (Node1.TimesheetCode), DomainUpgradeMode.Recreate, "Model1");
      var configuration2 = CreateConfiguration(typeof (Node2.TimesheetCode), DomainUpgradeMode.Recreate, "Model2");

      using (var domain = BuildDomain(configuration1)) {
        Assert.That(domain.Model.Types[typeof (Node1.TimesheetCode)].TypeId, Is.EqualTo(288));
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          for (var i = 0; i < 10; i++) {
            new Node1.TimesheetCode(session) {
              Active = i % 2==0,
              Code = "jdfgdj" + i,
              Description = "dfjghjdhfgjhsjkhgjdfg",
              EventName = "Event" + i,
              IsPaid = true
            };
          }
          transaction.Complete();
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var list = session.Query.All<Node1.TimesheetCode>()
            .Where(c => c.Active)
            .OrderBy(c => c.Code)
            .AsEnumerable()
            .Select(c => new {
              Value = c.Code,
              Name = c.Code
            }).ToList();
          Assert.That(list.Count, Is.EqualTo(5));
        }
      }

      using (var domain = BuildDomain(configuration2)) {
        Assert.That(domain.Model.Types[typeof(Node2.TimesheetCode)].TypeId, Is.EqualTo(284));
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          for (var i = 0; i < 10; i++) {
            new Node2.TimesheetCode(session) {
              Active = i % 2 == 0,
              Code = "jdfgdj" + i,
              Description = "dfjghjdhfgjhsjkhgjdfg",
              EventName = "Event" + i,
              IsPaid = true
            };
          }
          transaction.Complete();
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var list = session.Query.All<Node2.TimesheetCode>()
            .Where(c => c.Active)
            .OrderBy(c => c.Code)
            .AsEnumerable()
            .Select(c => new {
              Value = c.Code,
              Name = c.Code
            }).ToList();
          Assert.That(list.Count, Is.EqualTo(5));
        }
      }

      var multinodeDomainConfiguration = CreateConfiguration(typeof (Target.TimesheetCode), DomainUpgradeMode.Skip, "Model1");
      var nodeConfiguration = CreateNodeConfiguration(Node2Name, "Model1", "Model2", DomainUpgradeMode.Skip);
      using (var domain = BuildDomain(multinodeDomainConfiguration)) {
        domain.StorageNodeManager.AddNode(nodeConfiguration);
        using (var session = domain.OpenSession()) {
          session.SelectStorageNode(WellKnown.DefaultNodeId);
          using (var transaction = session.OpenTransaction()) {
            var list = session.Query.All<Target.TimesheetCode>()
              .Where(c => c.Active)
              .OrderBy(c => c.Code)
              .AsEnumerable()
              .Select(c => new {
                Value = c.Code,
                Name = c.Code
              }).ToList();
            Assert.That(list.Count, Is.EqualTo(5));
          }

          using (var transaction = session.OpenTransaction()) {
            var list = session.Query.All<Target.TimesheetCode>()
              .Where(c => c.Active)
              .OrderBy(c => c.Code)
              .AsEnumerable()
              .Select(c => new {
                Value = c.Code,
                Name = c.Code
              }).ToList();
            Assert.That(list.Count, Is.EqualTo(5));
          }
        }

        using (var session = domain.OpenSession()) {
          session.SelectStorageNode(Node2Name);
          using (var transaction = session.OpenTransaction()) {
            
            var list = session.Query.All<Target.TimesheetCode>()
              .Where(c => c.Active)
              .OrderBy(c => c.Code)
              .AsEnumerable()
              .Select(c => new {
                Value = c.Code,
                Name = c.Code
              }).ToList();
          }
        }
      }
    }

    [Test]
    public void Test02()
    {
      var configuration1 = CreateConfiguration(typeof(Node1.TimesheetCode), DomainUpgradeMode.Recreate, "Model1");
      var configuration2 = CreateConfiguration(typeof(Node2.TimesheetCode), DomainUpgradeMode.Recreate, "Model2");

      using (var domain = BuildDomain(configuration1)) {
        Assert.That(domain.Model.Types[typeof(Node1.TimesheetCode)].TypeId, Is.EqualTo(288));
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          for (var i = 0; i < 10; i++) {
            new Node1.TimesheetCode(session) {
              Active = i % 2 == 0,
              Code = "jdfgdj" + i,
              Description = "dfjghjdhfgjhsjkhgjdfg",
              EventName = "Event" + i,
              IsPaid = true
            };
          }
          transaction.Complete();
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var list = session.Query.All<Node1.TimesheetCode>()
              .Where(c => c.Active)
              .OrderBy(c => c.Code)
              .AsEnumerable()
              .Select(c => new
              {
                Value = c.Code,
                Name = c.Code
              }).ToList();
          Assert.That(list.Count, Is.EqualTo(5));
        }
      }

      using (var domain = BuildDomain(configuration2)) {
        Assert.That(domain.Model.Types[typeof(Node2.TimesheetCode)].TypeId, Is.EqualTo(284));
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          for (var i = 0; i < 10; i++) {
            new Node2.TimesheetCode(session) {
              Active = i % 2 == 0,
              Code = "jdfgdj" + i,
              Description = "dfjghjdhfgjhsjkhgjdfg",
              EventName = "Event" + i,
              IsPaid = true
            };
          }
          transaction.Complete();
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var list = session.Query.All<Node2.TimesheetCode>()
              .Where(c => c.Active)
              .OrderBy(c => c.Code)
              .AsEnumerable()
              .Select(c => new
              {
                Value = c.Code,
                Name = c.Code
              }).ToList();
          Assert.That(list.Count, Is.EqualTo(5));
        }
      }

      var multinodeDomainConfiguration = CreateConfiguration(typeof(Target.TimesheetCode), DomainUpgradeMode.PerformSafely, "Model2");
      var nodeConfiguration = CreateNodeConfiguration(Node1Name, "Model2", "Model1", DomainUpgradeMode.PerformSafely);
      using (var domain = BuildDomain(multinodeDomainConfiguration, nodeConfiguration)) {
        using (var session = domain.OpenSession()) {
          session.SelectStorageNode(WellKnown.DefaultNodeId);
          using (var transaction = session.OpenTransaction()) {
            var list = session.Query.All<Target.TimesheetCode>()
              .Where(c => c.Active)
              .OrderBy(c => c.Code)
              .AsEnumerable()
              .Select(c => new
              {
                Value = c.Code,
                Name = c.Code
              }).ToList();
            Assert.That(list.Count, Is.EqualTo(5));
          }
          using (var transaction = session.OpenTransaction()) {
            var list = session.Query.All<Target.TimesheetCode>()
              .Where(c => c.Active)
              .OrderBy(c => c.Code)
              .AsEnumerable()
              .Select(c => new
              {
                Value = c.Code,
                Name = c.Code
              }).ToList();
            Assert.That(list.Count, Is.EqualTo(5));
          }
        }

        using (var session = domain.OpenSession()) {
          session.SelectStorageNode(Node1Name);
          using (var transaction = session.OpenTransaction()) {

            var list = session.Query.All<Target.TimesheetCode>()
              .Where(c => c.Active)
              .OrderBy(c => c.Code)
              .AsEnumerable()
              .Select(c => new
              {
                Value = c.Code,
                Name = c.Code
              }).ToList();
          }
          using (var transaction = session.OpenTransaction()) {
            var list = session.Query.All<Target.TimesheetCode>()
              .Where(c => c.Active)
              .OrderBy(c => c.Code)
              .AsEnumerable()
              .Select(c => new {
                Value = c.Code,
                Name = c.Code
              }).ToList();
          }
        }
      }
    }

    private void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    private Domain BuildDomain(DomainConfiguration configuration, NodeConfiguration nodeConfiguration = null)
    {
      try{
        var domain = Domain.Build(configuration);
        if (nodeConfiguration!=null)
          domain.StorageNodeManager.AddNode(nodeConfiguration);
        return domain;
      }
      catch (Exception e) {
        TestLog.Error(GetType().GetFullName());
        TestLog.Error(e);
        throw;
      }
    }

    private DomainConfiguration CreateConfiguration(Type type, DomainUpgradeMode upgradeMode, string defaultSchema)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(type.Assembly, type.Namespace);
      configuration.DefaultSchema = defaultSchema;
      return configuration;
    }

    private NodeConfiguration CreateNodeConfiguration(string nodeId, string oldSchema, string newSchema, DomainUpgradeMode upgradeMode)
    {
      var configuration = new NodeConfiguration(nodeId);
      configuration.SchemaMapping.Add(oldSchema, newSchema);
      configuration.UpgradeMode = upgradeMode;
      return configuration;
    }

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      try {
        CheckRequirements();
      }
      catch (IgnoreException) {
        throw;
      }
      catch (Exception e) {
        Debug.WriteLine(ErrorInTestFixtureSetup, e);
        throw;
      }
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
    }
  }
}
