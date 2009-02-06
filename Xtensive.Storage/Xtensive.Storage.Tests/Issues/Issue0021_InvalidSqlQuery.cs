// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.19

using System;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0021_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0021_Model
{
  [HierarchyRoot(typeof (KeyGenerator), "ID", InheritanceSchema = InheritanceSchema.ClassTable)]
  public class RootClassTable : Entity
  {
    [Field]
    public long ID { get; private set; }

    [Field]
    public string StringField { get; set; }
  }

  public class DerivedClassTable : RootClassTable
  {
    [Field]
    public DateTime DateTimeField { get; set; }

    [Field]
    public bool BoolField { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0021_InvalidSqlQuery : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (RootClassTable).Assembly, typeof (RootClassTable).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var d1 = new DerivedClassTable
            {
              StringField = "1",
              BoolField = true,
              DateTimeField = new DateTime(1967, 10, 23)
            };
          var d2 = new DerivedClassTable
            {
              StringField = "2",
              BoolField = false,
              DateTimeField = new DateTime(1968, 11, 24)
            };

          t.Complete();
        }
        using (var t = Transaction.Open()) {
          var allD = Query<DerivedClassTable>.All;
          foreach (var d in allD) {
            d.Remove();
          }
          t.Complete();
        }
      }
    }
  }
}