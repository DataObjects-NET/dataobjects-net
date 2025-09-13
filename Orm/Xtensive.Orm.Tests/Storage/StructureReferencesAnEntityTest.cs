// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.02.16

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.StructureReferencesAnEntityTest_Model;

namespace Xtensive.Orm.Tests.Storage
{
  namespace StructureReferencesAnEntityTest_Model
  {
    [HierarchyRoot]
    public class Some : Entity
    {
      [Field,Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    public class SomeStructure : Structure
    {
      [Field]
      public string Value { get; set; }
      [Field]
      [Association(OnTargetRemove = OnRemoveAction.Clear)]
      public Some Reference { get; set; }
    }
  }

  [Serializable]
  public class StructureReferencesAnEntityTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof (SomeStructure).Assembly, typeof (SomeStructure).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var some = new Some {Name = " Alpha"};
        session.SaveChanges();
        some.Remove();
        t.Complete();
      }
    }
  }
}