// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.10

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0282_EntitySetContains_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0282_EntitySetContains_Model
{
  [Serializable]
  [HierarchyRoot]
  public abstract class Parent : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Item> Items { get; private set; }
  }

  [Serializable]
  public class Child : Parent
  {
  }

  [Serializable]
  [HierarchyRoot]
  public class Item : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0282_EntitySetContains : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Parent).Assembly, typeof (Parent).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var child = new Child();
          var item = new Item();
          child.Items.Contains(item);

          // Rollback
        }
      }
    }
  }
}