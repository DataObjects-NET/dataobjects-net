// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.14

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0459_SelfReferencingEntitiesRemoveModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0459_SelfReferencingEntitiesRemoveModel
{
  [HierarchyRoot]
  public class Department : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Department SeniorDepartment { get; set; }
  }

}

namespace Xtensive.Orm.Tests.Issues
{

  public class IssueJira0459_SelfReferencingEntitiesRemove : AutoBuildTest
  {
    private Key selfReferencedInstanceKey;
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Department).Assembly, typeof(Department).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var deparnment = new Department { Name = "Cool Department" };
        deparnment.SeniorDepartment = deparnment;
        selfReferencedInstanceKey = deparnment.Key;
        transaction.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        (from a in session.Query.All<Department>() where a.Key == selfReferencedInstanceKey select a).First().Remove();
        var deletedEntity = (from a in session.Query.All<Department>()
          where a.Key == selfReferencedInstanceKey
          select a).FirstOrDefault();
        transaction.Complete();
        Assert.AreEqual(deletedEntity, null);
      }
    }
  }
}

