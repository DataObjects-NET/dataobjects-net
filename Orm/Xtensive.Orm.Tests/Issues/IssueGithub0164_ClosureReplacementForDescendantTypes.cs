// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueGithub0164_ClosureReplacementForDescendantTypesModel;

namespace Xtensive.Orm.Tests.Issues.IssueGithub0164_ClosureReplacementForDescendantTypesModel
{
  [HierarchyRoot]
  public class TestOperation : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public Guid UniqueId { get; set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Deny, PairTo = "Operation")]
    public EntitySet<WorkOrder> WorkOrders { get; private set; }

    public string RootClosureByThis()
    {
      var erpReferences = Session.Query.Execute(q => q.All<WorkOrder>()
        .Where(w => w.Operation == this)
        .Select(w => w.Str));

      return string.Join(" / ", erpReferences.ToArray());
    }

    public string RootClosureByIdField()
    {
      var erpReferences = Session.Query.Execute(q => q.All<WorkOrder>()
        .Where(w => w.Operation.Id == Id)
        .Select(w => w.Str));

      return string.Join(" / ", erpReferences.ToArray());
    }

    public string RootClosureByOtherField()
    {
      var erpReferences = Session.Query.Execute(q => q.All<WorkOrder>()
        .Where(w => w.Operation.UniqueId == UniqueId)
        .Select(w => w.Str));

      return string.Join(" / ", erpReferences.ToArray());
    }

    public TestOperation(Session session, WorkOrder wo)
      : base(session)
    {
      UniqueId = Guid.NewGuid();
      _ = WorkOrders.Add(wo);
    }
  }

  public class ChildOperation : TestOperation
  {
    [Field(Length = 10)]
    public string SomeField { get; set; }

    public string ChildClosureByThis()
    {
      var erpReferences = Session.Query.Execute(q => q.All<WorkOrder>()
        .Where(w => w.Operation == this)
        .Select(w => w.Str));

      return string.Join(" / ", erpReferences.ToArray());
    }

    public string ChildClosureByIdField()
    {
      var erpReferences = Session.Query.Execute(q => q.All<WorkOrder>()
        .Where(w => w.Operation.Id == Id)
        .Select(w => w.Str));

      return string.Join(" / ", erpReferences.ToArray());
    }

    public string ChildClosureByOtherField()
    {
      var erpReferences = Session.Query.Execute(q => q.All<WorkOrder>()
        .Where(w => w.Operation.UniqueId == UniqueId)
        .Select(w => w.Str));

      return string.Join(" / ", erpReferences.ToArray());
    }

    public ChildOperation(Session session, WorkOrder wo)
      : base(session, wo)
    {
    }
  }

  [HierarchyRoot]
  public class WorkOrder : Entity
  {
    [Key, Field]
    public long ID { get; private set; }

    [Field]
    public TestOperation Operation { get; private set; }

    [Field]
    public string Str { get; set; }

    public WorkOrder(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueGithub0164_ClosureReplacementForDescendantTypes : AutoBuildTest
  {
    private Guid[] operationIdentifiers;

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(WorkOrder).Assembly, typeof(WorkOrder).Namespace);
      return config;
    }

    protected override void PopulateData()
    {
      operationIdentifiers = new Guid[6];

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var wo1 = new WorkOrder(session) { Str = "A" };
        var wo2 = new WorkOrder(session) { Str = "B" };
        var wo3 = new WorkOrder(session) { Str = "C" };
        var wo4 = new WorkOrder(session) { Str = "D" };
        var wo5 = new WorkOrder(session) { Str = "E" };
        var wo6 = new WorkOrder(session) { Str = "F" };


        operationIdentifiers[0] = new TestOperation(session, wo1).UniqueId;
        operationIdentifiers[1] = new TestOperation(session, wo2).UniqueId;
        operationIdentifiers[2] = new TestOperation(session, wo3).UniqueId;

        operationIdentifiers[3] = new ChildOperation(session, wo4).UniqueId;
        operationIdentifiers[4] = new ChildOperation(session, wo5).UniqueId;
        operationIdentifiers[5] = new ChildOperation(session, wo6).UniqueId;

        tx.Complete();
      }
    }

    [Test]
    public void ThreeRootSequentialByThis()
    {
      Domain.QueryCache.Clear();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var operation1 = session.Query.All<TestOperation>().First(o => o.UniqueId == operationIdentifiers[0]);
        var operation2 = session.Query.All<TestOperation>().First(o => o.UniqueId == operationIdentifiers[1]);
        var operation3 = session.Query.All<TestOperation>().First(o => o.UniqueId == operationIdentifiers[2]);

        var str1 = operation1.RootClosureByThis();
        var str2 = operation2.RootClosureByThis();
        var str3 = operation3.RootClosureByThis();
        Assert.That(str1 != str2 && str2 != str3 && str3 != str1, Is.True);
      }
    }

    [Test]
    public void ThreeRootSequentialById()
    {
      Domain.QueryCache.Clear();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var operation1 = session.Query.All<TestOperation>().First(o => o.UniqueId == operationIdentifiers[0]);
        var operation2 = session.Query.All<TestOperation>().First(o => o.UniqueId == operationIdentifiers[1]);
        var operation3 = session.Query.All<TestOperation>().First(o => o.UniqueId == operationIdentifiers[2]);

        var str1 = operation1.RootClosureByIdField();
        var str2 = operation2.RootClosureByIdField();
        var str3 = operation3.RootClosureByIdField();

        Assert.That(str1 != str2 && str2 != str3 && str3 != str1, Is.True);
      }
    }

    [Test]
    public void ThreeRootSequentialByOtherField()
    {
      Domain.QueryCache.Clear();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var operation1 = session.Query.All<TestOperation>().First(o => o.UniqueId == operationIdentifiers[0]);
        var operation2 = session.Query.All<TestOperation>().First(o => o.UniqueId == operationIdentifiers[1]);
        var operation3 = session.Query.All<TestOperation>().First(o => o.UniqueId == operationIdentifiers[2]);

        var str1 = operation1.RootClosureByOtherField();
        var str2 = operation2.RootClosureByOtherField();
        var str3 = operation3.RootClosureByOtherField();

        Assert.That(str1 != str2 && str2 != str3 && str3 != str1, Is.True);
      }
    }

    [Test]
    public void ThreeChildrenSequentialByThis()
    {
      Domain.QueryCache.Clear();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var operation1 = session.Query.All<ChildOperation>().First(o => o.UniqueId == operationIdentifiers[3]);
        var operation2 = session.Query.All<ChildOperation>().First(o => o.UniqueId == operationIdentifiers[4]);
        var operation3 = session.Query.All<ChildOperation>().First(o => o.UniqueId == operationIdentifiers[5]);

        var str1 = operation1.ChildClosureByThis();
        var str2 = operation2.ChildClosureByThis();
        var str3 = operation3.ChildClosureByThis();

        Assert.That(str1 != str2 && str2 != str3 && str3 != str1, Is.True);

        str1 = operation1.RootClosureByThis();
        str2 = operation2.RootClosureByThis();
        str3 = operation3.RootClosureByThis();

        Assert.That(str1 != str2 && str2 != str3 && str3 != str1, Is.True);
      }
    }

    [Test]
    public void ThreeChildrenSequentialById()
    {
      Domain.QueryCache.Clear();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var operation1 = session.Query.All<ChildOperation>().First(o => o.UniqueId == operationIdentifiers[3]);
        var operation2 = session.Query.All<ChildOperation>().First(o => o.UniqueId == operationIdentifiers[4]);
        var operation3 = session.Query.All<ChildOperation>().First(o => o.UniqueId == operationIdentifiers[5]);

        var str1 = operation1.ChildClosureByIdField();
        var str2 = operation2.ChildClosureByIdField();
        var str3 = operation3.ChildClosureByIdField();

        Assert.That(str1 != str2 && str2 != str3 && str3 != str1, Is.True);

        str1 = operation1.RootClosureByIdField();
        str2 = operation2.RootClosureByIdField();
        str3 = operation3.RootClosureByIdField();

        Assert.That(str1 != str2 && str2 != str3 && str3 != str1, Is.True);
      }
    }

    [Test]
    public void ThreeChildrenSequentialByOtherField()
    {
      Domain.QueryCache.Clear();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var operation1 = session.Query.All<ChildOperation>().First(o => o.UniqueId == operationIdentifiers[3]);
        var operation2 = session.Query.All<ChildOperation>().First(o => o.UniqueId == operationIdentifiers[4]);
        var operation3 = session.Query.All<ChildOperation>().First(o => o.UniqueId == operationIdentifiers[5]);

        var str1 = operation1.ChildClosureByOtherField();
        var str2 = operation2.ChildClosureByOtherField();
        var str3 = operation3.ChildClosureByOtherField();

        Assert.That(str1 != str2 && str2 != str3 && str3 != str1, Is.True);

        str1 = operation1.RootClosureByOtherField();
        str2 = operation2.RootClosureByOtherField();
        str3 = operation3.RootClosureByOtherField();

        Assert.That(str1 != str2 && str2 != str3 && str3 != str1, Is.True);
      }
    }
  }
}
