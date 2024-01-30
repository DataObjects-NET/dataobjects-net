// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2024.01.17

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0802_PostgreOrderByWithConditionalIssueModel;

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueJira0802_PostgreOrderByWithConditionalIssue : AutoBuildTest
  {
    private Key sharedFlowKey;

    public Session Session { get; set; }

    public TransactionScope Transaction { get; set; }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      Session = Domain.OpenSession();
    }

    public override void TestFixtureTearDown()
    {
      Session?.Dispose();
      base.TestFixtureTearDown();
    }

    [SetUp]
    public void SetUp()
    {
      Transaction = Session.OpenTransaction();
    }

    [TearDown]
    public void TearDown()
    {
      Transaction?.Dispose();
    }

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.PostgreSql);

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
        sharedFlowKey = sharedFlow.Key;

        _ = new PickingProductRequirement(session, 10) {
          Quantity = new DimensionalField(session, 36),
          InventoryAction = new InventoryAction(session, 100, sharedFlow, "a")
        };

        _ = new PickingProductRequirement(session, 20) {
          Quantity = new DimensionalField(session, 35),
          InventoryAction = new InventoryAction(session, 200, sharedFlow, "b")
        };

        _ = new PickingProductRequirement(session, 30) {
          Quantity = new DimensionalField(session, 34),
          InventoryAction = new InventoryAction(session, 300, sharedFlow, "a")
        };

        _ = new PickingProductRequirement(session, 40) {
          Quantity = new DimensionalField(session, 34),
          InventoryAction = new InventoryAction(session, 400, new LogisticFlow(session, 1100), null)
        };

        transaction.Complete();
      }
    }


    [Test]
    public void ConditionalExprByEntityInOrderBy()
    {
      var sharedFlow = Session.Query.Single(sharedFlowKey);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.GreaterThan(0));
      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Skip(1).All(a => a.V2 < 40), Is.False);
      Assert.That(results.Skip(1).All(a => a.V2 > 40 && a.V2 < 75), Is.True);
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNumberInOrderBy1()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1))
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

    [Test]
    public void ConditionalExprByNumberInOrderBy2()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Take(3).All(a => a.V2 < 40), Is.True);
      Assert.That(results.Skip(3).All(a => a.V2 > 40 && a.V2 < 75), Is.True);
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInOrderBy1()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Take(3).All(a => a.V2 < 40), Is.True);
      Assert.That(results[3].V2, Is.GreaterThan(40));
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInOrderBy2()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Skip(1).All(a => a.V2 > 40), Is.True);
      Assert.That(results[0].V2, Is.LessThan(40));
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInOrderBy3()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Take(2).All(a => a.V2 < 40), Is.True);
      Assert.That(results.Skip(2).All(a => a.V2 > 40 && a.V2 < 75), Is.True);
      Assert.That(results[0].V2, Is.LessThan(40));
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInOrderBy4()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Take(2).All(a => a.V2 < 40), Is.True);
      Assert.That(results.Skip(2).All(a => a.V2 > 40), Is.True);
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInOrderBy5()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Take(3).All(a => a.V2 < 40), Is.True);
      Assert.That(results.Skip(3).All(a => a.V2 > 40 && a.V2 < 75), Is.True);
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInOrderBy6()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Take(1).All(a => a.V2 < 40), Is.True);
      Assert.That(results.Skip(1).All(a => a.V2 > 40), Is.True);
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByEntityInOrderByDesc()
    {
      var sharedFlow = Session.Query.Single(sharedFlowKey);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Skip(3).All(a => a.V2 < 40), Is.True);
      Assert.That(results.Take(3).All(a => a.V2 > 40), Is.True);
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNumberInOrderByDesc2()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Skip(3).All(a => a.V2 < 40), Is.True);
      Assert.That(results.Take(3).All(a => a.V2 > 40), Is.True);
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNumberInOrderByDesc3()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Skip(1).All(a => a.V2 < 40), Is.True);
      Assert.That(results.Take(1).All(a => a.V2 > 40), Is.True);
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInOrderByDesc1()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Take(1).All(a => a.V2 > 40), Is.True);
      Assert.That(results.Skip(1).All(a => a.V2 < 40), Is.True);
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInOrderByDesc2()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Take(3).All(a => a.V2 > 40), Is.True);
      Assert.That(results.Skip(3).All(a => a.V2 < 40), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInOrderByDesc3()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Skip(2).All(a => a.V2 < 40), Is.True);
      Assert.That(results.Take(2).All(a => a.V2 > 40), Is.True);
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInOrderByDesc4()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Take(2).All(a => a.V2 > 40), Is.True);
      Assert.That(results.Skip(2).All(a => a.V2 < 40), Is.True);
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInOrderByDesc5()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Take(1).All(a => a.V2 > 40), Is.True);
      Assert.That(results.Skip(1).All(a => a.V2 < 40), Is.True);
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInOrderByDesc6()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .OrderByDescending(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Skip(3).All(a => a.V2 < 40), Is.True);
      Assert.That(results.Take(3).All(a => a.V2 > 40), Is.True);
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByEntityInWhere()
    {
      var sharedFlow = Session.Query.Single(sharedFlowKey);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(3));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.All(a => a.V2 > 40), Is.True);
    }

    [Test]
    public void ConditionalExprByNumberInWhere1()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(3));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.All(a => a.V2 > 40), Is.True);
    }

    [Test]
    public void ConditionalExprByNumberInWhere2()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(1));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.All(a => a.V2 > 40), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInWhere1()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(1));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.All(a => a.V2 > 40), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInWhere2()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(3));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.All(a => a.V2 > 40), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInWhere3()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(2));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.All(a => a.V2 > 40), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInWhere4()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(2));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.All(a => a.V2 > 40), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInWhere5()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(1));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.All(a => a.V2 > 40), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInWhere6()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(3));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.All(a => a.V2 > 40), Is.True);
    }

    [Test]
    public void ConditionalExprByEntityInGroupBy()
    {
      var sharedFlow = Session.Query.Single(sharedFlowKey);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));
      Assert.That(results.Length, Is.EqualTo(expected.Length));

      Assert.That(results.Select(r => r.Key).Count(a => a < 40), Is.EqualTo(1));
      Assert.That(results.Select(r => r.Key).Count(a => a > 40), Is.EqualTo(3));
    }

    [Test]
    public void ConditionalExprByNumberInGroupBy1()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(3));
      Assert.That(results.Length, Is.EqualTo(expected.Length));

      Assert.That(results.Select(r => r.Key).Count(a => a < 40), Is.EqualTo(1));
      Assert.That(results.Select(r => r.Key).Count(a => a > 40), Is.EqualTo(2));
    }

    [Test]
    public void ConditionalExprByNumberInGroupBy2()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(3));
      Assert.That(results.Length, Is.EqualTo(expected.Length));

      Assert.That(results.Select(r => r.Key).Count(a => a < 40), Is.EqualTo(2));
      Assert.That(results.Select(r => r.Key).Count(a => a > 40), Is.EqualTo(1));
    }

    [Test]
    public void ConditionalExprByNullableFieldInGroupBy1()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));
      Assert.That(results.Length, Is.EqualTo(expected.Length));

      Assert.That(results.Select(r => r.Key).Count(a => a < 40), Is.EqualTo(3));
      Assert.That(results.Select(r => r.Key).Count(a => a > 40), Is.EqualTo(1));
    }

    [Test]
    public void ConditionalExprByNullableFieldInGroupBy2()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));
      Assert.That(results.Length, Is.EqualTo(expected.Length));

      Assert.That(results.Select(r => r.Key).Count(a => a < 40), Is.EqualTo(1));
      Assert.That(results.Select(r => r.Key).Count(a => a > 40), Is.EqualTo(3));
    }

    [Test]
    public void ConditionalExprByNullableFieldInGroupBy3()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));
      Assert.That(results.Length, Is.EqualTo(expected.Length));

      Assert.That(results.Select(r => r.Key).Count(a => a < 40), Is.EqualTo(2));
      Assert.That(results.Select(r => r.Key).Count(a => a > 40), Is.EqualTo(2));
    }

    [Test]
    public void ConditionalExprByNullableFieldInGroupBy4()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));
      Assert.That(results.Length, Is.EqualTo(expected.Length));

      Assert.That(results.Select(r => r.Key).Count(a => a < 40), Is.EqualTo(2));
      Assert.That(results.Select(r => r.Key).Count(a => a > 40), Is.EqualTo(2));
    }

    [Test]
    public void ConditionalExprByNullableFieldInGroupBy5()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(3));
      Assert.That(results.Length, Is.EqualTo(expected.Length));

      Assert.That(results.Select(r => r.Key).Count(a => a < 40), Is.EqualTo(2));
      Assert.That(results.Select(r => r.Key).Count(a => a > 40), Is.EqualTo(1));
    }

    [Test]
    public void ConditionalExprByNullableFieldInGroupBy6()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(3));
      Assert.That(results.Length, Is.EqualTo(expected.Length));

      Assert.That(results.Select(r => r.Key).Count(a => a < 40), Is.EqualTo(1));
      Assert.That(results.Select(r => r.Key).Count(a => a > 40), Is.EqualTo(2));
    }

    [Test]
    public void ConditionalExprByEntityInSum()
    {
      var sharedFlow = Session.Query.Single(sharedFlowKey);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
          })
        .Sum(t => t.V2);

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
          })
        .Sum(t => t.V2);

      Assert.That(results, Is.EqualTo(expected));
    }

    [Test]
    public void ConditionalExprByNumberInSum1()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1))
          })
        .Sum(t => t.V2);

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1))
          })
        .Sum(t => t.V2);

      Assert.That(results, Is.EqualTo(expected));
    }

    [Test]
    public void ConditionalExprByNumberInSum2()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1))
          })
        .Sum(t => t.V2);

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1))
          })
        .Sum(t => t.V2);

      Assert.That(results, Is.EqualTo(expected));
    }

    [Test]
    public void ConditionalExprByNullableFieldInSum1()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1))
          })
        .Sum(t => t.V2);

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1))
          })
        .Sum(t => t.V2);

      Assert.That(results, Is.EqualTo(expected));
    }

    [Test]
    public void ConditionalExprByNullableFieldInSum2()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1))
          })
        .Sum(t => t.V2);

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1))
          })
        .Sum(t => t.V2);

      Assert.That(results, Is.EqualTo(expected));
    }

    [Test]
    public void ConditionalExprByNullableFieldInSum3()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1))
          })
        .Sum(t => t.V2);

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1))
          })
        .Sum(t => t.V2);

      Assert.That(results, Is.EqualTo(expected));
    }

    [Test]
    public void ConditionalExprByNullableFieldInSum4()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .Sum(t => t.V2);

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .Sum(t => t.V2);

      Assert.That(results, Is.EqualTo(expected));
    }

    [Test]
    public void ConditionalExprByNullableFieldInSum5()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1))
          })
        .Sum(t => t.V2);

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1))
          })
        .Sum(t => t.V2);

      Assert.That(results, Is.EqualTo(expected));
    }

    [Test]
    public void ConditionalExprByNullableFieldInSum6()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .Sum(t => t.V2);

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (int) (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1))
          })
        .Sum(t => t.V2);

      Assert.That(results, Is.EqualTo(expected));
    }

    [Test]
    public void ConditionalExprByEntityInInclude()
    {
      var sharedFlow = Session.Query.Single(sharedFlowKey);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)).In(40, 70, 72)
          })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
            V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)).In(40, 70, 72)
          })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Count(p => p.V2 == true), Is.EqualTo(2));
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNumberInInclude1()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1)).In(40, 70, 72)
          })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
       .Select(p => new {
         V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1)).In(40, 70, 72)
       })
       .OrderBy(t => t.V2)
       .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Count(p => p.V2 == true), Is.EqualTo(1));
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNumberInInclude2()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1)).In(40, 70, 72) // 100 -> 36
          })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
            V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1)).In(40, 70, 72) // 100 -> 36
          })
        .OrderBy(t => t.V2)
        .ToArray();


      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Count(p => p.V2 == true), Is.EqualTo(1));
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInInclude1()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1)).In(40, 68, 72)
        })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
          V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1)).In(40, 68, 72)
        })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Count(p => p.V2 == true), Is.EqualTo(1));
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInInclude2()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1)).In(40, 70, 72)
        })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
          V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1)).In(40, 70, 72)
        })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Count(p => p.V2 == true), Is.EqualTo(2));
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInInclude3()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1)).In(40, 70, 72)
        })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
          V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1)).In(40, 70, 72)
        })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Count(p => p.V2 == true), Is.EqualTo(1));
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInInclude4()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          V2 = (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1)).In(40, 70, 72)
        })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
          V2 = (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1)).In(40, 70, 72)
        })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Count(p => p.V2 == true), Is.EqualTo(1));
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInInclude5()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1)).In(40, 70, 72)
        })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
          V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1)).In(40, 70, 72)
        })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Count(p => p.V2 == true), Is.EqualTo(1));
      Assert.That(results.SequenceEqual(expected), Is.True);
    }

    [Test]
    public void ConditionalExprByNullableFieldInInclude6()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          V2 = (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1)).In(40, 70, 72)
        })
        .OrderBy(t => t.V2)
        .ToArray();

      var expected = Session.Query.All<PickingProductRequirement>().AsEnumerable()
        .Select(p => new {
          V2 = (p.Quantity.NormalizedValue *
              (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1)).In(40, 70, 72)
        })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(expected.Length, Is.EqualTo(4));

      Assert.That(results.Length, Is.EqualTo(expected.Length));
      Assert.That(results.Count(p => p.V2 == true), Is.EqualTo(1));
      Assert.That(results.SequenceEqual(expected), Is.True);
    }


    //private void PrintExpected<TItem, TValue>(TItem[] items, Func<TItem, TValue> accessor)
    //{
    //  Console.WriteLine();
    //  Console.WriteLine("Expected order of values:");

    //  foreach (var item in items) {
    //    Console.WriteLine(accessor(item));
    //  }
    //}

    private static void CommandExecutingEventHandler(object sender, DbCommandEventArgs e)
    {
      var command = e.Command;
      var commandText = command.CommandText;
      Console.WriteLine("No Modifications SQL Text:");
      Console.WriteLine(commandText);
      var parameters = command.Parameters;

      Console.Write(" Parameters: ");
      for (int i = 0, count = parameters.Count; i < count; i++) {
        var parameter = parameters[0];
        Console.WriteLine($"{parameter.ParameterName} = {parameter.Value.ToString()}");
      }
    }
  }
}


namespace Xtensive.Orm.Tests.Issues.IssueJira0802_PostgreOrderByWithConditionalIssueModel
{
  [HierarchyRoot]
  public class InventoryAction : MesObject
  {
    [Field]
    public LogisticFlow LogisticFlow { get; set; }

    [Field(Length = 50)]
    public string NullableField { get; set; }

    public InventoryAction(Session Session, int id, LogisticFlow logisticFlow, string nullableValue)
      : base(Session, id)
    {
      LogisticFlow = logisticFlow;
      NullableField = nullableValue;
    }
  }

  [HierarchyRoot]
  public class LogisticFlow : MesObject
  {
    public LogisticFlow(Session Session, int id)
      : base(Session, id)
    {
    }
  }

  [HierarchyRoot]
  public class PickingProductRequirement : MesObject
  {
    [Field]
    public DimensionalField Quantity { get; set; }

    [Field]
    public InventoryAction InventoryAction { get; set; }

    public PickingProductRequirement(Session Session, int id)
      : base(Session, id)
    {
    }
  }

  public class DimensionalField : Structure
  {
    [Field]
    public int NormalizedValue { get; private set; }

    public DimensionalField(Session Session, int nValue)
      : base(Session)
    {
      NormalizedValue = nValue;
    }
  }

  public abstract class MesObject : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    protected MesObject(Session Session, int id)
      : base(Session, id)
    {
    }
  }
}