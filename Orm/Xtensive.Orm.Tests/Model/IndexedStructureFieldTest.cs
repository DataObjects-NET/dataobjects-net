// Copyright (C) 2012-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      [Field(Indexed = true, Length = 250)]
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
      configuration.Types.RegisterCaching(typeof(Hierarchy).Assembly, typeof(Hierarchy).Namespace);
      return configuration;
    }

    [Test]
    public void Test()
    {
    }
  }
}