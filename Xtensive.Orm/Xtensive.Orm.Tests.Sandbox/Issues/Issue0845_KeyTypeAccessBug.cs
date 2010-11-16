// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2010.10.27

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0845_KeyTypeAccessBug_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0845_KeyTypeAccessBug_Model
{
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  // There must be a descendant to make this test fall
  public class Employee : Person
  {
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0845_KeyTypeAccessBug : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var key = Key.Create<Person>(999); // Key of non-existing entity
        
        Assert.IsNull(Query.SingleOrDefault(key));
        AssertEx.ThrowsInvalidOperationException(() => {
          var type = key.Type;
        });

        tx.Complete();
      }
    }
  }
}