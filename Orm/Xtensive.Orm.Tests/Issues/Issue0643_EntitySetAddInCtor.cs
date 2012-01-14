// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0643_EntitySetAddInCtor_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0643_EntitySetAddInCtor_Model
  {
    [HierarchyRoot]
    public abstract class ModuleBase : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 100)]
      public string Text { get; set; }

      [Field]
      [Association(PairTo = "Module", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<ModuleItem> Items { get; private set; }
    }

    public class Module : ModuleBase
    {
      public Module()
      {
        new ModuleItem();
//        Items.Add(new ModuleItem());
      }
    }

    [HierarchyRoot]
    public class ModuleItem : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public ModuleBase Module { get; set; }

      [Field(Length = 100)]
      public string Text { get; set; }
    }
  }

  [Serializable]
  public class Issue0643_EntitySetAddInCtor : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (Module).Assembly, typeof (Module).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          // Creating new persistent object
          var modules = new Module();

          // Committing transaction
          transactionScope.Complete();
        }
      }
    }
  }
}