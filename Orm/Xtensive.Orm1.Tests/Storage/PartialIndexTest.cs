// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.06

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.PartialIndexTestModel;

namespace Xtensive.Orm.Tests.Storage.PartialIndexTestModel
{
  public class TestBase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [HierarchyRoot]
  public class TargetEntity : TestBase
  {
  }

  [HierarchyRoot, KeyGenerator(KeyGeneratorKind.None)]
  public class ComplexTargetEntity : Entity
  {
    [Field, Key(0)]
    public int Id1 { get; private set; }

    [Field, Key(1)]
    public int Id2 { get; private set; }

    public ComplexTargetEntity(int id1, int id2)
      : base(id1, id2)
    {
    }
  }

  public interface ITestInterface : IEntity
  {
    [Field]
    string TestField { get; set; }
  }

  [HierarchyRoot, Index("TestField", Filter = "Index")]
  public class SimpleFilterWithMethod : TestBase
  {
    public static Expression<Func<SimpleFilterWithMethod, bool>> Index()
    {
      return test => test.TestField.GreaterThan("hello world");
    }

    [Field]
    public string TestField { get; set; }
  }

  [HierarchyRoot, Index("TestField", Filter = "Index")]
  public class SimpleFilterWithProperty : TestBase
  {
    public static Expression<Func<SimpleFilterWithProperty, bool>> Index
    {
      get { return test => test.TestField.GreaterThan("hello world"); }
    }

    [Field]
    public string TestField { get; set; }
  }

  [HierarchyRoot, Index("Target", Filter = "Index")]
  public class FilterOnReferenceField : TestBase
  {
    public static Expression<Func<FilterOnReferenceField, bool>> Index()
    {
      return test => test.Target!=null;
    }

    [Field]
    public TargetEntity Target { get; set; } 
  }

  [HierarchyRoot, Index("Target", Filter = "Index")]
  public class FilterOnComplexReferenceField : TestBase
  {
    public static Expression<Func<FilterOnComplexReferenceField, bool>> Index()
    {
      return test => test.Target!=null;
    }

    [Field]
    public ComplexTargetEntity Target { get; set; }
  }

  [HierarchyRoot, Index("Target", Filter = "Index")]
  public class FilterOnReferenceIdField : TestBase
  {
    public static Expression<Func<FilterOnReferenceIdField, bool>> Index()
    {
      return test => test.Target.Id > 0;
    }

    [Field]
    public TargetEntity Target { get; set; }
  }

  [HierarchyRoot, Index("TestField1", Filter = "Index")]
  public class FilterOnAlienField : TestBase
  {
    public static Expression<Func<FilterOnAlienField, bool>> Index
    {
      get { return test => test.TestField2.GreaterThan("hello world"); }
    }

    [Field]
    public string TestField1 { get; set; }

    [Field]
    public string TestField2 { get; set; }
  }

  [HierarchyRoot, Index("TestField", Filter = "Index")]
  public class MultipleFieldUses : TestBase
  {
    public static Expression<Func<MultipleFieldUses, bool>> Index
    {
      get { return test => test.TestField.GreaterThan("hello") && test.TestField.LessThan("world"); }
    }

    [Field]
    public string TestField { get; set; }
  }

  [HierarchyRoot, Index("TestField", Filter = "Index")]
  public class InterfaceSupport : TestBase, ITestInterface
  {
    public static Expression<Func<InterfaceSupport, bool>> Index
    {
      get { return test => test.TestField.LessThan("bye world"); }
    }

    [Field]
    public string TestField { get; set; }
  }

  [HierarchyRoot, Index("TestField", Filter = "Index")]
  public class InOperatorSupport : TestBase
  {
    public static Expression<Func<InOperatorSupport, bool>> Index
    {
      get { return test => test.TestField.In("1", "2", "3"); }
    }

    [Field]
    public string TestField { get; set; }
  }

  [HierarchyRoot, Index("TestField", Filter = "Index")]
  public class InOperatorSupport2 : TestBase
  {
    public static Expression<Func<InOperatorSupport2, bool>> Index
    {
      get {
        return test => test.TestField.In(
          new Guid("{D27F71D7-D4FC-446C-8C7E-E89DC2209863}"),
          new Guid("{94ED80D9-6749-41E2-B60D-BEDE1CDCA237}"));
      }
    }

    [Field]
    public Guid TestField { get; set; }
  }

  [HierarchyRoot, Index("TestField", Filter = "Index")]
  public class ContainsOperatorSupport : TestBase
  {
    public static Expression<Func<ContainsOperatorSupport, bool>> Index
    {
      get { return test => new[] {"1", "2", "3"}.Contains(test.TestField); }
    }

    [Field]
    public string TestField { get; set; }
  }

  [HierarchyRoot,
    Index("TestField", Filter = "More", Name = "MoreIndex"),
    Index("TestField", Filter = "Less", Name = "LessIndex")]
  public class DoubleIndexWithName : TestBase
  {
    public static Expression<Func<DoubleIndexWithName, bool>> More()
    {
      return test => test.TestField > 1000;
    }

    public static Expression<Func<DoubleIndexWithName, bool>> Less()
    {
      return test => test.TestField < 100;
    }

    [Field]
    public int TestField { get; set; }
  }

  [HierarchyRoot,
    Index("TestField", Filter = "More"),
    Index("TestField", Filter = "Less")]
  public class DoubleIndexWithoutName : TestBase
  {
    public static Expression<Func<DoubleIndexWithoutName, bool>> More()
    {
      return test => test.TestField > 1000;
    }

    public static Expression<Func<DoubleIndexWithoutName, bool>> Less()
    {
      return test => test.TestField < 100;
    }

    [Field]
    public int TestField { get; set; }
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
  public class InheritanceClassTableBase : TestBase
  {
    [Field]
    public int BaseField { get; set; }
  }

  [Index("TestField", Filter = "Index")]
  public class InheritanceClassTable : InheritanceClassTableBase
  {
    public static Expression<Func<InheritanceClassTable, bool>> Index()
    {
      return test => test.BaseField > 0;
    }

    [Field]
    public int TestField { get; set; }
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.SingleTable)]
  public class InheritanceSingleTableBase : TestBase
  {
    [Field]
    public int BaseField { get; set; }
  }

  [Index("TestField", Filter = "Index")]
  public class InheritanceSingleTable : InheritanceSingleTableBase
  {
    public static Expression<Func<InheritanceSingleTable, bool>> Index()
    {
      return test => test.BaseField > 0;
    }

    [Field]
    public int TestField { get; set; }
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
  public class InheritanceConcreteTableBase : TestBase
  {
    [Field]
    public int BaseField { get; set; }
  }

  [Index("TestField", Filter = "Index")]
  public class InheritanceConcreteTable : InheritanceConcreteTableBase
  {
    public static Expression<Func<InheritanceConcreteTable, bool>> Index()
    {
      return test => test.BaseField > 0;
    }

    [Field]
    public int TestField { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class PartialIndexTest
  {
    private Domain domain;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Require.AllFeaturesSupported(ProviderFeatures.PartialIndexes);
    }

    [TearDown]
    public void TearDown()
    {
      CleanDomain();
    }

    private void CleanDomain()
    {
      if (domain==null)
        return;
      try {
        domain.Dispose();
      }
      finally {
        domain = null;
      }
    }

    private void BuildDomain(IEnumerable<Type> entities, DomainUpgradeMode mode)
    {
      CleanDomain();
      var config = DomainConfigurationFactory.Create();
      foreach (var entity in entities)
        config.Types.Register(entity);
      config.UpgradeMode = mode;
      domain = Domain.Build(config);
    }

    private void AssertBuildSuccess(params Type[] entities)
    {
      BuildDomain(entities, DomainUpgradeMode.Recreate);
      var partialIndexes = domain.Model.RealIndexes
        .Where(index => index.IsPartial && index.FilterExpression!=null && index.Filter!=null)
        .ToList();
      Assert.IsNotEmpty(partialIndexes);
    }

    private void AssertBuildFailure(params Type[] entities)
    {
      AssertEx.Throws<DomainBuilderException>(() => BuildDomain(entities, DomainUpgradeMode.Recreate));
    }

    [Test]
    public void SimpleFilterWithMethodTest()
    {
      AssertBuildSuccess(typeof (SimpleFilterWithMethod));
    }

    [Test]
    public void SimpleFilterWithPropertyTest()
    {
      AssertBuildSuccess(typeof (SimpleFilterWithProperty));
    }

    [Test]
    public void FilterOnReferenceFieldTest()
    {
      AssertBuildSuccess(typeof (FilterOnReferenceField));
    }

    [Test]
    public void FilterOnComplexReferenceFieldTest()
    {
      AssertBuildSuccess(typeof (FilterOnComplexReferenceField));
    }

    [Test]
    public void FilterOnReferenceFieldIdTest()
    {
      AssertBuildSuccess(typeof (FilterOnReferenceIdField));
    }

    [Test]
    public void FilterOnAlienFieldTest()
    {
      AssertBuildSuccess(typeof (FilterOnAlienField));
    }

    [Test]
    public void MultipleFieldUsesTest()
    {
      AssertBuildSuccess(typeof(MultipleFieldUses));
    }

    [Test]
    public void InOperatorSupportTest()
    {
      AssertBuildSuccess(typeof (InOperatorSupport));
    }

    [Test]
    public void InOperatorSupport2Test()
    {
      AssertBuildSuccess(typeof(InOperatorSupport2));
    }

    [Test]
    public void ContainsOperatorSupportTest()
    {
      AssertBuildSuccess(typeof(ContainsOperatorSupport));
    }

    [Test]
    public void DoubleIndexWithNameTest()
    {
      AssertBuildSuccess(typeof (DoubleIndexWithName));
    }
    
    [Test]
    public void DoubleIndexWithoutNameTest()
    {
      AssertBuildSuccess(typeof (DoubleIndexWithoutName));
    }

    [Test]
    public void InterfaceSupportTest()
    {
      AssertBuildSuccess(typeof(InterfaceSupport));
    }

    [Test]
    public void InheritanceClassTableTest()
    {
      AssertBuildFailure(typeof (InheritanceClassTable));
    }

    [Test]
    public void InheritanceSingleTableTest()
    {
      AssertBuildSuccess(typeof(InheritanceSingleTable));
    }

    [Test]
    public void InheritanceConcreteTableTest()
    {
      AssertBuildSuccess(typeof(InheritanceConcreteTable));
    }

    [Test]
    public void ValidateTest()
    {
      var types = typeof (TestBase).Assembly
        .GetTypes()
        .Where(type => type.Namespace==typeof (TestBase).Namespace && type!=typeof (InheritanceClassTable))
        .ToList();
      BuildDomain(types, DomainUpgradeMode.Recreate);
      BuildDomain(types, DomainUpgradeMode.Validate);
    }
  }
}