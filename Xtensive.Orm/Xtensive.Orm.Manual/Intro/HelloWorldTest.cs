// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.23

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Manual.Intro.HelloWorld
{
  #region Model

  [Serializable]
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 100)]
    public string Text { get; set; }
  }

  #endregion

  [TestFixture]
  public class HelloWorldTest
  {
    [Test]
    public void MainTest()
    {
      // Creatign new Domain configuration
      var config = new DomainConfiguration("memory://localhost/DO40-Tests") {
        UpgradeMode = DomainUpgradeMode.Recreate
      };
      // Modifying it by registering the persistent types
      config.Types.Register(typeof(MyEntity).Assembly, typeof(MyEntity).Namespace);
      // And finally building the domain
      var domain = Domain.Build(config);

      using (var session = domain.OpenSession()) {
        Debug.Assert(session==Session.Current);
      }

      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var helloWorld = new MyEntity {
            Text = "Hello World!"
          };
          // Marking transaction as committable
          transactionScope.Complete();
        }
      }

      // Reading all persisted objects from another Session
      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          foreach (var myEntity in session.Query.All<MyEntity>())
            Console.WriteLine(myEntity.Text);
          transactionScope.Complete();
        }
      }
    }
  }
}