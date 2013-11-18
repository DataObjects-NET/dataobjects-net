// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.11.14

using System;
using System.Reflection;
using NUnit.Framework;
using noFTModel = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel1;
using simpleFTModel = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel2;
using complexFTModel = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel3;
using wrongModel1 = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel4;
using wrongModel2 = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel5;
using wrongModel3 = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel6;
using wrongModel4 = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel7;

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel1
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Title { get; set; }

    [Field(Length = 8001)]
    public byte[] DocumentBody { get; set; }

    [Field(Length = 50)]
    public string Extension { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel2
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, FullText("English")]
    public string Title { get; set; }

    [Field(Length = 8001)]
    public byte[] DocumentBody { get; set; }

    [Field(Length = 50)]
    public string Extension { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel3
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, FullText("English")]
    public string Title { get; set; }

    [Field(Length = 8001)]
    [FullText("English", "Extension")]
    public byte[] DocumentBody { get; set; }

    [Field(Length = 50)]
    public string Extension { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel4
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, Key(Position = 1)]
    public int Id2 { get; set; }

    [Field, FullText("English")]
    public string Title { get; set; }

    [Field(Length = 8001)]
    [FullText("English", "Extension")]
    public byte[] DocumentBody { get; set; }

    [Field(Length = 50)]
    public string Extension { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel5
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, FullText("English")]
    public string Title { get; set; }

    [Field(Length = 8001)]
    [FullText("English", "Extension")]
    public string DocumentBody { get; set; }

    [Field(Length = 50)]
    public string Extension { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel6
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, FullText("English")]
    public string Title { get; set; }

    [Field(Length = 8001)]
    [FullText("English", "Extension")]
    public byte[] DocumentBody { get; set; }

    [Field]
    public string Extension { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel7
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, FullText("English")]
    public string Title { get; set; }

    [Field(Length = 8001)]
    [FullText("English", "Extension")]
    public byte[] DocumentBody { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTest
{
  [TestFixture]
  class FullTextDataTypeColumnUpgradeTest : AutoBuildTest
  {
    [TestFixtureSetUp]
    public void SetUp()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      Require.ProviderVersionAtLeast(StorageProviderVersion.SqlServer2005);
    }

    [Test]
    public void BuildFullTextModelInRecreateModeTest()
    {
      Assert.DoesNotThrow(
        () => {
          BuildDomain(DomainUpgradeMode.Recreate, typeof (complexFTModel.Document).Assembly, typeof (complexFTModel.Document).Namespace);
          BuildDomain(DomainUpgradeMode.Validate, typeof (complexFTModel.Document).Assembly, typeof (complexFTModel.Document).Namespace);
        });
    }

    [Test]
    public void PerformUpgradeToFullTextModelTest()
    {
      Assert.DoesNotThrow(
        () => {
          BuildDomain(DomainUpgradeMode.Recreate, typeof(noFTModel.Document).Assembly, typeof(noFTModel.Document).Namespace);
          BuildDomain(DomainUpgradeMode.Perform, typeof(complexFTModel.Document).Assembly, typeof(complexFTModel.Document).Namespace);
        });
    }

    [Test]
    public void PerformSafelyUpgradeToFullTextModelTest()
    {
      Assert.DoesNotThrow(
        () => {
          BuildDomain(DomainUpgradeMode.Recreate, typeof(noFTModel.Document).Assembly, typeof(noFTModel.Document).Namespace);
          BuildDomain(DomainUpgradeMode.PerformSafely, typeof(complexFTModel.Document).Assembly, typeof(complexFTModel.Document).Namespace);
        });
    }

    [Test]
    public void PerformUpgrageFullTextModelToFullTextTModelWithTypeCplomnTest()
    {
      Assert.DoesNotThrow(
        () => {
          BuildDomain(DomainUpgradeMode.Recreate, typeof(simpleFTModel.Document).Assembly, typeof(simpleFTModel.Document).Namespace);
          BuildDomain(DomainUpgradeMode.Perform, typeof(complexFTModel.Document).Assembly, typeof(complexFTModel.Document).Namespace);
        });
    }

    [Test]
    public void PerformSafelyUpgrageFullTextModelToFullTextTModelWithTypeCplomnTest()
    {
      Assert.DoesNotThrow(
        () => {
          BuildDomain(DomainUpgradeMode.Recreate, typeof(simpleFTModel.Document).Assembly, typeof(simpleFTModel.Document).Namespace);
          BuildDomain(DomainUpgradeMode.PerformSafely, typeof(complexFTModel.Document).Assembly, typeof(complexFTModel.Document).Namespace);
        });
    }

    [Test]
    public void PrimaryIndexContainsTwoColumnsTest()
    {
      Assert.Throws<StorageException>(
        () => BuildDomain(DomainUpgradeMode.Recreate, typeof (wrongModel1.Document).Assembly, typeof (wrongModel1.Document).Namespace));
    }

    [Test]
    public void WrongTypeOfFullTextColumnTest()
    {
      Assert.Throws<StorageException>(
        () => BuildDomain(DomainUpgradeMode.Recreate, typeof (wrongModel2.Document).Assembly, typeof (wrongModel2.Document).Namespace));
    }

    [Test]
    public void WrongTypeOfTypeColumnTest()
    {
      Assert.Throws<StorageException>(
        () => BuildDomain(DomainUpgradeMode.Recreate, typeof (wrongModel3.Document).Assembly, typeof (wrongModel3.Document).Namespace));
    }

    [Test]
    public void TypeColumnIsNotExistsInEntity()
    {
      Assert.Throws<ArgumentException>(
        () => BuildDomain(DomainUpgradeMode.Recreate, typeof (wrongModel4.Document).Assembly, typeof (wrongModel4.Document).Namespace));
    }

    private Domain BuildDomain(DomainUpgradeMode mode, Assembly assembly, string @namespace)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.Types.Register(assembly, @namespace);
      return Domain.Build(configuration);
    }
  }
}
