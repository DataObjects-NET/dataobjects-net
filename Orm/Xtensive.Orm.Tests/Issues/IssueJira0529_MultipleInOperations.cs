// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.05.21

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0529_MultipleInOperationsTestModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0529_MultipleInOperationsTestModel
  {
    [HierarchyRoot]
    public class TechnicalProcess : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public Equipment Equipment { get; set; }

      [Field]
      public EquipmentParameter RunIdHiParameter { get; set; }

      [Field]
      public EquipmentParameter RunIdLowParameter { get; set; }

      [Field]
      public EquipmentParameter StatusParameter { get; set; }
    }

    [HierarchyRoot]
    public class Equipment : Entity
    {
      [Key, Field]
      public long Id { get; private set; }
    }

    [HierarchyRoot]
    public class EquipmentParameter : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Url { get; set; }
    }

    [HierarchyRoot]
    public class Entity1 : Entity
    {
      [Key]
      [Field(Nullable = false)]
      public Guid Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public Entity2 Link { get; set; }
    }

    [HierarchyRoot]
    public class Entity2 : Entity
    {
      [Key]
      [Field(Nullable = false)]
      public Guid Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }
  }

  [TestFixture]
  public class IssueJira0529_MultipleInOperations : AutoBuildTest
  {
    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof (Equipment).Assembly, typeof (Equipment).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var equipment1 = new Equipment();
        var equipment2 = new Equipment();
        var equipment3 = new Equipment();

        var parameter1Hi = new EquipmentParameter {Url = "1Hi"};
        var parameter1Low = new EquipmentParameter {Url = "1Low"};
        var parameter1Status = new EquipmentParameter {Url = "1Status"};
        var parameter2Hi = new EquipmentParameter {Url = "2Hi"};
        var parameter2Low = new EquipmentParameter {Url = "2Low"};
        var parameter2Status = new EquipmentParameter {Url = "2Status"};
        var parameter3Hi = new EquipmentParameter {Url = "3Hi"};
        var parameter3Low = new EquipmentParameter {Url = "3Low"};
        var parameter3Status = new EquipmentParameter {Url = "3Status"};

        var technicalProcess1 = new TechnicalProcess {
          Equipment = equipment1,
          RunIdHiParameter = parameter1Hi,
          RunIdLowParameter = parameter1Low,
          StatusParameter = parameter1Status,
        };
        var technicalProcess2 = new TechnicalProcess {
          Equipment = equipment2,
          RunIdHiParameter = parameter2Hi,
          RunIdLowParameter = parameter2Low,
          StatusParameter = parameter2Status,
        };
        var technicalProcess3 = new TechnicalProcess {
          Equipment = equipment3,
          RunIdHiParameter = parameter3Hi,
          RunIdLowParameter = parameter3Low,
          StatusParameter = parameter3Status,
        };

        var equipments = new[] {equipment1, equipment2};

        var technicalProcesses = Query.All<TechnicalProcess>()
          .Where(tp => tp.Equipment.In(equipments));

        
        var urls = technicalProcesses.Select(p => p.RunIdHiParameter.Url)
          .Concat(technicalProcesses.Select(p => p.RunIdLowParameter.Url))
          .Concat(technicalProcesses.Select(p => p.StatusParameter.Url))
          .ToArray();

        Assert.That(urls.Length, Is.EqualTo(6));
        Assert.That(urls.Contains("1Hi"));
        Assert.That(urls.Contains("1Low"));
        Assert.That(urls.Contains("1Status"));
        Assert.That(urls.Contains("2Hi"));
        Assert.That(urls.Contains("2Low"));
        Assert.That(urls.Contains("2Status"));
      }
    }

    [Test]
    public void NonPersistentTypesExceptTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.Firebird);
      Require.ProviderIsNot(StorageProvider.MySql);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var someIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var query1 = session.Query.All<Entity2>().Where(a => a.Id.In(someIds));
        var query2 = session.Query.All<Entity1>().Select(a => a.Link);
        var query3 = query1.Except(query2).ToList();
      }
    }

    [Test]
    public void NonPersistentTypesIntersectTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.Firebird);
      Require.ProviderIsNot(StorageProvider.MySql);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var someIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var query1 = session.Query.All<Entity2>().Where(a => a.Id.In(someIds));
        var query2 = session.Query.All<Entity1>().Select(a => a.Link);
        var query3 = query1.Intersect(query2).ToList();
      }
    }

    [Test]
    public void NonPersistentTypesUnionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var someIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var query1 = session.Query.All<Entity2>().Where(a => a.Id.In(someIds));
        var query2 = session.Query.All<Entity1>().Select(a => a.Link);
        var query3 = query1.Union(query2).ToList();
      }
    }

    [Test]
    public void NonPersistentTypesConcatTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var someIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var query1 = session.Query.All<Entity2>().Where(a => a.Id.In(someIds));
        var query2 = session.Query.All<Entity1>().Select(a => a.Link);
        var query3 = query1.Concat(query2).ToList();
      }
    }
    
    [Test]
    public void OnlyInOperationTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction())
      {
        var equipment1 = new Equipment();
        var equipment2 = new Equipment();
        var equipment3 = new Equipment();

        var parameter1Hi = new EquipmentParameter { Url = "1Hi" };
        var parameter1Low = new EquipmentParameter { Url = "1Low" };
        var parameter1Status = new EquipmentParameter { Url = "1Status" };
        var parameter2Hi = new EquipmentParameter { Url = "2Hi" };
        var parameter2Low = new EquipmentParameter { Url = "2Low" };
        var parameter2Status = new EquipmentParameter { Url = "2Status" };
        var parameter3Hi = new EquipmentParameter { Url = "3Hi" };
        var parameter3Low = new EquipmentParameter { Url = "3Low" };
        var parameter3Status = new EquipmentParameter { Url = "3Status" };

        var technicalProcess1 = new TechnicalProcess
        {
          Equipment = equipment1,
          RunIdHiParameter = parameter1Hi,
          RunIdLowParameter = parameter1Low,
          StatusParameter = parameter1Status,
        };
        var technicalProcess2 = new TechnicalProcess
        {
          Equipment = equipment2,
          RunIdHiParameter = parameter2Hi,
          RunIdLowParameter = parameter2Low,
          StatusParameter = parameter2Status,
        };
        var technicalProcess3 = new TechnicalProcess
        {
          Equipment = equipment3,
          RunIdHiParameter = parameter3Hi,
          RunIdLowParameter = parameter3Low,
          StatusParameter = parameter3Status,
        };

        var equipments = new[] { equipment1, equipment2 };

        var technicalProcesses = Query.All<TechnicalProcess>()
          .Where(tp => tp.Equipment.In(equipments)).ToArray();

        Assert.AreEqual(2, technicalProcesses.Length);
      }
    }

    [Test]
    public void OnlyJoinOperationTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction())
      {
        var equipment1 = new Equipment();
        var equipment2 = new Equipment();
        var equipment3 = new Equipment();

        var parameter1Hi = new EquipmentParameter { Url = "1Hi" };
        var parameter1Low = new EquipmentParameter { Url = "1Low" };
        var parameter1Status = new EquipmentParameter { Url = "1Status" };
        var parameter2Hi = new EquipmentParameter { Url = "2Hi" };
        var parameter2Low = new EquipmentParameter { Url = "2Low" };
        var parameter2Status = new EquipmentParameter { Url = "2Status" };
        var parameter3Hi = new EquipmentParameter { Url = "3Hi" };
        var parameter3Low = new EquipmentParameter { Url = "3Low" };
        var parameter3Status = new EquipmentParameter { Url = "3Status" };

        var technicalProcess1 = new TechnicalProcess
        {
          Equipment = equipment1,
          RunIdHiParameter = parameter1Hi,
          RunIdLowParameter = parameter1Low,
          StatusParameter = parameter1Status,
        };
        var technicalProcess2 = new TechnicalProcess
        {
          Equipment = equipment2,
          RunIdHiParameter = parameter2Hi,
          RunIdLowParameter = parameter2Low,
          StatusParameter = parameter2Status,
        };
        var technicalProcess3 = new TechnicalProcess
        {
          Equipment = equipment3,
          RunIdHiParameter = parameter3Hi,
          RunIdLowParameter = parameter3Low,
          StatusParameter = parameter3Status,
        };

        var urls = session.Query.All<TechnicalProcess>()
          .Select(p => p.RunIdHiParameter.Url)
          .Concat(session.Query.All<TechnicalProcess>().Select(p => p.RunIdLowParameter.Url))
          .Concat(session.Query.All<TechnicalProcess>().Select(p => p.StatusParameter.Url))
          .ToArray();

        Assert.AreEqual(9, urls.Length);
      }
    }
  }
}