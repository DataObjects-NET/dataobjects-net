// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.20

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0012_Model;

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0011_MultipleFieldPersistence : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      return config;
    }

    /*
    This code snippet in a transaction executes 3 queries, an INSERT, and
    2 UPDATES. But the third persistance updates Field3 again even though
    it hadn't changed  since the second persistance.
    */
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var e1 = new MyEntity();
          e1.Field3 = 3;
          Session.Current.SaveChanges();
          e1.Field3 = 3;
          Session.Current.SaveChanges();
          e1.Field1 = 1;
          e1.Field2 = 2;
          Session.Current.SaveChanges();
          t.Complete();
        }
      }
    }
  }
}