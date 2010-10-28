// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.24

using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0630_NullReferenceOnSelect_Model;
using System.Linq.Dynamic;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0630_NullReferenceOnSelect_Model
  {
    [Serializable]
    [HierarchyRoot]
    public class MyEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 100)]
      public string Text { get; set; }
    }

    [Serializable]
    [HierarchyRoot]
    public class MyEntity2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 100)]
      public string Text { get; set; }
    }

    public class ValObj
    {
      public int Id { get; set; }
      public string Text { get; set; }
    }
  }

  [Serializable]
  public class Issue0630_NullReferenceOnSelect : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof(MyEntity).Assembly, typeof(MyEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          // Creating new persistent object
          // Creating new persistent object
          var helloWorld = new MyEntity
          {
            Text = "Hello World!"
          }; var helloWorld2 = new MyEntity2
          {
            Text = "Hello World!"
          };

          // Committing transaction
          transactionScope.Complete();
        }

        using (var transactionScope = session.OpenTransaction()) {
          var c = session.Query.All<MyEntity>()
            .Select(a => new ValObj {Id = a.Id, Text = a.Text})
            .ToList();
          var d = session.Query.All<MyEntity2>()
            .Select(a => new ValObj { Id = a.Id, Text = a.Text })
            .ToList();
          var a11 = new ArrayList() {session.Query.All<MyEntity>().Select("new(Id,Text)")};
          var b12 = new ArrayList() {session.Query.All<MyEntity2>().Select("new(Id,Text)")};
        }
      }
    }
  }
}