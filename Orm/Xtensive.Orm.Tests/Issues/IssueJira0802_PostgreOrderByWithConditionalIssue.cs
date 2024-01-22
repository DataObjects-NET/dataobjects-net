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
        var uniqueFlow = new LogisticFlow(session, 1100);

        _ = new PickingProductRequirement(session, 10) {
          Quantity = new DimensionalField(session, 36),
          InventoryAction = new InventoryAction(session, 100) {
            LogisticFlow = sharedFlow,
            NullableField = "a"
          }
        };

        _ = new PickingProductRequirement(session, 20) {
          Quantity = new DimensionalField(session, 35),
          InventoryAction = new InventoryAction(session, 200) {
            LogisticFlow = sharedFlow,
            NullableField = "b"
          }
        };

        _ = new PickingProductRequirement(session, 30) {
          Quantity = new DimensionalField(session, 34),
          InventoryAction = new InventoryAction(session, 300) {
            LogisticFlow = sharedFlow,
            NullableField = "a"
          }
        };

        _ = new PickingProductRequirement(session, 40) {
          Quantity = new DimensionalField(session, 34),
          InventoryAction = new InventoryAction(session, 400) {
            LogisticFlow = uniqueFlow,
            NullableField = null
          }
        };

        transaction.Complete();
      }
    }


    [Test]
    public void ComplexConditionalByEntityInOrderBy()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var sharedFlow = session.Query.All<LogisticFlow>().First(f => f.ID == 1000);

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(1).All(a => a.V2 < 40), Is.False);
        Assert.That(results.Skip(1).All(a => a.V2 > 40 && a.V2 < 75), Is.True);
        Assert.That(results[0].V2, Is.LessThan(40));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void ComplexConditionalByNumberInOrderBy1()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(1).All(a => a.V2 < 40), Is.False);
        Assert.That(results.Skip(1).All(a => a.V2 > 40 && a.V2 < 75), Is.True);
        Assert.That(results[0].V2, Is.LessThan(40));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void ComplexConditionalByNumberInOrderBy2()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(3).All(a => a.V2 < 40), Is.True);
        Assert.That(results.Skip(3).All(a => a.V2 > 40 && a.V2 < 75), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }


    [Test]
    public void ComplexConditionalByNullableFieldInOrderBy1()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(3).All(a => a.V2 < 40), Is.True);
        Assert.That(results[3].V2, Is.GreaterThan(40));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void ComplexConditionalByNullableFieldInOrderBy2()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(1).All(a => a.V2 > 40), Is.True);
        Assert.That(results[0].V2, Is.LessThan(40));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void ComplexConditionalByNullableFieldInOrderBy3()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(2).All(a => a.V2 < 40), Is.True);
        Assert.That(results.Skip(2).All(a => a.V2 > 40 && a.V2 < 75), Is.True);
        Assert.That(results[0].V2, Is.LessThan(40));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void ComplexConditionalByNullableFieldInOrderBy4()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue *
                (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue *
                (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(2).All(a => a.V2 < 40), Is.True);
        Assert.That(results.Skip(2).All(a => a.V2 > 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void ComplexConditionalByNullableFieldInOrderBy5()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(3).All(a => a.V2 < 40), Is.True);
        Assert.That(results.Skip(3).All(a => a.V2 > 40 && a.V2 < 75), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void ComplexConditionalByNullableFieldInOrderBy6()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue *
                (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue *
                (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1))
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(1).All(a => a.V2 < 40), Is.True);
        Assert.That(results.Skip(1).All(a => a.V2 > 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }


    [Test]
    public void ComplexConditionalByEntityInOrderByDesc()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var sharedFlow = session.Query.All<LogisticFlow>().First(f => f.ID == 1000);

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(3).All(a => a.V2 < 40), Is.True);
        Assert.That(results.Take(3).All(a => a.V2 > 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void ComplexConditionalByNumberInOrderByDesc2()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID > 100 ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(3).All(a => a.V2 < 40), Is.True);
        Assert.That(results.Take(3).All(a => a.V2 > 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void ComplexConditionalByNumberInOrderByDesc3()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.ID == 100 ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(1).All(a => a.V2 < 40), Is.True);
        Assert.That(results.Take(1).All(a => a.V2 > 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }


    [Test]
    public void ComplexConditionalByNullableFieldInOrderByDesc1()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == null ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(1).All(a => a.V2 > 40), Is.True);
        Assert.That(results.Skip(1).All(a => a.V2 < 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void ComplexConditionalByNullableFieldInOrderByDesc2()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField != null ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(3).All(a => a.V2 > 40), Is.True);
        Assert.That(results.Skip(3).All(a => a.V2 < 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void ComplexConditionalByNullableFieldInOrderByDesc3()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "a" ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(2).All(a => a.V2 < 40), Is.True);
        Assert.That(results.Take(2).All(a => a.V2 > 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void ComplexConditionalByNullableFieldInOrderByDesc4()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue *
                (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue *
                (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(2).All(a => a.V2 > 40), Is.True);
        Assert.That(results.Skip(2).All(a => a.V2 < 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void ComplexConditionalByNullableFieldInOrderByDesc5()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.NullableField == "b" ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(1).All(a => a.V2 > 40), Is.True);
        Assert.That(results.Skip(1).All(a => a.V2 < 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void ComplexConditionalByNullableFieldInOrderByDesc6()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue *
                (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.Quantity.NormalizedValue *
                (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1))
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(3).All(a => a.V2 < 40), Is.True);
        Assert.That(results.Take(3).All(a => a.V2 > 40), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }


    [Test]
    public void SimpleConditionalByEntityInOrderBy()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var sharedFlow = session.Query.All<LogisticFlow>().First(f => f.ID == 1000);

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(1).All(a => a.V2 == 2), Is.True);
        Assert.That(results.Take(1).All(a => a.V2 == 1), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void SimpleConditionalByNumberInOrderBy1()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.ID > 100 ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.ID > 100 ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(1).All(a => a.V2 == 2), Is.True);
        Assert.That(results.Take(1).All(a => a.V2 == 1), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void SimpleConditionalByNumberInOrderBy2()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.ID == 100 ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.ID == 100 ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(3).All(a => a.V2 == 1), Is.True);
        Assert.That(results[3].V2, Is.EqualTo(2));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }



    [Test]
    public void SimpleConditionalWithNullableFieldInOrderBy1()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField == null ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField == null ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t)=>t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(3).All(a => a.V2 == 1), Is.True);
        Assert.That(results.Skip(3).All(a => a.V2 == 2), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void SimpleConditionalWithNullableFieldInOrderBy2()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField != null ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField != null ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(1).All(a => a.V2 == 1), Is.True);
        Assert.That(results.Skip(1).All(a => a.V2 == 2), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void SimpleConditionalWithNullableFieldInOrderBy3()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField == "a" ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField == "a" ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(2).All(a => a.V2 == 2), Is.True);
        Assert.That(results.Take(2).All(a => a.V2 == 1), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void SimpleConditionalWithNullableFieldInOrderBy4()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(2).All(a => a.V2 == 2), Is.True);
        Assert.That(results.Take(2).All(a => a.V2 == 1), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void SimpleConditionalWithNullableFieldInOrderBy5()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField == "b" ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField == "b" ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(3).All(a => a.V2 == 1), Is.True);
        Assert.That(results.Skip(3).All(a => a.V2 == 2), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void SimpleConditionalWithNullableFieldInOrderBy6()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1)
            })
          .OrderBy(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(1).All(a => a.V2 == 2), Is.True);
        Assert.That(results.Take(1).All(a => a.V2 == 1), Is.True);
        Assert.That(results[0].V2, Is.EqualTo(1));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }



    [Test]
    public void SimpleConditionalByEntityInOrderByDesc()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var sharedFlow = session.Query.All<LogisticFlow>().First(f => f.ID == 1000);

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(3).All(a => a.V2 == 2), Is.True);
        Assert.That(results[3].V2, Is.EqualTo(1));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void SimpleConditionalByNumberInOrderByDesc1()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.ID > 100 ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.ID > 100 ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(3).All(a => a.V2 == 2), Is.True);
        Assert.That(results[3].V2, Is.EqualTo(1));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void SimpleConditionalByNumberInOrderByDesc2()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.ID == 100 ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.ID == 100 ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(1).All(a => a.V2 == 1), Is.True);
        Assert.That(results[0].V2, Is.EqualTo(2));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }




    [Test]
    public void SimpleConditionalByNullableFieldInOrderByDesc1()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField == null ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField == null ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(1).All(a => a.V2 == 1), Is.True);
        Assert.That(results[0].V2, Is.EqualTo(2));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void SimpleConditionalByNullableFieldInOrderByDesc2()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField != null ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField != null ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(3).All(a => a.V2 == 2), Is.True);
        Assert.That(results[3].V2, Is.EqualTo(1));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void SimpleConditionalByNullableFieldInOrderByDesc3()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField == "a" ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField == "a" ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(2).All(a => a.V2 == 2), Is.True);
        Assert.That(results.Skip(2).All(a => a.V2 == 1), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void SimpleConditionalByNullableFieldInOrderByDesc4()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField != "a" || p.InventoryAction.NullableField == null ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(2).All(a => a.V2 == 2), Is.True);
        Assert.That(results.Skip(2).All(a => a.V2 == 1), Is.True);
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void SimpleConditionalByNullableFieldInOrderByDesc5()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField == "b" ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField == "b" ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Skip(1).All(a => a.V2 == 1), Is.True);
        Assert.That(results[0].V2, Is.EqualTo(2));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }

    [Test]
    public void SimpleConditionalByNullableFieldInOrderByDesc6()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += CommandExecutingEventHandler;
        var margin = 2;
        var results = session.Query.All<PickingProductRequirement>()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();
        session.Events.DbCommandExecuting -= CommandExecutingEventHandler;

        var expected = session.Query.All<PickingProductRequirement>()
          .AsEnumerable()
          .Select(
            p => new {
              V2 = (int) (p.InventoryAction.NullableField != "b" || p.InventoryAction.NullableField == null ? margin : 1)
            })
          .OrderByDescending(t => t.V2)
          .ToArray();

        Assert.That(expected.Length, Is.EqualTo(4));
        PrintExpected(expected, (t) => t.V2);

        Assert.That(results.Length, Is.EqualTo(expected.Length));
        Assert.That(results.Take(3).All(a => a.V2 == 2), Is.True);
        Assert.That(results[3].V2, Is.EqualTo(1));
        Assert.That(results.SequenceEqual(expected), Is.True);
      }
    }


    private void PrintExpected<T>(T[] items, Func<T, int> accessor)
    {
      Console.WriteLine();
      Console.WriteLine("Expected order of values:");

      foreach (var item in items) {
        Console.WriteLine(accessor(item));
      }
    }


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
    public Product Product { get; set; }

    [Field]
    public LogisticFlow LogisticFlow { get; set; }

    [Field(Length = 50)]
    public string NullableField { get; set; }

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