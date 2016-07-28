// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.07.26

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0637_EntitySetFullOfNullsOnEnumerationModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0637_EntitySetFullOfNullsOnEnumerationModel
{
  [HierarchyRoot]
  public class Table : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public EntitySet<TableLeg> Legs { get; set; }
  }

  [HierarchyRoot]
  public class TableLeg : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Legs", OnOwnerRemove = OnRemoveAction.None, OnTargetRemove = OnRemoveAction.None)]
    public Table Table { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0637_EntitySetFullOfNullsOnEnumeration : AutoBuildTest
  {
    [Test]
    public void DirectEnumerationTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var table = session.Query.All<Table>().First();
        session.Remove(table.Legs);
        int itterations = 0;
        foreach (var leg in table.Legs.AsEnumerable()) {
          itterations++;
          Assert.That(leg, Is.Not.Null);
        }
        Assert.That(itterations, Is.EqualTo(0));
      }
    }

    [Test]
    public void ToListTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var table = session.Query.All<Table>().First();
        session.Remove(table.Legs);
        int itterations = 0;
        foreach (var leg in table.Legs.ToList()) {
          itterations++;
          Assert.That(leg, Is.Not.Null);
        }
        Assert.That(itterations, Is.EqualTo(0));
      }
    }

    [Test]
    public void ToArrayTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var table = session.Query.All<Table>().First();
        session.Remove(table.Legs);
        int itterations = 0;
        foreach (var leg in table.Legs.ToArray()) {
          itterations++;
          Assert.That(leg, Is.Not.Null);
        }
        Assert.That(itterations, Is.EqualTo(0));
      }
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var table = new Table { Name = "Ikea coffee table" };
        table.Legs.Add(new TableLeg { Name = "Leg 1" });
        table.Legs.Add(new TableLeg { Name = "Leg 2" });
        table.Legs.Add(new TableLeg { Name = "Leg 3" });
        table.Legs.Add(new TableLeg { Name = "Leg 4" });

        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (Table).Assembly, typeof (Table).Namespace);
      return configuration;
    }
  }
}

