// Copyright (C) 2019-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kudelin
// Created:    2019.01.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0615_IncorrectComparisonWithLocalEntityModel;

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueJira0615_IncorrectComparisonWithLocalEntity : AutoBuildTest
  {
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
#if DEBUG
      Session.Events.DbCommandExecuting += CommandExecutingEventHandler;
#endif
    }

    [TearDown]
    public void TearDown()
    {
#if DEBUG
      Session.Events.DbCommandExecuting -= CommandExecutingEventHandler;
#endif
      Transaction?.Dispose();
    }

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

        var nullFlow = new LogisticFlow(session, 999);
        var sharedFlow = new LogisticFlow(session, 1000);
        var uniqueFlow = new LogisticFlow(session, 1100);

        var sharedMultiFieldKeyRef = new MultiFieldKeyEntity(session, 1000, 10);
        var uniqueMultiFieldKeyRef = new MultiFieldKeyEntity(session, 1100, 11);

        _ = new PickingProductRequirement(session, null, 10, true, true) {
          Quantity = new DimensionalField(session, 36),
          InventoryAction = new InventoryAction(session, 100, sharedFlow, nullFlow, true, true),
        };
        _ = new PickingProductRequirement(session, sharedMultiFieldKeyRef, 20, true, true) {
          Quantity = new DimensionalField(session, 35),
          InventoryAction = new InventoryAction(session, 200, sharedFlow, nullFlow, true, true),
        };
        _ = new PickingProductRequirement(session, sharedMultiFieldKeyRef, 30, true, true) {
          Quantity = new DimensionalField(session, 34),
          InventoryAction = new InventoryAction(session, 300, nullFlow, nullFlow, true, true),
        };
        _ = new PickingProductRequirement(session, uniqueMultiFieldKeyRef, 40, true, null) {
          Quantity = new DimensionalField(session, 34),
          InventoryAction = new InventoryAction(session, 400, uniqueFlow, nullFlow, true, null),
        };

        transaction.Complete();
      }
    }

    [Test]
    public void EqualsInWhereByEntityItselfTest1()
    {
      var requirement = Session.Query.All<PickingProductRequirement>().First();

      var results = Session.Query.All<PickingProductRequirement>()
        .Where(x => x == requirement)
        .ToArray();
      Assert.That(results.Length, Is.EqualTo(1));
    }

    [Test]
    public void EqualsInWhereByEntityItselfTest2()
    {
      var requirement = Session.Query.All<PickingProductRequirement>().First();

      var results = Session.Query.All<PickingProductRequirement>()
        .Where(x => x == requirement && x.BooleanFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
    }

    [Test]
    public void EqualsInWhereByEntityItselfTest3()
    {
      var requirement = Session.Query.All<PickingProductRequirement>().First();

      var results = Session.Query.All<PickingProductRequirement>()
        .Where(x => x == requirement && x.NullableBooleanFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
    }

    [Test]
    public void EqualsInWhereByEntityFieldTest1()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var results = Session.Query.All<InventoryAction>()
        .Where(i => i.LogisticFlow == sharedFlow)
        .ToArray();

      Assert.That(results.Length, Is.GreaterThan(0));
      Assert.That(results.All(i => i.LogisticFlow == sharedFlow));
    }

    [Test]
    public void EqualsInWhereByEntityFieldTest2()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var results = Session.Query.All<InventoryAction>()
        .Where(i => i.LogisticFlow == sharedFlow && i.BooleanFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.GreaterThan(0));
      Assert.That(results.All(i => i.LogisticFlow == sharedFlow));
    }

    [Test]
    public void EqualsInWhereByEntityFieldTest3()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var results = Session.Query.All<InventoryAction>()
        .Where(i => i.LogisticFlow == sharedFlow && i.NullableBooleanFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.GreaterThan(0));
      Assert.That(results.All(i => i.LogisticFlow == sharedFlow));
    }

    [Test]
    public void EqualsInWhereByChainOfEntityFieldsTest1()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => p.InventoryAction.LogisticFlow == sharedFlow)
        .ToArray();

      Assert.That(results.Length, Is.GreaterThan(0));
      Assert.That(results.All(p => p.InventoryAction.LogisticFlow == sharedFlow));
    }

    [Test]
    public void EqualsInWhereByChainOfEntityFieldsTest2()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => p.InventoryAction.LogisticFlow == sharedFlow && p.InventoryAction.BooleanFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.GreaterThan(0));
      Assert.That(results.All(p => p.InventoryAction.LogisticFlow == sharedFlow));
    }

    [Test]
    public void EqualsInWhereByChainOfEntityFieldsTest3()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => p.InventoryAction.LogisticFlow == sharedFlow && p.InventoryAction.NullableBooleanFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.GreaterThan(0));
      Assert.That(results.All(p => p.InventoryAction.LogisticFlow == sharedFlow));
    }

    [Test]
    public void EqualsInWhereByEntityFieldRemoteTest1()
    {
      var results = Session.Query.All<InventoryAction>()
        .Where(i => i.LogisticFlow == i.NullFlow)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].LogisticFlow, Is.EqualTo(results[0].NullFlow));
    }

    [Test]
    public void EqualsInWhereByEntityFieldRemoteTest2()
    {
      var results = Session.Query.All<InventoryAction>()
        .Where(i => i.LogisticFlow == i.NullFlow && i.BooleanFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].LogisticFlow, Is.EqualTo(results[0].NullFlow));
    }

    [Test]
    public void EqualsInWhereByEntityFieldRemoteTest3()
    {
      var results = Session.Query.All<InventoryAction>()
        .Where(i => i.LogisticFlow == i.NullFlow && i.NullableBooleanFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].LogisticFlow, Is.EqualTo(results[0].NullFlow));
    }

    [Test]
    public void EqualsInWhereByChainOfEntityFieldRemoteTest1()
    {
      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.EqualTo(results[0].InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInWhereByChainOfEntityFieldRemoteTest2()
    {
      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow && p.InventoryAction.BooleanFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.EqualTo(results[0].InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInWhereByChainOfEntityFieldRemoteTest3()
    {
      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow && p.InventoryAction.NullableBooleanFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.EqualTo(results[0].InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInWhereConditionalTest01()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Where(p => (p.LogisticFlow == sharedFlow ? margin : 1) > 1)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInWhereConditionalTest02()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1) > 1)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInWhereConditionalTest03()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1) > 1)
        .Where(p => (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1) > 40))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInWhereConditionalTest04()
    {
      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Where(p => (p.LogisticFlow == p.NullFlow ? margin : 1) > 1)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].LogisticFlow, Is.EqualTo(results[0].NullFlow));
    }

    [Test]
    public void EqualsInWhereConditionalTest05()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1) > 1)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.EqualTo(results[0].InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInWhereConditionalTest06()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1) > 40))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.EqualTo(results[0].InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInWhereConditionalTest07()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Where(p => (p.LogisticFlow == sharedFlow ? margin : 1).In(1, 3))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInWhereConditionalTest08()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1).In(1, 3))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInWhereConditionalTest09()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)).In(40, 70, 72))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInWhereConditionalTest10()
    {
      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Where(p => (p.LogisticFlow == p.NullFlow ? margin : 1).In(2, 3))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
    }

    [Test]
    public void EqualsInWhereConditionalTest11()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1).In(2, 3))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
    }

    [Test]
    public void EqualsInWhereConditionalTest12()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1)).In(40, 68))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
    }

    [Test]
    public void EqualsInSelectByEntityItselfTest1()
    {
      var requirement = Session.Query.All<PickingProductRequirement>().First();

      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p == requirement
        })
        .Where(x => x.SomeFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].SomeFlag, Is.True);
      Assert.That(results[0].EntityItself, Is.EqualTo(requirement));
    }

    [Test]
    public void EqualsInSelectByEntityItselfTest2()
    {
      var requirement = Session.Query.All<PickingProductRequirement>().First();

      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p == requirement
        })
        .OrderBy(x => x.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.False);
      Assert.That(results[1].SomeFlag, Is.False);
      Assert.That(results[2].SomeFlag, Is.False);
      Assert.That(results[3].SomeFlag, Is.True);
      Assert.That(results[3].EntityItself, Is.EqualTo(requirement));
    }

    [Test]
    public void EqualsInSelectByEntityItselfTest3()
    {
      var requirement = Session.Query.All<PickingProductRequirement>().First();

      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p == requirement
        })
        .GroupBy(x => x.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectByEntityFieldTest1()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = i.LogisticFlow == sharedFlow
        })
        .Where(x => x.SomeFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.GreaterThan(0));
      Assert.That(results.All(p => p.SomeFlag), Is.True);
    }

    [Test]
    public void EqualsInSelectByEntityFieldTest2()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = i.LogisticFlow == sharedFlow
        })
        .OrderBy(x => x.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.False);
      Assert.That(results[1].SomeFlag, Is.False);
      Assert.That(results[2].SomeFlag, Is.True);
      Assert.That(results[3].SomeFlag, Is.True);
      Assert.That(results[2].EntityItself.LogisticFlow, Is.EqualTo(sharedFlow));
      Assert.That(results[3].EntityItself.LogisticFlow, Is.EqualTo(sharedFlow));
    }

    [Test]
    public void EqualsInSelectByEntityFieldTest3()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = i.LogisticFlow == sharedFlow
        })
        .GroupBy(x => x.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectByChainOfEntityFieldsTest1()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.InventoryAction.LogisticFlow == sharedFlow
        })
        .Where(x => x.SomeFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.GreaterThan(0));
      Assert.That(results.All(p => p.SomeFlag), Is.True);
    }

    [Test]
    public void EqualsInSelectByChainOfEntityFieldsTest2()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.InventoryAction.LogisticFlow == sharedFlow
        })
        .OrderBy(x => x.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.False);
      Assert.That(results[1].SomeFlag, Is.False);
      Assert.That(results[2].SomeFlag, Is.True);
      Assert.That(results[3].SomeFlag, Is.True);
      Assert.That(results[2].EntityItself.InventoryAction.LogisticFlow, Is.EqualTo(sharedFlow));
      Assert.That(results[3].EntityItself.InventoryAction.LogisticFlow, Is.EqualTo(sharedFlow));
    }

    [Test]
    public void EqualsInSelectByChainOfEntityFieldsTest3()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.InventoryAction.LogisticFlow == sharedFlow
        })
        .GroupBy(x => x.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectByEntityFieldRemoteTest1()
    {
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = i.LogisticFlow == i.NullFlow
        })
        .Where(x => x.SomeFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].SomeFlag, Is.True);
      Assert.That(results[0].EntityItself.LogisticFlow, Is.EqualTo(results[0].EntityItself.NullFlow));
    }

    [Test]
    public void EqualsInSelectByEntityFieldRemoteTest2()
    {
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = i.LogisticFlow == i.NullFlow
        })
        .OrderBy(x => x.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.False);
      Assert.That(results[1].SomeFlag, Is.False);
      Assert.That(results[2].SomeFlag, Is.False);
      Assert.That(results[3].SomeFlag, Is.True);
      Assert.That(results[3].EntityItself.LogisticFlow, Is.EqualTo(results[3].EntityItself.NullFlow));
    }

    [Test]
    public void EqualsInSelectByEntityFieldRemoteTest3()
    {
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = i.LogisticFlow == i.NullFlow
        })
        .GroupBy(x => x.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectByChainOfEntityFieldRemoteTest1()
    {
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = i.InventoryAction.LogisticFlow == i.InventoryAction.NullFlow
        })
        .Where(x => x.SomeFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].SomeFlag, Is.True);
      Assert.That(results[0].EntityItself.InventoryAction.LogisticFlow,
        Is.EqualTo(results[0].EntityItself.InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInSelectByChainOfEntityFieldRemoteTest2()
    {
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = i.InventoryAction.LogisticFlow == i.InventoryAction.NullFlow
        })
        .OrderBy(x => x.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.False);
      Assert.That(results[1].SomeFlag, Is.False);
      Assert.That(results[2].SomeFlag, Is.False);
      Assert.That(results[3].SomeFlag, Is.True);
      Assert.That(results[3].EntityItself.InventoryAction.LogisticFlow,
        Is.EqualTo(results[3].EntityItself.InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInSelectByChainOfEntityFieldRemoteTest3()
    {
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = i.InventoryAction.LogisticFlow == i.InventoryAction.NullFlow
        })
        .GroupBy(x => x.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectConditionalTest01()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = (i.LogisticFlow == sharedFlow) ? margin : 1
        })
        .Where(p => p.SomeFlag > 1)
        .ToArray();

      Assert.That(results.Length, Is.GreaterThan(0));
      Assert.That(results.Length, Is.LessThan(4));
      Assert.That(results.All(i => i.SomeFlag > 1), Is.True);
    }

    [Test]
    public void EqualsInSelectConditionalTest02()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = (i.LogisticFlow == sharedFlow) ? margin : 1
        })
        .OrderBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.EqualTo(1));
      Assert.That(results[1].SomeFlag, Is.EqualTo(1));
      Assert.That(results[2].SomeFlag, Is.EqualTo(2));
      Assert.That(results[3].SomeFlag, Is.EqualTo(2));
      Assert.That(results[2].EntityItself.LogisticFlow, Is.EqualTo(sharedFlow));
      Assert.That(results[3].EntityItself.LogisticFlow, Is.EqualTo(sharedFlow));
    }

    [Test]
    public void EqualsInSelectConditionalTest03()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = (i.LogisticFlow == sharedFlow) ? margin : 1
        })
        .GroupBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectConditionalTest04()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1
        })
        .Where(p => p.SomeFlag > 1)
        .ToArray();

      Assert.That(results.Length, Is.GreaterThan(0));
      Assert.That(results.Length, Is.LessThan(4));
      Assert.That(results.All(i => i.SomeFlag > 1), Is.True);
    }

    [Test]
    public void EqualsInSelectConditionalTest05()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1
        })
        .OrderBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.EqualTo(1));
      Assert.That(results[1].SomeFlag, Is.EqualTo(1));
      Assert.That(results[2].SomeFlag, Is.EqualTo(2));
      Assert.That(results[3].SomeFlag, Is.EqualTo(2));
      Assert.That(results[3].EntityItself.InventoryAction.LogisticFlow, Is.EqualTo(sharedFlow));
    }

    [Test]
    public void EqualsInSelectConditionalTest06()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1
        })
        .GroupBy(p => p.SomeFlag > 1)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectConditionalTest07()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)
        })
        .Where(p => p.SomeFlag > 40)
        .ToArray();

      Assert.That(results.Length, Is.GreaterThan(0));
      Assert.That(results.Length, Is.LessThan(4));
      Assert.That(results.All(i => i.SomeFlag > 40), Is.True);
    }

    [Test]
    public void EqualsInSelectConditionalTest08()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)
        })
        .OrderBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.LessThan(40));
      Assert.That(results[1].SomeFlag, Is.LessThan(40));
      Assert.That(results[2].SomeFlag, Is.GreaterThan(40));
      Assert.That(results[3].SomeFlag, Is.GreaterThan(40));
    }

    [Test]
    public void EqualsInSelectConditionalTest09()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)
        })
        .GroupBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(3));
    }

    [Test]
    public void EqualsInSelectConditionalTest10()
    {
      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = (i.LogisticFlow == i.NullFlow) ? margin : 1
        })
        .Where(p => p.SomeFlag > 1)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].SomeFlag, Is.EqualTo(2));
      Assert.That(results[0].EntityItself.LogisticFlow, Is.EqualTo(results[0].EntityItself.NullFlow));
    }

    [Test]
    public void EqualsInSelectConditionalTest11()
    {
      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = (i.LogisticFlow == i.NullFlow) ? margin : 1
        })
        .OrderBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.EqualTo(1));
      Assert.That(results[1].SomeFlag, Is.EqualTo(1));
      Assert.That(results[2].SomeFlag, Is.EqualTo(1));
      Assert.That(results[3].SomeFlag, Is.EqualTo(2));
      Assert.That(results[3].EntityItself.LogisticFlow, Is.EqualTo(results[3].EntityItself.NullFlow));
    }

    [Test]
    public void EqualsInSelectConditionalTest12()
    {
      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = (i.LogisticFlow == i.NullFlow) ? margin : 1
        })
        .GroupBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectConditionalTest13()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1
        })
        .Where(p => p.SomeFlag > 1)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].SomeFlag, Is.EqualTo(2));
      Assert.That(results[0].EntityItself.InventoryAction.LogisticFlow,
        Is.EqualTo(results[0].EntityItself.InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInSelectConditionalTest14()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1
        })
        .OrderBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.EqualTo(1));
      Assert.That(results[1].SomeFlag, Is.EqualTo(1));
      Assert.That(results[2].SomeFlag, Is.EqualTo(1));
      Assert.That(results[3].SomeFlag, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectConditionalTest15()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1
        })
        .GroupBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectConditionalTest16()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1)
        })
        .Where(p => p.SomeFlag > 40)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].SomeFlag, Is.EqualTo(68));
      Assert.That(results[0].EntityItself.InventoryAction.LogisticFlow,
        Is.EqualTo(results[0].EntityItself.InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInSelectConditionalTest17()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1)
        })
        .OrderBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.LessThan(40));
      Assert.That(results[1].SomeFlag, Is.LessThan(40));
      Assert.That(results[2].SomeFlag, Is.LessThan(40));
      Assert.That(results[3].SomeFlag, Is.GreaterThan(40));
      Assert.That(results[3].EntityItself.InventoryAction.LogisticFlow,
        Is.EqualTo(results[3].EntityItself.InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInSelectConditionalTest18()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1)
        })
        .GroupBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
    }

    [Test]
    public void EqualsInSelectConditionalTest19()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = ((i.LogisticFlow == sharedFlow) ? margin : 1).In(1, 3)
        })
        .Where(p => p.SomeFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectConditionalTest20()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = ((i.LogisticFlow == sharedFlow) ? margin : 1).In(1, 3)
        })
        .OrderBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.False);
      Assert.That(results[1].SomeFlag, Is.False);
      Assert.That(results[2].SomeFlag, Is.True);
      Assert.That(results[3].SomeFlag, Is.True);
      Assert.That(results[2].EntityItself.LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[3].EntityItself.LogisticFlow, Is.Not.EqualTo(sharedFlow));
    }

    [Test]
    public void EqualsInSelectConditionalTest21()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = ((i.LogisticFlow == sharedFlow) ? margin : 1).In(1, 3)
        })
        .GroupBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectConditionalTest22()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1).In(1, 3)
        })
        .Where(p => p.SomeFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectConditionalTest23()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1).In(1, 3)
        })
        .OrderBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.False);
      Assert.That(results[1].SomeFlag, Is.False);
      Assert.That(results[2].SomeFlag, Is.True);
      Assert.That(results[3].SomeFlag, Is.True);
      Assert.That(results[2].EntityItself.InventoryAction.LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[3].EntityItself.InventoryAction.LogisticFlow, Is.Not.EqualTo(sharedFlow));
    }

    [Test]
    public void EqualsInSelectConditionalTest24()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1).In(1, 3)
        })
        .GroupBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectConditionalTest25()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)).In(40, 70, 72)
        })
        .Where(p => p.SomeFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectConditionalTest26()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)).In(40, 70, 72)
        })
        .OrderBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.False);
      Assert.That(results[1].SomeFlag, Is.False);
      Assert.That(results[2].SomeFlag, Is.True);
      Assert.That(results[3].SomeFlag, Is.True);
    }

    [Test]
    public void EqualsInSelectConditionalTest27()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)).In(40, 70, 72)
        })
        .GroupBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectConditionalTest28()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = ((i.LogisticFlow == i.NullFlow) ? margin : 1).In(2, 3)
        })
        .Where(p => p.SomeFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].SomeFlag, Is.True);
      Assert.That(results[0].EntityItself.LogisticFlow, Is.EqualTo(results[0].EntityItself.NullFlow));
    }

    [Test]
    public void EqualsInSelectConditionalTest29()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = ((i.LogisticFlow == i.NullFlow) ? margin : 1).In(2, 3)
        })
        .OrderBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.False);
      Assert.That(results[1].SomeFlag, Is.False);
      Assert.That(results[2].SomeFlag, Is.False);
      Assert.That(results[3].SomeFlag, Is.True);
    }

    [Test]
    public void EqualsInSelectConditionalTest30()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .Select(i => new {
          EntityItself = i,
          SomeFlag = ((i.LogisticFlow == i.NullFlow) ? margin : 1).In(2, 3)
        })
        .GroupBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectConditionalTest31()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1).In(2, 3)
        })
        .Where(p => p.SomeFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].SomeFlag, Is.True);
      Assert.That(results[0].EntityItself.InventoryAction.LogisticFlow,
        Is.EqualTo(results[0].EntityItself.InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInSelectConditionalTest32()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1).In(2, 3)
        })
        .OrderBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.False);
      Assert.That(results[1].SomeFlag, Is.False);
      Assert.That(results[2].SomeFlag, Is.False);
      Assert.That(results[3].SomeFlag, Is.True);
    }

    [Test]
    public void EqualsInSelectConditionalTest33()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1).In(2, 3)
        })
        .GroupBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInSelectConditionalTest34()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1)).In(30, 32, 68)
        })
        .Where(p => p.SomeFlag == true)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(1));
      Assert.That(results[0].SomeFlag, Is.True);
      Assert.That(results[0].EntityItself.InventoryAction.LogisticFlow,
        Is.EqualTo(results[0].EntityItself.InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInSelectConditionalTest35()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1)).In(34, 36)
        })
        .OrderBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].SomeFlag, Is.False);
      Assert.That(results[1].SomeFlag, Is.False);
      Assert.That(results[2].SomeFlag, Is.True);
      Assert.That(results[3].SomeFlag, Is.True);
    }

    [Test]
    public void EqualsInSelectConditionalTest36()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(p => new {
          EntityItself = p,
          SomeFlag = (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1)).In(34, 36)
        })
        .GroupBy(p => p.SomeFlag)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInOrderByEntityItselfTest()
    {
      var requirement = Session.Query.All<PickingProductRequirement>().First();

      var results = Session.Query.All<PickingProductRequirement>()
        .OrderBy(x => x == requirement)
        .ToArray();
    }

    [Test]
    public void EqualsInOrderByEntityFieldTest()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .OrderBy(p => p.LogisticFlow == sharedFlow)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[1].LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[2].LogisticFlow, Is.EqualTo(sharedFlow));
      Assert.That(results[3].LogisticFlow, Is.EqualTo(sharedFlow));
    }

    [Test]
    public void EqualsInOrderByChainOfFieldsTest()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .OrderBy(p => p.InventoryAction.LogisticFlow == sharedFlow)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[1].InventoryAction.LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[2].InventoryAction.LogisticFlow, Is.EqualTo(sharedFlow));
      Assert.That(results[3].InventoryAction.LogisticFlow, Is.EqualTo(sharedFlow));
    }

    [Test]
    public void EqualsInOrderByEntityFieldAndRemoteEntityTest()
    {
      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .OrderBy(p => p.LogisticFlow == p.NullFlow)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].LogisticFlow, Is.Not.EqualTo(results[0].NullFlow));
      Assert.That(results[1].LogisticFlow, Is.Not.EqualTo(results[1].NullFlow));
      Assert.That(results[2].LogisticFlow, Is.Not.EqualTo(results[2].NullFlow));
      Assert.That(results[3].LogisticFlow, Is.EqualTo(results[3].NullFlow));
    }

    [Test]
    public void EqualsInOrderByChainOfFieldsAndRemoteEntityTest()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .OrderBy(p => p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[0].InventoryAction.NullFlow));
      Assert.That(results[1].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[1].InventoryAction.NullFlow));
      Assert.That(results[2].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[2].InventoryAction.NullFlow));
      Assert.That(results[3].InventoryAction.LogisticFlow, Is.EqualTo(results[3].InventoryAction.NullFlow));
    }

    [Test]
    public void EquaInOrderByConditionalTest01()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .OrderBy(p => (p.LogisticFlow == sharedFlow ? margin : 1))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[1].LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[2].LogisticFlow, Is.EqualTo(sharedFlow));
      Assert.That(results[3].LogisticFlow, Is.EqualTo(sharedFlow));
    }

    [Test]
    public void EqualsInOrderByConditionalTest02()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .OrderBy(p => (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[1].InventoryAction.LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[2].InventoryAction.LogisticFlow, Is.EqualTo(sharedFlow));
      Assert.That(results[3].InventoryAction.LogisticFlow, Is.EqualTo(sharedFlow));
    }

    [Test]
    public void EqualsInOrderByConditionalTest03()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .OrderBy(p => p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[1].InventoryAction.LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[2].InventoryAction.LogisticFlow, Is.EqualTo(sharedFlow));
      Assert.That(results[3].InventoryAction.LogisticFlow, Is.EqualTo(sharedFlow));
    }

    [Test]
    public void EqualsInOrderByConditionalTest04()
    {
      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .OrderBy(p => (p.LogisticFlow == p.NullFlow ? margin : 1))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].LogisticFlow, Is.Not.EqualTo(results[0].NullFlow));
      Assert.That(results[1].LogisticFlow, Is.Not.EqualTo(results[1].NullFlow));
      Assert.That(results[2].LogisticFlow, Is.Not.EqualTo(results[2].NullFlow));
      Assert.That(results[3].LogisticFlow, Is.EqualTo(results[3].NullFlow));
    }

    [Test]
    public void EqualsInOrderByConditionalTest05()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .OrderBy(p => (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[0].InventoryAction.NullFlow));
      Assert.That(results[1].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[1].InventoryAction.NullFlow));
      Assert.That(results[2].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[2].InventoryAction.NullFlow));
      Assert.That(results[3].InventoryAction.LogisticFlow, Is.EqualTo(results[3].InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInOrderByConditionalTest06()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .OrderBy(p => p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[0].InventoryAction.NullFlow));
      Assert.That(results[1].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[1].InventoryAction.NullFlow));
      Assert.That(results[2].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[2].InventoryAction.NullFlow));
      Assert.That(results[3].InventoryAction.LogisticFlow, Is.EqualTo(results[3].InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInOrderByConditionalTest07()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().FirstOrDefault(i => i.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .OrderBy(p => (p.LogisticFlow == sharedFlow ? margin : 1).In(1, 3))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].LogisticFlow, Is.EqualTo(sharedFlow));
      Assert.That(results[1].LogisticFlow, Is.EqualTo(sharedFlow));
      Assert.That(results[2].LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[3].LogisticFlow, Is.Not.EqualTo(sharedFlow));
    }

    [Test]
    public void EqualsInOrderByConditionalTest08()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().FirstOrDefault(i => i.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .OrderBy(p => (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1).In(1, 3))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.EqualTo(sharedFlow));
      Assert.That(results[1].InventoryAction.LogisticFlow, Is.EqualTo(sharedFlow));
      Assert.That(results[2].InventoryAction.LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[3].InventoryAction.LogisticFlow, Is.Not.EqualTo(sharedFlow));
    }

    [Test]
    public void EqualsInOrderByConditionalTest09()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().FirstOrDefault(i => i.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .OrderBy(p => (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)).In(40, 70, 72))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[1].InventoryAction.LogisticFlow, Is.Not.EqualTo(sharedFlow));
      Assert.That(results[2].InventoryAction.LogisticFlow, Is.EqualTo(sharedFlow));
      Assert.That(results[3].InventoryAction.LogisticFlow, Is.EqualTo(sharedFlow));
    }

    [Test]
    public void EqualsInOrderByConditionalTest10()
    {
      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .OrderBy(p => (p.LogisticFlow == p.NullFlow ? margin : 1).In(2, 3))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].LogisticFlow, Is.Not.EqualTo(results[0].NullFlow));
      Assert.That(results[1].LogisticFlow, Is.Not.EqualTo(results[1].NullFlow));
      Assert.That(results[2].LogisticFlow, Is.Not.EqualTo(results[2].NullFlow));
      Assert.That(results[3].LogisticFlow, Is.EqualTo(results[3].NullFlow));
    }

    [Test]
    public void EqualsInOrderByConditionalTest11()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .OrderBy(p => (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1).In(2, 3))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[0].InventoryAction.NullFlow));
      Assert.That(results[1].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[1].InventoryAction.NullFlow));
      Assert.That(results[2].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[2].InventoryAction.NullFlow));
      Assert.That(results[3].InventoryAction.LogisticFlow, Is.EqualTo(results[3].InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInOrderByConditionalTest12()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .OrderBy(p => (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1)).In(40, 41, 68))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results[0].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[0].InventoryAction.NullFlow));
      Assert.That(results[1].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[1].InventoryAction.NullFlow));
      Assert.That(results[2].InventoryAction.LogisticFlow, Is.Not.EqualTo(results[2].InventoryAction.NullFlow));
      Assert.That(results[3].InventoryAction.LogisticFlow, Is.EqualTo(results[3].InventoryAction.NullFlow));
    }

    [Test]
    public void EqualsInGroupByEntityItselfTest()
    {
      var requirement = Session.Query.All<PickingProductRequirement>().First();

      var results = Session.Query.All<PickingProductRequirement>()
        .GroupBy(x => x == requirement)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInGroupByEntityFieldTest()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .GroupBy(p => p.LogisticFlow == sharedFlow)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInGroupByChainOfFieldsTest()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .GroupBy(p => p.InventoryAction.LogisticFlow == sharedFlow)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInGroupByEntityFieldAndRemoteEntityTest()
    {
      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .GroupBy(p => p.LogisticFlow == p.NullFlow)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInGroupByChainOfFieldsAndRemoteEntityTest()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .GroupBy(p => p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EquaInGroupByConditionalTest01()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .GroupBy(p => (p.LogisticFlow == sharedFlow ? margin : 1))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInGroupByConditionalTest02()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .GroupBy(p => (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInGroupByConditionalTest03()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(l => l.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .GroupBy(p => p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(3));
    }

    [Test]
    public void EqualsInGroupByConditionalTest04()
    {
      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .GroupBy(p => (p.LogisticFlow == p.NullFlow ? margin : 1))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInGroupByConditionalTest05()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .GroupBy(p => (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInGroupByConditionalTest06()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .GroupBy(p => p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
    }

    [Test]
    public void EqualsInGroupByConditionalTest07()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().FirstOrDefault(i => i.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .GroupBy(p => (p.LogisticFlow == sharedFlow ? margin : 1).In(1, 3))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInGroupByConditionalTest08()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().FirstOrDefault(i => i.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .GroupBy(p => (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1).In(1, 3))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInGroupByConditionalTest09()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().FirstOrDefault(i => i.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .GroupBy(p => (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == sharedFlow ? margin : 1)).In(40, 70, 72))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInGroupByConditionalTest10()
    {
      var margin = 2;
      var results = Session.Query.All<InventoryAction>()
        .GroupBy(p => (p.LogisticFlow == p.NullFlow ? margin : 1).In(2, 3))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInGroupByConditionalTest11()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .GroupBy(p => (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1).In(2, 3))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsInGroupByConditionalTest12()
    {
      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .GroupBy(p => (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow == p.InventoryAction.NullFlow ? margin : 1)).In(40, 41, 68))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
    }

    [Test]
    public void EqualsByEntityKey01()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(f => f.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(
          p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow.Key == sharedFlow.Key ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
      Assert.That(results.All(a => a.V2 > 40), Is.True);
    }

    [Test]
    public void EqualsByEntityKey02()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(f => f.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(
          p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow.Key == sharedFlow.Key ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results.Take(2).All(a => a.V2 < 40), Is.True);
      Assert.That(results.Skip(2).All(a => a.V2 > 40), Is.True);
    }

    [Test]
    public void EqualsByEntityKey03()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(f => f.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(
          p => new {
            V2 = (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow.Key == sharedFlow.Key ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(3));
    }

    [Test]
    public void EqualsByEntityKey04()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(f => f.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow.Key == sharedFlow.Key ? margin : 1)) > 40)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
      Assert.That(results.All(a => a.InventoryAction.LogisticFlow == sharedFlow), Is.True);
    }

    [Test]
    public void EqualsByEntityKey05()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(f => f.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .OrderBy(p => (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow.Key == sharedFlow.Key ? margin : 1)))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results.Take(2).Any(a => a.InventoryAction.LogisticFlow == sharedFlow), Is.False);
      Assert.That(results.Skip(2).All(a => a.InventoryAction.LogisticFlow == sharedFlow), Is.True);
    }

    [Test]
    public void EqualsByEntityKey06()
    {
      var sharedFlow = Session.Query.All<LogisticFlow>().First(f => f.Id == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .GroupBy(p => (p.Quantity.NormalizedValue * (p.InventoryAction.LogisticFlow.Key == sharedFlow.Key ? margin : 1)))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(3));
    }

    [Test]
    public void EqualsByEntityMultiFieldKey01()
    {
      var sharedEntity = Session.Query.All<MultiFieldKeyEntity>().First(f => f.Id1 == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(
          p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.MultiFieldKeyRef.Key == sharedEntity.Key ? margin : 1))
          })
        .Where(t => t.V2 > 40)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
      Assert.That(results.All(a => a.V2 > 40), Is.True);
    }

    [Test]
    public void EqualsByEntityMultiFieldKey02()
    {
      var sharedEntity = Session.Query.All<MultiFieldKeyEntity>().First(f => f.Id1 == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(
          p => new {
            V2 = (int) (p.Quantity.NormalizedValue * (p.MultiFieldKeyRef.Key == sharedEntity.Key ? margin : 1))
          })
        .OrderBy(t => t.V2)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results.Take(2).All(a => a.V2 < 40), Is.True);
      Assert.That(results.Skip(2).All(a => a.V2 > 40), Is.True);
    }

    [Test]
    public void EqualsByEntityMultifieldKey03()
    {
      var sharedEntity = Session.Query.All<MultiFieldKeyEntity>().First(f => f.Id1 == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Select(
          p => new {
            V2 = (p.Quantity.NormalizedValue * (p.MultiFieldKeyRef.Key == sharedEntity.Key ? margin : 1))
          })
        .GroupBy(t => t.V2)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
    }

    [Test]
    public void EqualsByEntityMultiFieldKey04()
    {
      var sharedEntity = Session.Query.All<MultiFieldKeyEntity>().First(f => f.Id1 == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .Where(p => (p.Quantity.NormalizedValue * (p.MultiFieldKeyRef.Key == sharedEntity.Key ? margin : 1)) > 40)
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(2));
      Assert.That(results.All(a => a.MultiFieldKeyRef == sharedEntity), Is.True);
    }

    [Test]
    public void EqualsByEntityMultiFieldKey05()
    {
      var sharedEntity = Session.Query.All<MultiFieldKeyEntity>().First(f => f.Id1 == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .OrderBy(p => p.Quantity.NormalizedValue * (p.MultiFieldKeyRef.Key == sharedEntity.Key ? margin : 1))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
      Assert.That(results.Take(2).Any(p => p.MultiFieldKeyRef == sharedEntity), Is.False);
      Assert.That(results.Skip(2).All(p => p.MultiFieldKeyRef == sharedEntity), Is.True);
    }

    [Test]
    public void EqualsByEntityMultifieldKey06()
    {
      var sharedEntity = Session.Query.All<MultiFieldKeyEntity>().First(f => f.Id1 == 1000);

      var margin = 2;
      var results = Session.Query.All<PickingProductRequirement>()
        .GroupBy(p => p.Quantity.NormalizedValue * (p.MultiFieldKeyRef.Key == sharedEntity.Key ? margin : 1))
        .ToArray();

      Assert.That(results.Length, Is.EqualTo(4));
    }
#if DEBUG

    private static void CommandExecutingEventHandler(object sender, DbCommandEventArgs e)
    {
      var command = e.Command;
      var commandText = command.CommandText;
      Console.WriteLine("No Modifications SQL Text:");
      Console.WriteLine(commandText);
      var parameters = command.Parameters;

      Console.Write(" Parameters: ");
      for (int i = 0, count = parameters.Count; i < count; i++) {
        var parameter = parameters[i];
        Console.WriteLine($"{parameter.ParameterName} = {parameter.Value}");
      }
    }
#endif
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0615_IncorrectComparisonWithLocalEntityModel
{
  [HierarchyRoot]
  public class InventoryAction : MesObject
  {
    [Field]
    public LogisticFlow LogisticFlow { get; set; }

    [Field]
    public LogisticFlow NullFlow { get; set; }

    [Field]
    public bool BooleanFlag { get; set; }

    [Field]
    public bool? NullableBooleanFlag { get; set; }

    public InventoryAction(Session session, int id, 
      LogisticFlow logisticFlow, LogisticFlow nullFlow, bool boolFlag, bool? nullableBoolFlag)
      : base(session, id)
    {
      LogisticFlow = logisticFlow;
      NullFlow = nullFlow;
      BooleanFlag = boolFlag;
      NullableBooleanFlag = nullableBoolFlag;
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
  public class PickingProductRequirement : MesObject
  {
    [Field]
    public DimensionalField Quantity { get; set; }

    [Field]
    public InventoryAction InventoryAction { get; set; }

    [Field]
    public bool BooleanFlag { get; set; }

    [Field]
    public bool? NullableBooleanFlag { get; set; }

    [Field]
    public MultiFieldKeyEntity MultiFieldKeyRef { get; set; }

    public PickingProductRequirement(Session session,
      MultiFieldKeyEntity multiFieldKeyEntity, int id, bool boolFlag, bool? nullableBoolFlag)
       : base(session, id)
    {
      BooleanFlag = boolFlag;
      NullableBooleanFlag = nullableBoolFlag;
      MultiFieldKeyRef = multiFieldKeyEntity;
    }
  }

  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class MultiFieldKeyEntity : Entity
  {
    [Field, Key(0)]
    public int Id1 { get; private set; }

    [Field, Key(1)]
    public int Id2 { get; private set; }

    public MultiFieldKeyEntity(Session session, int id1, int id2)
      : base(session, id1, id2)
    {
    }
  }

  public class DimensionalField : Structure
  {
    [Field]
    public int NormalizedValue { get; private set; }

    public DimensionalField(Session session, int nValue)
      : base(session)
    {
      NormalizedValue = nValue;
    }
  }

  public abstract class MesObject : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

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
