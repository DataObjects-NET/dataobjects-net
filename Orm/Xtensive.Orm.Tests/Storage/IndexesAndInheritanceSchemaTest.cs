// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.06.20

using NUnit.Framework;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.IndexesAndInheritanceSchemaModel;

namespace Xtensive.Orm.Tests.Storage
{
  namespace IndexesAndInheritanceSchemaModel
  {
    [HierarchyRoot]
    [Index("ParentValue", "ParentRef", Unique = true, Name = "ParentEntity.IX_ParentValue_ParentRef")]
    public class ParentEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 100)]
      public string ParentValue { get; set; }

      [Field(Nullable = false)]
      public ReferencedEntity ParentRef { get; set; }
    }

    [Index("ChildValue", "ParentValue", Unique = true, Name = "ChildEntity.IX_ChildValue_ParentValue")]
    [Index("ChildValue", Unique = true, Name = "ChildEntity.IX_ChildValue")]
    [Index("ParentValue", Unique = true, Name = "ChildEntity.IX_ParentValue")]
    public class ChildEntity : ParentEntity
    {
      [Field]
      public int ChildValue { get; private set; }
    }

    [HierarchyRoot]
    public class ReferencedEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }
  }

  [TestFixture]
  public class IndexesAndInheritanceSchemaTest
  {
    private Domain BuildDomain(InheritanceSchema schema)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (ParentEntity).Assembly, typeof (ParentEntity).Namespace);
      configuration.Types.Register(typeof (ClassTableSchemaModifier));
      configuration.Types.Register(typeof (SingleTableSchemaModifier));
      configuration.Types.Register(typeof (SingleTableSchemaModifier));
      InheritanceSchemaModifier.ActivateModifier(schema);
      return Domain.Build(configuration);
    }

    [Test]
    public void ClassTableTest()
    {
      using (var domain = BuildDomain(InheritanceSchema.ClassTable)) {
        var model = domain.Model;
      }
    }

    [Test]
    public void SingleTableTest()
    {
      using (var domain = BuildDomain(InheritanceSchema.SingleTable)) {
        var model = domain.Model;
      }
    }

    [Test]
    public void ConcreteTableTest()
    {
      using (var domain = BuildDomain(InheritanceSchema.ConcreteTable)) {
        var model = domain.Model;
      }
    }
  }
}