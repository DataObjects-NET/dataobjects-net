// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.04.26

using System.Linq;
using System.Linq.Dynamic;
using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Tests.Issues.IssueJira0083_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0083_Model
  {
    [HierarchyRoot]
    public class Class1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }

    public class Class1DTO
    {
      public int Id { get; set; }

      public Class1DTO(Class1 entity)
      {
        Id = entity.Id;
      }
    }

    public class Class2DTO
    {
      public int Id { get; set; }

      public string Text { get; set; }

      public object Class1DTO { get; set; }
    }
  }

  public class IssueJira0083_DtoMaterialization : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Class1).Assembly, typeof (Class1).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {


          var class1 = new Class1();

          var q = session.Query.All<Class1>()
            .Select(c => new Class2DTO { Id = c.Id, Class1DTO = new Class1DTO(c), Text = c.Text});
          var r = q.ToArray()[0];
          Assert.IsNotNull(r.Class1DTO);
          // Rollback
        }
      }
    }
  }
}