// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.10.16

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Model;
using Xtensive.Orm.BulkOperations.Tests.Issues.Model;

namespace Xtensive.Orm.BulkOperations.Tests.Issues.Model
{
  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class ServiceMaterial : AbstractBaseForServiceMaterial
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public PreservedService Owner { get; set; }
  }

  [HierarchyRoot]
  public class PreservedService : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    [Association(PairTo = "Owner")]
    public EntitySet<ServiceMaterial> ServiceMaterials { get; set; }
  }

  public abstract class AbstractBaseForServiceMaterial : Entity
  {
    [Field]
    public bool Active { get; set; }
  }
}

namespace Xtensive.Orm.BulkOperations.Tests.Issues
{
  public class IssueJira0560_ChangeFieldWhichDefinedInAncestorClassBug : AutoBuildTest
  {
    [Test]
    public void UpdateTest()
    {
      List<int> keys = new List<int>();
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          var owner = new PreservedService();
          keys.Add(owner.Id);
          new ServiceMaterial { Active = false, Owner = owner };
        }
        transaction.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => session.Query.All<ServiceMaterial>().Where(el => !el.Owner.Id.In(keys)).Set(el => el.Active, false).Update());
      }
    }

    [Test]
    public void InsertTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          ()=>session.Query.Insert(() => new ServiceMaterial { Active = true, Owner = null }));
      }
    }
  }
}
