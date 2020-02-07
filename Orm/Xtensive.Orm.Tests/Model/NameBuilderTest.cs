// Copyright (C) 2020 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2020.02.06

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Model.NameBuilderTestModel;
using Xtensive.Orm.Weaving;
using Xtensive.Reflection;
using Xtensive.Orm.Building.Definitions;
using System.Linq;

namespace Xtensive.Orm.Tests.Model.NameBuilderTestModel
{
  [HierarchyRoot]
  public class RootEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Field1 { get; set; }

    [Field]
    [OverrideFieldName("Field2Overridden")]
    public string Field2 { get; set; }
  }

  public class MidEntity : RootEntity
  {
    [Field]
    public string Field3 { get; set; }

    [Field]
    [OverrideFieldName("Field4Overriden")]
    public string Field4 { get; set; }
  }

  public class LeafEntity : MidEntity
  {
    [Field]
    public string Field5 { get; set; }

    [Field]
    [OverrideFieldName("Field6Overriden")]
    public string Field6 { get; set; }
  }

  [HierarchyRoot]
  public class EntitySetTestEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    [Association(PairTo = "PairedField")]
    [OverrideFieldName("NotItems")]
    public EntitySet<EsItem> Items { get; private set; }
  }

  [HierarchyRoot]
  public class StructureTestEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    [OverrideFieldName("NotPoint")]
    public Point Point { get;set; }
  }

  [HierarchyRoot]
  public class EsItem : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public EntitySetTestEntity PairedField { get; set; }
  }

  public class Point : Structure
  {
    [Field]
    public int X { get; set; }

    [Field]
    public int Y { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture]
  public class NameBuilderTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (RootEntity).Assembly, typeof (RootEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    [Test]
    public void BuildFieldNameByNullPropertyTest()
    {
      var nameBuilder = CreateNameBuilder();
      Assert.Throws<ArgumentNullException>(() => nameBuilder.BuildFieldName((PropertyInfo) null));
    }

    [Test]
    public void BuildFieldNameByNullFieldDefTest()
    {
      var nameBuilder = CreateNameBuilder();
      Assert.Throws<ArgumentNullException>(() => nameBuilder.BuildFieldName((FieldDef) null));
    }

    [Test]
    public void BuildFieldNameByPropertyTest01()
    {
      var nameBuilder = CreateNameBuilder();
      var ownProperty = typeof (RootEntity).GetProperty(nameof (RootEntity.Field1));
      Assert.That(ownProperty.DeclaringType, Is.EqualTo(typeof (RootEntity)));
      Assert.That(ownProperty.ReflectedType, Is.EqualTo(typeof (RootEntity)));

      var name = nameBuilder.BuildFieldName(ownProperty);
      Assert.That(name, Is.EqualTo(nameof (RootEntity.Field1)));
    }

    [Test]
    public void BuildFieldNameByPropertyTest02()
    {
      var nameBuilder = CreateNameBuilder();
      var ownProperty = typeof (RootEntity).GetProperty(nameof (RootEntity.Field2));
      Assert.That(ownProperty.DeclaringType, Is.EqualTo(typeof (RootEntity)));
      Assert.That(ownProperty.ReflectedType, Is.EqualTo(typeof (RootEntity)));

      var name = nameBuilder.BuildFieldName(ownProperty);
      Assert.That(name, Is.Not.EqualTo(nameof (RootEntity.Field2)));
      var overridenName = ownProperty.GetAttribute<OverrideFieldNameAttribute>().Name;
      Assert.That(name, Is.EqualTo(overridenName));
    }

    [Test]
    public void BuildFieldNameByPropertyTest03()
    {
      var nameBuilder = CreateNameBuilder();
      var inheritedProperty = typeof (MidEntity).GetProperty(nameof (MidEntity.Field1));
      Assert.That(inheritedProperty.DeclaringType, Is.EqualTo(typeof (RootEntity)));
      Assert.That(inheritedProperty.ReflectedType, Is.EqualTo(typeof (MidEntity)));

      var name = nameBuilder.BuildFieldName(inheritedProperty);
      Assert.That(name, Is.EqualTo(nameof (MidEntity.Field1)));
    }

    [Test]
    public void BuildFieldNameByPropertyTest04()
    {
      var nameBuilder = CreateNameBuilder();
      var inheritedProperty = typeof (MidEntity).GetProperty(nameof (MidEntity.Field2));
      Assert.That(inheritedProperty.DeclaringType, Is.EqualTo(typeof (RootEntity)));
      Assert.That(inheritedProperty.ReflectedType, Is.EqualTo(typeof (MidEntity)));

      var name = nameBuilder.BuildFieldName(inheritedProperty);
      Assert.That(name, Is.Not.EqualTo(nameof (MidEntity.Field2)));
      var overridenName = inheritedProperty.GetAttribute<OverrideFieldNameAttribute>().Name;
      Assert.That(name, Is.EqualTo(overridenName));
    }

    [Test]
    public void BuildFieldNameByPropertyTest05()
    {
      var nameBuilder = CreateNameBuilder();
      var ownProperty = typeof (MidEntity).GetProperty(nameof (MidEntity.Field3));
      Assert.That(ownProperty.DeclaringType, Is.EqualTo(typeof (MidEntity)));
      Assert.That(ownProperty.ReflectedType, Is.EqualTo(typeof (MidEntity)));

      var name = nameBuilder.BuildFieldName(ownProperty);
      Assert.That(name, Is.EqualTo(nameof (MidEntity.Field3)));
    }

    [Test]
    public void BuildFieldNameByPropertyTest06()
    {
      var nameBuilder = CreateNameBuilder();
      var ownProperty = typeof (MidEntity).GetProperty(nameof (MidEntity.Field4));
      Assert.That(ownProperty.DeclaringType, Is.EqualTo(typeof (MidEntity)));
      Assert.That(ownProperty.ReflectedType, Is.EqualTo(typeof (MidEntity)));

      var name = nameBuilder.BuildFieldName(ownProperty);
      Assert.That(name, Is.Not.EqualTo(nameof (MidEntity.Field4)));
      var overridenName = ownProperty.GetAttribute<OverrideFieldNameAttribute>().Name;
      Assert.That(name, Is.EqualTo(overridenName));
    }

    [Test]
    public void BuildFieldNameByPropertyTest07()
    {
      var nameBuilder = CreateNameBuilder();
      var inheritedProperty = typeof (LeafEntity).GetProperty(nameof (LeafEntity.Field1));
      Assert.That(inheritedProperty.DeclaringType, Is.EqualTo(typeof (RootEntity)));
      Assert.That(inheritedProperty.ReflectedType, Is.EqualTo(typeof (LeafEntity)));

      var name = nameBuilder.BuildFieldName(inheritedProperty);
      Assert.That(name, Is.EqualTo(nameof (LeafEntity.Field1)));
    }

    [Test]
    public void BuildFieldNameByPropertyTest08()
    {
      var nameBuilder = CreateNameBuilder();
      var inheritedProperty = typeof (LeafEntity).GetProperty(nameof (LeafEntity.Field2));
      Assert.That(inheritedProperty.DeclaringType, Is.EqualTo(typeof (RootEntity)));
      Assert.That(inheritedProperty.ReflectedType, Is.EqualTo(typeof (LeafEntity)));

      var name = nameBuilder.BuildFieldName(inheritedProperty);
      Assert.That(name, Is.Not.EqualTo(nameof (LeafEntity.Field2)));
      var overridenName = inheritedProperty.GetAttribute<OverrideFieldNameAttribute>().Name;
      Assert.That(name, Is.EqualTo(overridenName));
    }

    [Test]
    public void BuildFieldNameByPropertyTest09()
    {
      var nameBuilder = CreateNameBuilder();
      var inheritedProperty = typeof (LeafEntity).GetProperty(nameof (LeafEntity.Field3));
      Assert.That(inheritedProperty.DeclaringType, Is.EqualTo(typeof (MidEntity)));
      Assert.That(inheritedProperty.ReflectedType, Is.EqualTo(typeof (LeafEntity)));

      var name = nameBuilder.BuildFieldName(inheritedProperty);
      Assert.That(name, Is.EqualTo(nameof(MidEntity.Field3)));
    }

    [Test]
    public void BuildFieldNameByPropertyTest10()
    {
      var nameBuilder = CreateNameBuilder();
      var inheritedProperty = typeof (LeafEntity).GetProperty(nameof (LeafEntity.Field4));
      Assert.That(inheritedProperty.DeclaringType, Is.EqualTo(typeof (MidEntity)));
      Assert.That(inheritedProperty.ReflectedType, Is.EqualTo(typeof (LeafEntity)));

      var name = nameBuilder.BuildFieldName(inheritedProperty);
      Assert.That(name, Is.Not.EqualTo(nameof (MidEntity.Field4)));
      var overridenName = inheritedProperty.GetAttribute<OverrideFieldNameAttribute>().Name;
      Assert.That(name, Is.EqualTo(overridenName));
    }

    [Test]
    public void BuildFieldNameByPropertyTest11()
    {
      var nameBuilder = CreateNameBuilder();
      var ownProperty = typeof (LeafEntity).GetProperty(nameof (LeafEntity.Field5));
      Assert.That(ownProperty.DeclaringType, Is.EqualTo(typeof (LeafEntity)));
      Assert.That(ownProperty.ReflectedType, Is.EqualTo(typeof (LeafEntity)));

      var name = nameBuilder.BuildFieldName(ownProperty);
      Assert.That(name, Is.EqualTo(nameof (LeafEntity.Field5)));
    }

    [Test]
    public void BuildFieldNameByPropertyTest12()
    {
      var nameBuilder = CreateNameBuilder();
      var ownProperty = typeof (LeafEntity).GetProperty(nameof (LeafEntity.Field6));
      Assert.That(ownProperty.DeclaringType, Is.EqualTo(typeof (LeafEntity)));
      Assert.That(ownProperty.ReflectedType, Is.EqualTo(typeof (LeafEntity)));

      var name = nameBuilder.BuildFieldName(ownProperty);
      Assert.That(name, Is.Not.EqualTo(nameof (LeafEntity.Field6)));
      var overridenName = ownProperty.GetAttribute<OverrideFieldNameAttribute>().Name;
      Assert.That(name, Is.EqualTo(overridenName));
    }

    [Test]
    public void BuildFieldNameByFieldDefTest01()
    {
      var nameBuilder = CreateNameBuilder();
      var ownProperty = typeof (RootEntity).GetProperty(nameof (RootEntity.Field1));
      Assert.That(ownProperty.DeclaringType, Is.EqualTo(typeof (RootEntity)));
      Assert.That(ownProperty.ReflectedType, Is.EqualTo(typeof (RootEntity)));

      var fieldDef = CreateFieldDef(ownProperty);
      var name = nameBuilder.BuildFieldName(fieldDef);
      Assert.That(name, Is.EqualTo(nameof (RootEntity.Field1)));
    }

    [Test]
    public void BuildFieldNameByFieldDefTest02()
    {
      var nameBuilder = CreateNameBuilder();
      var ownProperty = typeof (RootEntity).GetProperty(nameof (RootEntity.Field2));
      Assert.That(ownProperty.DeclaringType, Is.EqualTo(typeof (RootEntity)));
      Assert.That(ownProperty.ReflectedType, Is.EqualTo(typeof (RootEntity)));

      var fieldDef = CreateFieldDef(ownProperty);
      var name = nameBuilder.BuildFieldName(fieldDef);
      Assert.That(name, Is.Not.EqualTo(nameof (RootEntity.Field2)));
      var overridenName = ownProperty.GetAttribute<OverrideFieldNameAttribute>().Name;
      Assert.That(name, Is.EqualTo(overridenName));
    }

    [Test]
    public void BuildFieldNameByFieldDefTest03()
    {
      var nameBuilder = CreateNameBuilder();
      var inheritedProperty = typeof (MidEntity).GetProperty(nameof (MidEntity.Field1));
      Assert.That(inheritedProperty.DeclaringType, Is.EqualTo(typeof (RootEntity)));
      Assert.That(inheritedProperty.ReflectedType, Is.EqualTo(typeof (MidEntity)));

      var fieldDef = CreateFieldDef(inheritedProperty);
      var name = nameBuilder.BuildFieldName(fieldDef);
      Assert.That(name, Is.EqualTo(nameof (MidEntity.Field1)));
    }

    [Test]
    public void BuildFieldNameByFieldDefTest04()
    {
      var nameBuilder = CreateNameBuilder();
      var inheritedProperty = typeof (MidEntity).GetProperty(nameof (MidEntity.Field2));
      Assert.That(inheritedProperty.DeclaringType, Is.EqualTo(typeof (RootEntity)));
      Assert.That(inheritedProperty.ReflectedType, Is.EqualTo(typeof (MidEntity)));

      var fieldDef = CreateFieldDef(inheritedProperty);
      var name = nameBuilder.BuildFieldName(fieldDef);
      Assert.That(name, Is.Not.EqualTo(nameof (MidEntity.Field2)));
      var overridenName = inheritedProperty.GetAttribute<OverrideFieldNameAttribute>().Name;
      Assert.That(name, Is.EqualTo(overridenName));
    }

    [Test]
    public void BuildFieldNameByFieldDefTest05()
    {
      var nameBuilder = CreateNameBuilder();
      var ownProperty = typeof (MidEntity).GetProperty(nameof (MidEntity.Field3));
      Assert.That(ownProperty.DeclaringType, Is.EqualTo(typeof (MidEntity)));
      Assert.That(ownProperty.ReflectedType, Is.EqualTo(typeof (MidEntity)));

      var fieldDef = CreateFieldDef(ownProperty);
      var name = nameBuilder.BuildFieldName(fieldDef);
      Assert.That(name, Is.EqualTo(nameof (MidEntity.Field3)));
    }

    [Test]
    public void BuildFieldNameByFieldDefTest06()
    {
      var nameBuilder = CreateNameBuilder();
      var ownProperty = typeof (MidEntity).GetProperty(nameof (MidEntity.Field4));
      Assert.That(ownProperty.DeclaringType, Is.EqualTo(typeof (MidEntity)));
      Assert.That(ownProperty.ReflectedType, Is.EqualTo(typeof (MidEntity)));

      var fieldDef = CreateFieldDef(ownProperty);
      var name = nameBuilder.BuildFieldName(fieldDef);
      Assert.That(name, Is.Not.EqualTo(nameof (MidEntity.Field4)));
      var overridenName = ownProperty.GetAttribute<OverrideFieldNameAttribute>().Name;
      Assert.That(name, Is.EqualTo(overridenName));
    }

    [Test]
    public void BuildFieldNameByFieldDefTest07()
    {
      var nameBuilder = CreateNameBuilder();
      var inheritedProperty = typeof (LeafEntity).GetProperty(nameof (LeafEntity.Field1));
      Assert.That(inheritedProperty.DeclaringType, Is.EqualTo(typeof (RootEntity)));
      Assert.That(inheritedProperty.ReflectedType, Is.EqualTo(typeof (LeafEntity)));

      var fieldDef = CreateFieldDef(inheritedProperty);
      var name = nameBuilder.BuildFieldName(fieldDef);
      Assert.That(name, Is.EqualTo(nameof (LeafEntity.Field1)));
    }

    [Test]
    public void BuildFieldNameByFieldDefTest08()
    {
      var nameBuilder = CreateNameBuilder();
      var inheritedProperty = typeof (LeafEntity).GetProperty(nameof (LeafEntity.Field2));
      Assert.That(inheritedProperty.DeclaringType, Is.EqualTo(typeof (RootEntity)));
      Assert.That(inheritedProperty.ReflectedType, Is.EqualTo(typeof (LeafEntity)));

      var fieldDef = CreateFieldDef(inheritedProperty);
      var name = nameBuilder.BuildFieldName(fieldDef);
      Assert.That(name, Is.Not.EqualTo(nameof (LeafEntity.Field2)));
      var overridenName = inheritedProperty.GetAttribute<OverrideFieldNameAttribute>().Name;
      Assert.That(name, Is.EqualTo(overridenName));
    }

    [Test]
    public void BuildFieldNameByFieldDefTest09()
    {
      var nameBuilder = CreateNameBuilder();
      var inheritedProperty = typeof (LeafEntity).GetProperty(nameof (LeafEntity.Field3));
      Assert.That(inheritedProperty.DeclaringType, Is.EqualTo(typeof (MidEntity)));
      Assert.That(inheritedProperty.ReflectedType, Is.EqualTo(typeof (LeafEntity)));

      var fieldDef = CreateFieldDef(inheritedProperty);
      var name = nameBuilder.BuildFieldName(fieldDef);
      Assert.That(name, Is.EqualTo(nameof (MidEntity.Field3)));
    }

    [Test]
    public void BuildFieldNameByFieldDefTest10()
    {
      var nameBuilder = CreateNameBuilder();
      var inheritedProperty = typeof (LeafEntity).GetProperty(nameof (LeafEntity.Field4));
      Assert.That(inheritedProperty.DeclaringType, Is.EqualTo(typeof (MidEntity)));
      Assert.That(inheritedProperty.ReflectedType, Is.EqualTo(typeof (LeafEntity)));

      var fieldDef = CreateFieldDef(inheritedProperty);
      var name = nameBuilder.BuildFieldName(fieldDef);
      Assert.That(name, Is.Not.EqualTo(nameof (MidEntity.Field4)));
      var overridenName = inheritedProperty.GetAttribute<OverrideFieldNameAttribute>().Name;
      Assert.That(name, Is.EqualTo(overridenName));
    }

    [Test]
    public void BuildFieldNameByFieldDefTest11()
    {
      var nameBuilder = CreateNameBuilder();
      var ownProperty = typeof (LeafEntity).GetProperty(nameof (LeafEntity.Field5));
      Assert.That(ownProperty.DeclaringType, Is.EqualTo(typeof (LeafEntity)));
      Assert.That(ownProperty.ReflectedType, Is.EqualTo(typeof (LeafEntity)));

      var fieldDef = CreateFieldDef(ownProperty);
      var name = nameBuilder.BuildFieldName(fieldDef);
      Assert.That(name, Is.EqualTo(nameof (LeafEntity.Field5)));
    }

    [Test]
    public void BuildFieldNameByFieldDefTest12()
    {
      var nameBuilder = CreateNameBuilder();
      var ownProperty = typeof (LeafEntity).GetProperty(nameof (LeafEntity.Field6));
      Assert.That(ownProperty.DeclaringType, Is.EqualTo(typeof (LeafEntity)));
      Assert.That(ownProperty.ReflectedType, Is.EqualTo(typeof (LeafEntity)));

      var fieldDef = CreateFieldDef(ownProperty);
      var name = nameBuilder.BuildFieldName(fieldDef);
      Assert.That(name, Is.Not.EqualTo(nameof (LeafEntity.Field6)));
      var overridenName = ownProperty.GetAttribute<OverrideFieldNameAttribute>().Name;
      Assert.That(name, Is.EqualTo(overridenName));
    }

    [Test]
    public void BuildFieldNameCachingTest01()
    {
      var nameBuilder = CreateNameBuilder();
      var property = typeof (RootEntity).GetProperty(nameof (RootEntity.Field2));
      Assert.That(property.DeclaringType, Is.EqualTo(typeof (RootEntity)));
      Assert.That(property.ReflectedType, Is.EqualTo(typeof (RootEntity)));

      var fieldDef = CreateFieldDef(property);
      var nameByDef = nameBuilder.BuildFieldName(fieldDef);
      var nameByProp = nameBuilder.BuildFieldName(property);

      Assert.That(nameByDef, Is.Not.EqualTo(nameof (RootEntity.Field1)));
      var overridenName = property.GetAttribute<OverrideFieldNameAttribute>().Name;
      Assert.That(nameByDef, Is.EqualTo(overridenName));
      Assert.That(nameByProp, Is.EqualTo(nameByDef));
    }

    [Test]
    public void BuildFieldNameCachingTest02()
    {
      var nameBuilder = CreateNameBuilder();
      var property = typeof (RootEntity).GetProperty(nameof (RootEntity.Field2));
      Assert.That(property.DeclaringType, Is.EqualTo(typeof (RootEntity)));
      Assert.That(property.ReflectedType, Is.EqualTo(typeof (RootEntity)));

      var fieldDef = CreateFieldDef(property);
      var nameByProp = nameBuilder.BuildFieldName(property);
      var nameByDef = nameBuilder.BuildFieldName(fieldDef);

      Assert.That(nameByDef, Is.Not.EqualTo(nameof (RootEntity.Field1)));
      var overridenName = property.GetAttribute<OverrideFieldNameAttribute>().Name;
      Assert.That(nameByDef, Is.EqualTo(overridenName));
      Assert.That(nameByProp, Is.EqualTo(nameByDef));
    }


    [Test]
    public void QueryTranslationTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var originalNameBuilder = Domain.Handlers.NameBuilder;
        try {
          Assert.DoesNotThrow(() => session.Query.All<EntitySetTestEntity>().Where(q => q.Items.Count > 0).Run());
          Domain.Handlers.NameBuilder = CreateNameBuilder(); // dropping cache basically :)
          Assert.DoesNotThrow(() => session.Query.All<EntitySetTestEntity>().Where(q => q.Items.Count > 0).Run());
        }
        finally {
          Domain.Handlers.NameBuilder = originalNameBuilder;
        }
      }
    }

    [Test]
    public void QueryTranslationTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var originalNameBuilder = Domain.Handlers.NameBuilder;
        try {
          Assert.DoesNotThrow(() => session.Query.All<StructureTestEntity>().Where(q => q.Point.X > 0).Run());
          Domain.Handlers.NameBuilder = CreateNameBuilder(); // dropping cache basically :)
          Assert.DoesNotThrow(() => session.Query.All<StructureTestEntity>().Where(q => q.Point.X > 0).Run());
        }
        finally {
          Domain.Handlers.NameBuilder = originalNameBuilder;
        }
      }
    }

    private NameBuilder CreateNameBuilder()
    {
      var builder = new NameBuilder(Domain.Configuration, StorageProviderInfo.Instance.Info);
      return builder;
    }

    private FieldDef CreateFieldDef(PropertyInfo propertyInfo)
    {
      return new FieldDef(propertyInfo, null);
    }
  }
}