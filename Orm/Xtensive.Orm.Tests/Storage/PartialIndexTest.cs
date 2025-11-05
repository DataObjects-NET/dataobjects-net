// Copyright (C) 2011-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
using NUnit.Framework.Interfaces;

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

  [HierarchyRoot, Index(nameof(TestField), Filter = nameof(Index))]
  public class SimpleFilterWithMethod : TestBase
  {
    public static Expression<Func<SimpleFilterWithMethod, bool>> Index() =>
      test => test.TestField.GreaterThan("hello world");

    [Field]
    public string TestField { get; set; }
  }

  [HierarchyRoot, Index(nameof(TestField), Filter = nameof(Index))]
  public class SimpleFilterWithProperty : TestBase
  {
    public static Expression<Func<SimpleFilterWithProperty, bool>> Index =>
      test => test.TestField.GreaterThan("hello world");

    [Field]
    public string TestField { get; set; }
  }

  [HierarchyRoot, Index(nameof(Target), Filter = nameof(Index))]
  public class FilterOnReferenceField1 : TestBase
  {
    public static Expression<Func<FilterOnReferenceField1, bool>> Index() =>
      test => test.Target != null;

    [Field]
    public TargetEntity Target { get; set; } 
  }

  [HierarchyRoot, Index(nameof(Target), Filter = nameof(Index))]
  public class FilterOnReferenceField2 : TestBase
  {
    public static Expression<Func<FilterOnReferenceField2, bool>> Index() =>
      test => test.Target == null;

    [Field]
    public TargetEntity Target { get; set; }
  }

  [HierarchyRoot, Index(nameof(Target), Filter = nameof(Index))]
  public class FilterOnReferenceField3 : TestBase
  {
    public static Expression<Func<FilterOnReferenceField3, bool>> Index() =>
      test => test.Target == test.Alien;

    [Field]
    public TargetEntity Target { get; set; }

    [Field]
    public TargetEntity Alien { get; set; }
  }

  [HierarchyRoot, Index(nameof(Target), Filter = nameof(Index))]
  public class FilterOnReferenceField4 : TestBase
  {
    public static Expression<Func<FilterOnReferenceField4, bool>> Index() =>
      test => test.Target != test.Alien;

    [Field]
    public TargetEntity Target { get; set; }

    [Field]
    public TargetEntity Alien { get; set; }
  }

  [HierarchyRoot, Index(nameof(Target), Filter = nameof(Index))]
  public class FilterOnComplexReferenceField1 : TestBase
  {
    public static Expression<Func<FilterOnComplexReferenceField1, bool>> Index() =>
      test => test.Target != null;

    [Field]
    public ComplexTargetEntity Target { get; set; }
  }

  [HierarchyRoot, Index(nameof(Target), Filter = nameof(Index))]
  public class FilterOnComplexReferenceField2 : TestBase
  {
    public static Expression<Func<FilterOnComplexReferenceField2, bool>> Index() =>
      test => test.Target == null;

    [Field]
    public ComplexTargetEntity Target { get; set; }
  }

  [HierarchyRoot, Index(nameof(Target), Filter = nameof(Index))]
  public class FilterOnComplexReferenceField3 : TestBase
  {
    public static Expression<Func<FilterOnComplexReferenceField3, bool>> Index() =>
      test => test.Target == test.Alien;

    [Field]
    public ComplexTargetEntity Target { get; set; }

    [Field]
    public ComplexTargetEntity Alien { get; set; }
  }

  [HierarchyRoot, Index(nameof(Target), Filter = nameof(Index))]
  public class FilterOnComplexReferenceField4 : TestBase
  {
    public static Expression<Func<FilterOnComplexReferenceField4, bool>> Index() =>
      test => test.Target != test.Alien;

    [Field]
    public ComplexTargetEntity Target { get; set; }

    [Field]
    public ComplexTargetEntity Alien { get; set; }
  }

  [HierarchyRoot, Index(nameof(Target), Filter = nameof(Index))]
  public class FilterOnReferenceIdField : TestBase
  {
    public static Expression<Func<FilterOnReferenceIdField, bool>> Index() =>
      test => test.Target.Id > 0;

    [Field]
    public TargetEntity Target { get; set; }
  }

  [HierarchyRoot, Index(nameof(TestField1), Filter = nameof(Index))]
  public class FilterOnAlienField : TestBase
  {
    public static Expression<Func<FilterOnAlienField, bool>> Index =>
      test => test.TestField2.GreaterThan("hello world");

    [Field]
    public string TestField1 { get; set; }

    [Field]
    public string TestField2 { get; set; }
  }

  [HierarchyRoot, Index(nameof(TestField), Filter = nameof(Index))]
  public class MultipleFieldUses : TestBase
  {
    public static Expression<Func<MultipleFieldUses, bool>> Index =>
      test => test.TestField.GreaterThan("hello") && test.TestField.LessThan("world");

    [Field]
    public string TestField { get; set; }
  }

  [HierarchyRoot, Index(nameof(TestField), Filter = nameof(Index))]
  public class InterfaceSupport : TestBase, ITestInterface
  {
    public static Expression<Func<InterfaceSupport, bool>> Index =>
      test => test.TestField.LessThan("bye world");

    [Field]
    public string TestField { get; set; }
  }

  [HierarchyRoot, Index(nameof(TestField), Filter = nameof(Index))]
  public class InOperatorSupport : TestBase
  {
    public static Expression<Func<InOperatorSupport, bool>> Index =>
      test => test.TestField.In("1", "2", "3");

    [Field]
    public string TestField { get; set; }
  }

  [HierarchyRoot, Index(nameof(TestField), Filter = nameof(Index))]
  public class InOperatorSupport2 : TestBase
  {
    public static Expression<Func<InOperatorSupport2, bool>> Index =>
      test => test.TestField.In(
        new Guid("{D27F71D7-D4FC-446C-8C7E-E89DC2209863}"),
        new Guid("{94ED80D9-6749-41E2-B60D-BEDE1CDCA237}"));

    [Field]
    public Guid TestField { get; set; }
  }

  [HierarchyRoot, Index(nameof(TestField), Filter = nameof(Index))]
  public class ContainsOperatorSupport : TestBase
  {
    public static Expression<Func<ContainsOperatorSupport, bool>> Index =>
      test => new[] { "1", "2", "3" }.Contains(test.TestField);

    [Field]
    public string TestField { get; set; }
  }

  [HierarchyRoot, Index(nameof(TestField), Filter = nameof(Index))]
  public class ConditionalExpressionSuport : TestBase
  {
    public static Expression<Func<ConditionalExpressionSuport, bool>> Index =>
      test => test.Id == 100 ? test.TestField.Contains("hello") : false;

    [Field]
    public string TestField { get; set; }
  }

  [HierarchyRoot,
    Index(nameof(TestField), Filter = nameof(More), Name = "MoreIndex"),
    Index(nameof(TestField), Filter = nameof(Less), Name = "LessIndex")]
  public class DoubleIndexWithName : TestBase
  {
    public static Expression<Func<DoubleIndexWithName, bool>> More() =>
      test => test.TestField > 1000;

    public static Expression<Func<DoubleIndexWithName, bool>> Less() =>
      test => test.TestField < 100;

    [Field]
    public int TestField { get; set; }
  }

  [HierarchyRoot,
    Index(nameof(TestField), Filter = nameof(More)),
    Index(nameof(TestField), Filter = nameof(Less))]
  public class DoubleIndexWithoutName : TestBase
  {
    public static Expression<Func<DoubleIndexWithoutName, bool>> More() =>
      test => test.TestField > 1000;

    public static Expression<Func<DoubleIndexWithoutName, bool>> Less() =>
      test => test.TestField < 100;

    [Field]
    public int TestField { get; set; }
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
  public class InheritanceClassTableBase : TestBase
  {
    [Field]
    public int BaseField { get; set; }
  }

  [Index(nameof(TestField), Filter = nameof(Index))]
  public class InheritanceClassTable : InheritanceClassTableBase
  {
    public static Expression<Func<InheritanceClassTable, bool>> Index() =>
      test => test.BaseField > 0;

    [Field]
    public int TestField { get; set; }
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.SingleTable)]
  public class InheritanceSingleTableBase : TestBase
  {
    [Field]
    public int BaseField { get; set; }
  }

  [Index(nameof(TestField), Filter = nameof(Index))]
  public class InheritanceSingleTable : InheritanceSingleTableBase
  {
    public static Expression<Func<InheritanceSingleTable, bool>> Index() =>
      test => test.BaseField > 0;

    [Field]
    public int TestField { get; set; }
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
  public class InheritanceConcreteTableBase : TestBase
  {
    [Field]
    public int BaseField { get; set; }
  }

  [Index(nameof(TestField), Filter = nameof(Index))]
  public class InheritanceConcreteTable : InheritanceConcreteTableBase
  {
    public static Expression<Func<InheritanceConcreteTable, bool>> Index() =>
      test => test.BaseField > 0;

    [Field]
    public int TestField { get; set; }
  }

  #region Enums
  public enum ByteEnum : byte
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = byte.MaxValue
  }

  public enum SByteEnum : sbyte
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = sbyte.MaxValue
  }

  public enum ShortEnum : short
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = short.MaxValue
  }

  public enum UShortEnum : ushort
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = ushort.MaxValue
  }

  public enum IntEnum : int
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = int.MaxValue
  }

  public enum UIntEnum : uint
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = uint.MaxValue
  }

  public enum LongEnum : long
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = long.MaxValue
  }

  public enum ULongEnum : ulong
  {
    Zero = 0,
    One = 1,
    Two = 2,
    Max = ulong.MaxValue
  }
  #endregion

  [HierarchyRoot]
  [Index(nameof(ByteEnumField), Filter = nameof(ByteIndex))]
  [Index(nameof(NByteEnumField), Filter = nameof(NByteIndex))]
  [Index(nameof(SByteEnumField), Filter = nameof(SByteIndex))]
  [Index(nameof(NSByteEnumField), Filter = nameof(SNByteIndex))]
  [Index(nameof(ShortEnumField), Filter = nameof(ShortIndex))]
  [Index(nameof(NShortEnumField), Filter = nameof(NShortIndex))]
  [Index(nameof(UShortEnumField), Filter = nameof(UShortIndex))]
  [Index(nameof(NUShortEnumField), Filter = nameof(NUShortIndex))]
  [Index(nameof(IntEnumField), Filter = nameof(IntIndex))]
  [Index(nameof(NIntEnumField), Filter = nameof(NIntIndex))]
  [Index(nameof(UIntEnumField), Filter = nameof(UIntIndex))]
  [Index(nameof(NUIntEnumField), Filter = nameof(NUIntIndex))]
  [Index(nameof(LongEnumField), Filter = nameof(LongIndex))]
  [Index(nameof(NLongEnumField), Filter = nameof(NLongIndex))]
  [Index(nameof(ULongEnumField), Filter = nameof(ULongIndex))]
  [Index(nameof(NULongEnumField), Filter = nameof(NULongIndex))]
  public class EnumFieldFilter : Entity
  {
    public const string Zero = "Zero";
    public const string One = "One";
    public const string Two = "Two";
    public const string Max = "Max";

    #region Filters
    public static Expression<Func<EnumFieldFilter, bool>> ByteIndex() =>
      test => test.ByteEnumField > ByteEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> NByteIndex() =>
      test => test.NByteEnumField > ByteEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> SByteIndex() =>
      test => test.ByteEnumField > ByteEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> SNByteIndex() =>
      test => test.ByteEnumField > ByteEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> ShortIndex() =>
      test => test.ShortEnumField > ShortEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> NShortIndex() =>
      test => test.NShortEnumField > ShortEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> UShortIndex() =>
      test => test.UShortEnumField > UShortEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> NUShortIndex() =>
      test => test.NUShortEnumField > UShortEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> IntIndex() =>
      test => test.IntEnumField > IntEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> NIntIndex() =>
      test => test.NIntEnumField > IntEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> UIntIndex() =>
      test => test.UIntEnumField > UIntEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> NUIntIndex() =>
      test => test.NUIntEnumField > UIntEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> LongIndex() =>
      test => test.LongEnumField > LongEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> NLongIndex() =>
      test => test.NLongEnumField > LongEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> ULongIndex() =>
      test => test.ULongEnumField > ULongEnum.Two;

    public static Expression<Func<EnumFieldFilter, bool>> NULongIndex() =>
      test => test.NULongEnumField > ULongEnum.Two;

    #endregion

    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public ByteEnum ByteEnumField { get; set; }

    [Field]
    public SByteEnum SByteEnumField { get; set; }

    [Field]
    public ShortEnum ShortEnumField { get; set; }

    [Field]
    public UShortEnum UShortEnumField { get; set; }

    [Field]
    public IntEnum IntEnumField { get; set; }

    [Field]
    public UIntEnum UIntEnumField { get; set; }

    [Field]
    public LongEnum LongEnumField { get; set; }

    [Field]
    public ULongEnum ULongEnumField { get; set; }

    [Field]
    public ByteEnum? NByteEnumField { get; set; }

    [Field]
    public SByteEnum? NSByteEnumField { get; set; }

    [Field]
    public ShortEnum? NShortEnumField { get; set; }

    [Field]
    public UShortEnum? NUShortEnumField { get; set; }

    [Field]
    public IntEnum? NIntEnumField { get; set; }

    [Field]
    public UIntEnum? NUIntEnumField { get; set; }

    [Field]
    public LongEnum? NLongEnumField { get; set; }

    [Field]
    public ULongEnum? NULongEnumField { get; set; }

    public EnumFieldFilter(Session session, string enumValue)
      : base(session)
    {
      ByteEnumField = (ByteEnum) Enum.Parse(typeof(ByteEnum), enumValue);
      SByteEnumField = (SByteEnum) Enum.Parse(typeof(SByteEnum), enumValue);
      ShortEnumField = (ShortEnum) Enum.Parse(typeof(ShortEnum), enumValue);
      UShortEnumField = (UShortEnum) Enum.Parse(typeof(UShortEnum), enumValue);
      IntEnumField = (IntEnum) Enum.Parse(typeof(IntEnum), enumValue);
      UIntEnumField = (UIntEnum) Enum.Parse(typeof(UIntEnum), enumValue);
      LongEnumField = (LongEnum) Enum.Parse(typeof(LongEnum), enumValue);
      ULongEnumField = (ULongEnum) Enum.Parse(typeof(ULongEnum), enumValue);

      NByteEnumField = (ByteEnum) Enum.Parse(typeof(ByteEnum), enumValue);
      NSByteEnumField = (SByteEnum) Enum.Parse(typeof(SByteEnum), enumValue);
      NShortEnumField = (ShortEnum) Enum.Parse(typeof(ShortEnum), enumValue);
      NUShortEnumField = (UShortEnum) Enum.Parse(typeof(UShortEnum), enumValue);
      NIntEnumField = (IntEnum) Enum.Parse(typeof(IntEnum), enumValue);
      NUIntEnumField = (UIntEnum) Enum.Parse(typeof(UIntEnum), enumValue);
      NLongEnumField = (LongEnum) Enum.Parse(typeof(LongEnum), enumValue);
      NULongEnumField = (ULongEnum) Enum.Parse(typeof(ULongEnum), enumValue);
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class PartialIndexTest
  {
    private Domain domain;

    [OneTimeSetUp]
    public void TestFixtureSetUp() =>
      Require.AllFeaturesSupported(ProviderFeatures.PartialIndexes);

    [TearDown]
    public void TearDown() => CleanDomain();

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
      foreach (var entity in entities) {
        config.Types.Register(entity);
      }

      config.UpgradeMode = mode;
      domain = Domain.Build(config);
    }

    private void AssertBuildSuccess(params Type[] entities)
    {
      BuildDomain(entities, DomainUpgradeMode.Recreate);
      var partialIndexes = domain.Model.RealIndexes
        .Where(index => index.IsPartial && index.FilterExpression != null && index.Filter != null)
        .ToList();
      Assert.IsNotEmpty(partialIndexes);
    }

    private void AssertBuildFailure(params Type[] entities)
    {
      AssertEx.Throws<DomainBuilderException>(() => BuildDomain(entities, DomainUpgradeMode.Recreate));
    }

    [Test]
    public void SimpleFilterWithMethodTest() => AssertBuildSuccess(typeof(SimpleFilterWithMethod));

    [Test]
    public void SimpleFilterWithPropertyTest() => AssertBuildSuccess(typeof(SimpleFilterWithProperty));

    [Test]
    public void FilterOnReferenceFieldTest1() => AssertBuildSuccess(typeof(FilterOnReferenceField1));

    [Test]
    public void FilterOnReferenceFieldTest2() => AssertBuildSuccess(typeof(FilterOnReferenceField2));

    [Test]
    public void FilterOnReferenceFieldTest3() => AssertBuildFailure(typeof(FilterOnReferenceField3));

    [Test]
    public void FilterOnReferenceFieldTest4() => AssertBuildFailure(typeof(FilterOnReferenceField4));

    [Test]
    public void FilterOnComplexReferenceFieldTest1() => AssertBuildSuccess(typeof(FilterOnComplexReferenceField1));

    [Test]
    public void FilterOnComplexReferenceFieldTest2() => AssertBuildSuccess(typeof(FilterOnComplexReferenceField2));

    [Test]
    public void FilterOnComplexReferenceFieldTest3() => AssertBuildFailure(typeof(FilterOnComplexReferenceField3));

    [Test]
    public void FilterOnComplexReferenceFieldTest4() => AssertBuildFailure(typeof(FilterOnComplexReferenceField4));

    [Test]
    public void FilterOnReferenceFieldIdTest() => AssertBuildSuccess(typeof(FilterOnReferenceIdField));

    [Test]
    public void FilterOnAlienFieldTest() => AssertBuildSuccess(typeof(FilterOnAlienField));

    [Test]
    public void MultipleFieldUsesTest() => AssertBuildSuccess(typeof(MultipleFieldUses));

    [Test]
    public void InOperatorSupportTest() => AssertBuildSuccess(typeof(InOperatorSupport));

    [Test]
    public void InOperatorSupport2Test() => AssertBuildSuccess(typeof(InOperatorSupport2));

    [Test]
    public void ContainsOperatorSupportTest() => AssertBuildSuccess(typeof(ContainsOperatorSupport));

    [Test, RequirePostgreSql]
    public void ConditionalOperationSupportTest() => AssertBuildSuccess(typeof(ConditionalExpressionSuport));

    [Test]
    public void DoubleIndexWithNameTest() => AssertBuildSuccess(typeof(DoubleIndexWithName));

    [Test]
    public void DoubleIndexWithoutNameTest() => AssertBuildSuccess(typeof(DoubleIndexWithoutName));

    [Test]
    public void InterfaceSupportTest() => AssertBuildSuccess(typeof(InterfaceSupport));

    [Test]
    public void InheritanceClassTableTest() => AssertBuildFailure(typeof(InheritanceClassTable));

    [Test]
    public void InheritanceSingleTableTest() => AssertBuildSuccess(typeof(InheritanceSingleTable));

    [Test]
    public void InheritanceConcreteTableTest() => AssertBuildSuccess(typeof(InheritanceConcreteTable));

    [Test]
    public void EnumFieldFilterTest() => AssertBuildSuccess(typeof(EnumFieldFilter));

    [Test, RequirePostgreSql]
    public void ValidateTestForPgSql()
    {
      var types = typeof(TestBase).Assembly
        .GetTypes()
        .Where(type => type.Namespace == typeof(TestBase).Namespace
          && type != typeof(InheritanceClassTable)
          && type != typeof(FilterOnReferenceField3)
          && type != typeof(FilterOnReferenceField4)
          && type != typeof(FilterOnComplexReferenceField3)
          && type != typeof(FilterOnComplexReferenceField4))
        .ToList();
      BuildDomain(types, DomainUpgradeMode.Recreate);
      BuildDomain(types, DomainUpgradeMode.Validate);
    }

    [Test, RequireSqlServer]
    public void ValidateTestForSqlServer()
    {
      var types = typeof(TestBase).Assembly
        .GetTypes()
        .Where(type => type.Namespace == typeof(TestBase).Namespace
          && type != typeof(InheritanceClassTable)
          && type != typeof(ConditionalExpressionSuport)
          && type != typeof(FilterOnReferenceField3)
          && type != typeof(FilterOnReferenceField4)
          && type != typeof(FilterOnComplexReferenceField3)
          && type != typeof(FilterOnComplexReferenceField4))
        .ToList();
      BuildDomain(types, DomainUpgradeMode.Recreate);
      BuildDomain(types, DomainUpgradeMode.Validate);
    }
  }
}