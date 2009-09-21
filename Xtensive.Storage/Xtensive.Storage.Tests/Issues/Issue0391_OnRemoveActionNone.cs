// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.21

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Issues.Issue0391_OnRemoveActionNone_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0391_OnRemoveActionNone_Model
{
  [HierarchyRoot]
  public class Customer : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

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

namespace Xtensive.Storage.Tests.Issues
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
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {

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