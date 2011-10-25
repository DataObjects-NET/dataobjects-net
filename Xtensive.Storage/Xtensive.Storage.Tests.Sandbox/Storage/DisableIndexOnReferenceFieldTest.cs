// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.05

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Storage.DisableIndexOnReferenceFieldTestModel;

namespace Xtensive.Storage.Tests.Storage.DisableIndexOnReferenceFieldTestModel
{
  public interface IReference : IEntity
  {
    [Field(Indexed = false)]
    TestTarget Target { get; set; }
  }

  public class ReferenceContainer1 : Structure
  {
    [Field(Indexed = false)]
    public TestTarget Target { get; set; }
  }

  public class ReferenceContainer2 : Structure
  {
    [Field]
    public ReferenceContainer1 Container1 { get; set; }
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
  public class TestClassTable : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field(Indexed = false)]
    public TestTarget Target { get; set; }
  }

  public class TestClassTableChild : TestClassTable
  {
    [Field]
    public string Title { get; set; }
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.SingleTable)]
  public class TestSingleTable : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field(Indexed = false)]
    public TestTarget Target { get; set; }
  }

  public class TestSingleTableChild : TestSingleTable
  {
    [Field]
    public string Title { get; set; }
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
  public class TestConcreteTable : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field(Indexed = false)]
    public TestTarget Target { get; set; }
  }

  public class TestConcreteTableChild : TestConcreteTable
  {
    [Field]
    public string Title { get; set; }
  }

  [HierarchyRoot]
  public class TestContainer : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public ReferenceContainer1 Container1 { get; set; }

    [Field]
    public ReferenceContainer2 Container2 { get; set; }
  }

  [HierarchyRoot]
  public class TestImplementNonIndexedField : Entity, IReference
  {
    [Field, Key]
    public int Id { get; private set; }

    // Due to DO-203 applying [Field] resets Indexed = false setting.
    // [Field]
    public TestTarget Target { get; set; }
  }

  [HierarchyRoot]
  public class TestAddIndexToImplementedNonIndexedField : Entity, IReference
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Indexed = true)]
    public TestTarget Target { get; set; }
  }

  [HierarchyRoot]
  public class TestTarget : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class DisableIndexOnReferenceFieldTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (TestTarget).Assembly, typeof (TestTarget).Namespace);
      return config;
    }

    private List<IndexInfo> GetIndexes(Type type)
    {
      return Domain.Model.Types[type].Indexes
        .Where(i => i.IsSecondary && i.KeyColumns.Any(c => c.Key.Name.Contains("Target")))
        .ToList();
    }

    [Test]
    public void ClassTableTest()
    {
      Assert.IsEmpty(GetIndexes(typeof (TestClassTable)));
      Assert.IsEmpty(GetIndexes(typeof(TestClassTableChild)));
    }

    [Test]
    public void SingleTableTest()
    {
      Assert.IsEmpty(GetIndexes(typeof(TestSingleTable)));
      Assert.IsEmpty(GetIndexes(typeof(TestSingleTableChild)));
    }

    [Test]
    public void ConcreteTableTest()
    {
      Assert.IsEmpty(GetIndexes(typeof(TestConcreteTable)));
      Assert.IsEmpty(GetIndexes(typeof(TestConcreteTableChild)));
    }

    [Test]
    public void ImplementNonIndexedFieldTest()
    {
      Assert.IsEmpty(GetIndexes(typeof(TestImplementNonIndexedField)));
    }

    [Test]
    public void AddIndexToImplementedNonIndexedFieldTest()
    {
      Assert.IsNotEmpty(GetIndexes(typeof (TestAddIndexToImplementedNonIndexedField)));
    }

    [Test]
    public void StructureTest()
    {
      Assert.IsEmpty(GetIndexes(typeof(TestContainer)));
    }
  }
}