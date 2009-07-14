// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.10

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0282_EntitySetContains_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0282_EntitySetContains_Model
{
  [HierarchyRoot]
  public abstract class Parent : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Item> Items { get; private set; }
  }

  public class Child : Parent
  {
  }

  [HierarchyRoot]
  public class Item : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
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
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {

          var child = new Child();
          var item = new Item();
          child.Items.Contains(item);

          // Rollback
        }
      }
    }
  }
}