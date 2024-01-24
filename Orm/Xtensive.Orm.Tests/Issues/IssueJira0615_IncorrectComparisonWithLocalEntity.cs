// Copyright (C) 2019-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kudelin
// Created:    2019.01.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0615_IncorrectComparisonWithLocalEntityModel;

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueJira0615_IncorrectComparisonWithLocalEntity : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(MesObject).Assembly, typeof(MesObject).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var sharedFlow = new LogisticFlow(session, 1000);
        var uniqueFlow = new LogisticFlow(session, 1100);

        _ = new PickingProductRequirement(session, 10) {
          Quantity = new DimensionalField(session, 36),
          InventoryAction = new InventoryAction(session, 100) {
            LogisticFlow = sharedFlow
          }
        };

        _ = new PickingProductRequirement(session, 20) {
          Quantity = new DimensionalField(session, 35),
          InventoryAction = new InventoryAction(session, 200) {
            LogisticFlow = sharedFlow
          }
        };

        _ = new PickingProductRequirement(session, 30) {
          Quantity = new DimensionalField(session, 34),
          InventoryAction = new InventoryAction(session, 300) {
            LogisticFlow = sharedFlow
          }
        };

        _ = new PickingProductRequirement(session, 40) {
          Quantity = new DimensionalField(session, 34),
          InventoryAction = new InventoryAction(session, 400) {
            LogisticFlow = uniqueFlow
          }
        };

        transaction.Complete();
      }
    }

    [Test]
    public void SimpleCaseOfLocalEntityComparison()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var requirement = session.Query.All<PickingProductRequirement>().First();
        var result = session.Query.All<PickingProductRequirement>()
          .Select(x => new { V1 = x == requirement })
          .OrderBy(o => o.V1).ToArray();

        Assert.That(result.Length, Is.EqualTo(4));
        Assert.That(result.All(x => x.V1 == true), Is.False);
        Assert.That(result.Any(x => x.V1 == true), Is.True);
      }
    }

    [Test]
    public void SequenceOfMembersInOrderBy()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var requirement = session.Query.All<PickingProductRequirement>().First();
        var result = session.Query.All<PickingProductRequirement>()
          .Select(x => new { V1 = new { V2 = x == requirement } })
          .OrderBy(o => o.V1.V2)
          .ToArray();

        Assert.That(result.Length, Is.EqualTo(4));
        Assert.That(result.All(x => x.V1.V2 == true), Is.False);
        Assert.That(result.Any(x => x.V1.V2 == true), Is.True);
      }
    }


    [Test]
    public void FilterByNewlyCreatedEntity()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var stockFlow = new LogisticFlow(session, 1);
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == stockFlow ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();
        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == stockFlow ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.All(a => a.V2 < 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void FilterByNewlyCreatedEntityReduced()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var stockFlow = new LogisticFlow(session, 1);
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int)(p.InventoryAction.LogisticFlow == stockFlow ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();
        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.LogisticFlow == stockFlow ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.All(a => a.V2 == 1), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void FilterByNewlyCreatedEntityKey()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var stockFlow = new LogisticFlow(session, 1);
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow.Key == stockFlow.Key ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();
        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow.Key == stockFlow.Key ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.All(a => a.V2 < 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void FilterByNewlyCreatedEntityId()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var stockFlow = new LogisticFlow(session, 1);
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow.ID == stockFlow.ID ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();
        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow.ID == stockFlow.ID ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.All(a => a.V2 < 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void FilterByNewlyCreatedEntityRef()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        var stockFlow = new LogisticFlow(session, 1);
        var inventoryAction = new InventoryAction(session, 2) { LogisticFlow = stockFlow, Product = new Product(session, 3) };

        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == inventoryAction.LogisticFlow ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();
        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == inventoryAction.LogisticFlow ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.All(a => a.V2 < 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void FilterByNewlyCreatedEntityRefKey()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var stockFlow = new LogisticFlow(session, 1);
        var inventoryAction = new InventoryAction(session, 2) { LogisticFlow = stockFlow, Product = new Product(session, 3) };

        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow.Key == inventoryAction.LogisticFlow.Key ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();
        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow.Key == inventoryAction.LogisticFlow.Key ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.All(a => a.V2 < 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void FilterByNewlyCreatedEntityRefId()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        var stockFlow = new LogisticFlow(session, 1);
        var inventoryAction = new InventoryAction(session, 2) { LogisticFlow = stockFlow, Product = new Product(session, 3) };

        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow.ID == inventoryAction.LogisticFlow.ID ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();
        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow.ID == inventoryAction.LogisticFlow.ID ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.All(a => a.V2 < 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void FilterByAlreadyExistedCreatedEntity()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var sharedFlow = session.Query.All<LogisticFlow>().First(f => f.ID == 1000);

        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(1).All(a => a.V2 < 40), Is.False);
        Assert.That(results.Skip(1).All(a => a.V2 > 40 && a.V2 < 75), Is.True);
        Assert.That(results[0].V2, Is.LessThan(40));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void FilterByAlreadyExistedCreatedEntityReduced()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var sharedFlow = session.Query.All<LogisticFlow>().First(f => f.ID == 1000);

        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(1).All(a => a.V2==2), Is.True);
        Assert.That(results[0].V2, Is.EqualTo(1));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0615_IncorrectComparisonWithLocalEntityModel
{
  [HierarchyRoot]
  public class InventoryAction : MesObject
  {
    [Field]
    public Product Product { get; set; }

    [Field]
    public LogisticFlow LogisticFlow { get; set; }

    public InventoryAction(Session session, int id)
      : base(session, id)
    {
    }
  }

  [HierarchyRoot]
  public class LogisticFlow : MesObject
  {
    public LogisticFlow(Session session, int id)
      : base(session, id)
    {
    }
  }

  [HierarchyRoot]
  public class Product : MesObject
  {
    [Field]
    public string MeasureType { get; set; }

    public Product(Session session, int id)
      : base(session, id)
    {
    }
  }

  [HierarchyRoot]
  public class PickingProductRequirement : MesObject
  {
    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Deny)]
    public Product Product { get; private set; }

    [Field]
    public DimensionalField Quantity { get; set; }

    [Field]
    public InventoryAction InventoryAction { get; set; }

    public PickingProductRequirement(Session session, int id)
      : base(session, id)
    {
    }
  }

  public class DimensionalField : Structure
  {
    [Field]
    public decimal NormalizedValue { get; private set; }

    public DimensionalField(Session session, decimal nValue)
      : base(session)
    {
      NormalizedValue = nValue;
    }
  }

  public abstract class MesObject : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    protected MesObject(Session session, int id)
      : base(session, id)
    {
    }

    protected MesObject(Session session)
      : base(session)
    {
    }
  }
}
