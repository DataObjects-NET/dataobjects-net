// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueGithub0150_ClosureProblemModel;

namespace Xtensive.Orm.Tests.Issues.IssueGithub0150_ClosureProblemModel
{
  [HierarchyRoot]
  public class TestOperation : Entity
  {
    [Key, Field]
    public long ID { get; private set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Deny, PairTo = "Operation")]
    public EntitySet<TestWorkOrder> WorkOrders { get; private set; }

    public string GetErpOrderReference()
    {
      var erpReferences = Session.Query.Execute(q => q.All<TestWorkOrder>()
        .Where(w => w.Operation == this)
        .Select(w => w.Str));

      var result = string.Join(" / ", erpReferences.ToArray());
      return result;
    }

    public TestOperation(TestWorkOrder wo)
    {
      ArgumentNullException.ThrowIfNull(wo);
      _ = WorkOrders.Add(wo);
    }
  }

  [HierarchyRoot]
  public class TestWorkOrder : Entity
  {
    [Key, Field]
    public long ID { get; private set; }

    [Field]
    public TestOperation Operation { get; private set; }

    [Field]
    public string Str { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueGithub0149_ParameterReplacerHandlesNullConstsIncorrectly : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(TestWorkOrder));
      config.Types.Register(typeof(TestOperation));
      return config;
    }

    [Test]
    public void MainTest()
    {
      using(var session = Domain.OpenSession())
      using(var tx = session.OpenTransaction()) {
        var wo = new TestWorkOrder {
          Str = "A"
        };
        var op = new TestOperation(wo);
        var erpOrder = op.GetErpOrderReference();
        Assert.AreEqual(wo.Str, erpOrder);
      }
    }
  }
}
