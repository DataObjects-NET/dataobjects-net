// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.11

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.IndexedStructureFieldTestModel;

namespace Xtensive.Orm.Tests.Model
{
  namespace IndexedStructureFieldTestModel
  {
    public interface IHierarchical : IEntity
    {
      [Field(Nullable = false)]
      Hierarchy Hierarchy { get; }
    }

    public interface IHierarchical1 : IHierarchical
    {
    }

    public interface IHierarchical2 : IHierarchical
    {
    }

    public class Hierarchy : Structure
    {
      [Field(Indexed = true)]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    public class Indexed1 : Entity, IHierarchical1
    {
      [Key, Field]
      public int Id { get; set; }

      public Hierarchy Hierarchy { get; set; }
    }

    [HierarchyRoot]
    public class Indexed2 : Entity, IHierarchical2
    {
      [Key, Field]
      public int Id { get; set; }

      public Hierarchy Hierarchy { get; set; }
    }
  }

  public class IndexedStructureFieldTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.NamingConvention.NamingRules = NamingRules.UnderscoreDots;
      configuration.Types.Register(typeof (Hierarchy).Assembly, typeof (Hierarchy).Namespace);
      return configuration;
    }

    [Test]
    public void Test()
    {
    }
  }
}