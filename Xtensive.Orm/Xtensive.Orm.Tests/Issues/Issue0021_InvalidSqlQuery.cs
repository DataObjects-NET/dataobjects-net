// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.19

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.Issue0021_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0021_Model
{
  [Serializable]
  [HierarchyRoot(InheritanceSchema.ClassTable)]
  public class Root : Entity
  {
    [Field, Key]
    public long ID { get; private set; }

    [Field]
    public string StringField { get; set; }
  }

  [Serializable]
  public class Child1 : Root
  {
    [Field]
    public Guid GuidField { get; set; }

  }

  [Serializable]
  public class Child2 : Child1
  {
    [Field]
    public DateTime DateTimeField { get; set; }

    [Field]
    public bool BoolField { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0021_InvalidSqlQuery : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Root).Assembly, typeof (Root).Namespace);
      return config;
    }

    /*protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      if (configuration.ConnectionInfo.Url.StartsWith("memory") && configuration.Builders.Contains(typeof(IncludeTypeIdModifier))) {
        throw new IgnoreException("This configuration hangs the test");
      }
      return base.BuildDomain(configuration);
    }*/

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          new Child2
            {
              StringField = "1",
              BoolField = true,
              DateTimeField = new DateTime(1967, 10, 23)
            };
          new Child2
            {
              StringField = "2",
              BoolField = false,
              DateTimeField = new DateTime(1968, 11, 24)
            };

          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          var all = session.Query.All<Child2>();
          foreach (var obj in all) {
            obj.Remove();
          }
          t.Complete();
        }
      }
    }
  }
}