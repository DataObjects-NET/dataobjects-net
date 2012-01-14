// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.21

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0391_OnRemoveActionNone_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0391_OnRemoveActionNone_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Customer : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    [Association(OnTargetRemove = OnRemoveAction.None)]
    public Customer Customer { get; set; }

    public bool HasCustomerKey()
    {
      var field = GetTypeInfo().Fields["Customer"];
      return GetReferenceKey(field)!=null;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0391_OnRemoveActionNone : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Customer).Assembly, typeof (Customer).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var c = new Customer();
          var o = new Order();
          o.Customer = c;
          c.Remove();
          Assert.IsTrue(o.HasCustomerKey());
          
          t.Complete();
        }
      }
    }
  }
}