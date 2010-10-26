// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.20

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0013_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0013_Model
{
  [Serializable]
  [HierarchyRoot]
  public class SqlTaskEntity : Entity
  {
    [Field, Key]
    public long ID { get; private set; }

    [Field]
    public int Field1 { get; set; }

    [Field]
    public int Field2 { get; set; }

    [Field]
    public int Field3 { get; set; }

    [Field]
    public int Field4 { get; set; }

    [Field]
    public int Field5 { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0013_SqlBuilderRequestTaskBug : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (SqlTaskEntity).Assembly, typeof (SqlTaskEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        SqlTaskEntity e1;
        using (TransactionScope trs = session.OpenTransaction()) {
          e1 = new SqlTaskEntity();
          //insert
          Session.Current.SaveChanges();
          e1.Field5 = 5;
          //update
          trs.Complete();
        }
        using (TransactionScope trs = session.OpenTransaction()) {
          e1.Field1 = 1;
          e1.Field3 = 3;
          //update
          trs.Complete();
        }
        using (TransactionScope trs = session.OpenTransaction()) {
          Assert.AreEqual(1, e1.Field1);
          Assert.AreEqual(3, e1.Field3);
        }
      }
    }
  }
}