// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0715.Model;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Issues.Issue0715.Model
{
  public static class Forest
  {
    [Serializable]
    [HierarchyRoot]
    public class Animal : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public override string ToString()
      {
        return Name;
      }
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Animal : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    public override string ToString()
    {
      return Name;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0715_NestedTypeBug : AutoBuildTest
  {
    private const string VersionFieldName = "Version";

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Animal).Assembly, typeof(Animal).Namespace);
      return configuration;
    }

    [Test]
    public void StandardTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new Forest.Animal {Name = "Forest Animal"};
        new Animal {Name = "Animal"};
      }
    }
  }
}