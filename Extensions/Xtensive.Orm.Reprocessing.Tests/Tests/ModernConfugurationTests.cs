// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Xtensive.Orm.Reprocessing.Configuration;

namespace Xtensive.Orm.Reprocessing.Tests.Configuration
{
  public sealed class JsonConfigurationTest : ModernConfigurationTest
  {
    protected override ConfigTypes ConfigFormat => ConfigTypes.Json;

    protected override void AddConfigurationFile(IConfigurationBuilder configurationBuilder)
    {
      _ = configurationBuilder.AddJsonFile("reprocessingsettings.json");
    }
  }

  public sealed class XmlConfigurationTest : ModernConfigurationTest
  {
    protected override ConfigTypes ConfigFormat => ConfigTypes.Xml;

    protected override void AddConfigurationFile(IConfigurationBuilder configurationBuilder)
    {
      _ = configurationBuilder.AddXmlFile("ReprocessingSettings.config");
    }

    [Test]
    public void EmptyNodesTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Reprocessing.Xml.EmptyNodes");
      var repConfig = ReprocessingConfiguration.Load(section);
      CheckConfigIsDefault(repConfig);
    }

    [Test]
    public void EmptyValuesTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Reprocessing.Attributes.EmptyValues");
      var repConfig = ReprocessingConfiguration.Load(section);
      CheckConfigIsDefault(repConfig);
    }

    [Test]
    public void EmptyTransactionModeOnlyTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Reprocessing.Attributes.EmptyTMOnly");
      var repConfig = ReprocessingConfiguration.Load(section);
      CheckConfigIsDefault(repConfig);
    }

    [Test]
    public void EmptyStrategyOnlyTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Reprocessing.Attributes.EmptyStrategyOnly");
      var repConfig = ReprocessingConfiguration.Load(section);
      CheckConfigIsDefault(repConfig);
    }

    [Test]
    public void AllAttributesHasValuesTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Reprocessing.Attributes.All");
      var repConfig = ReprocessingConfiguration.Load(section);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Auto));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleUniqueConstraintViolationStrategy)));
    }

    [Test]
    public void OnlyTMAttributeHasValueTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Reprocessing.Attributes.OnlyTM");
      var repConfig = ReprocessingConfiguration.Load(section);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Auto));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleReprocessableExceptionStrategy)));
    }

    [Test]
    public void OnlyStrategyAttributeHasValueTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Reprocessing.Attributes.OnlyStrategy");
      var repConfig = ReprocessingConfiguration.Load(section);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.New));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleUniqueConstraintViolationStrategy)));
    }

    [Test]
    public void AttributesInLowCase()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Reprocessing.Attributes.LC");
      var repConfig = ReprocessingConfiguration.Load(section);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Auto));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleUniqueConstraintViolationStrategy)));
    }

    [Test]
    public void AttributesInUpperCase()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Reprocessing.Attributes.UC");
      var repConfig = ReprocessingConfiguration.Load(section);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Auto));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleUniqueConstraintViolationStrategy)));
    }

    [Test]
    public void AttributesInPascalCase()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Reprocessing.Attributes.PC");
      var repConfig = ReprocessingConfiguration.Load(section);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Auto));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleUniqueConstraintViolationStrategy)));
    }
  }

  public abstract class ModernConfigurationTest : TestCommon.ModernConfigurationTestBase
  {
    [Test]
    public void EmptySectionCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Empty");
      var repConfig = ReprocessingConfiguration.Load(section);
      CheckConfigIsDefault(repConfig);
    }

    [Test]
    public void EmptyNames()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.AllEmpty");
      var repConfig = ReprocessingConfiguration.Load(section);
      CheckConfigIsDefault(repConfig);
    }

    [Test]
    public void OnlyTransactionOpenModeAndEmpty()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyTM.Empty");
      var repConfig = ReprocessingConfiguration.Load(section);
      CheckConfigIsDefault(repConfig);
    }

    [Test]
    public void OnlyTransactionOpenModeAuto()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyTM.Auto");
      var repConfig = ReprocessingConfiguration.Load(section);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Auto));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleReprocessableExceptionStrategy)));
    }

    [Test]
    public void OnlyTransactionOpenModeNew()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyTM.New");
      var repConfig = ReprocessingConfiguration.Load(section);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.New));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleReprocessableExceptionStrategy)));
    }

    [Test]
    public void OnlyTransactionOpenModeDefault()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyTM.Default");
      var repConfig = ReprocessingConfiguration.Load(section);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Default));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleReprocessableExceptionStrategy)));
    }

    [Test]
    public void OnlyStrategyAndEmpty()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyStrategy.Empty");
      var repConfig = ReprocessingConfiguration.Load(section);
      CheckConfigIsDefault(repConfig);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.New));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleReprocessableExceptionStrategy)));
    }

    [Test]
    public void OnlyStrategyHandleReprocessible()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyStrategy.HandleReprocessible");
      var repConfig = ReprocessingConfiguration.Load(section);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.New));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleReprocessableExceptionStrategy)));
    }

    [Test]
    public void OnlyStrategyHandleUnique()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyStrategy.HandleUnique");
      var repConfig = ReprocessingConfiguration.Load(section);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.New));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleUniqueConstraintViolationStrategy)));
    }

    [Test]
    public void OnlyStrategyNonExistent()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyStrategy.NonExistent");
      _ = Assert.Throws<InvalidOperationException>(() => ReprocessingConfiguration.Load(section));
    }


    #region Naming
    [Test]
    public void NamingInLowCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Naming.LC");
      var repConfig = ReprocessingConfiguration.Load(section);
      ValidateNamingConfigurationResults(repConfig);
    }

    [Test]
    public void NamingInUpperCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Naming.UC");
      var repConfig = ReprocessingConfiguration.Load(section);
      ValidateNamingConfigurationResults(repConfig);
    }

    [Test]
    public void NamingInCamelCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Naming.CC");
      var repConfig = ReprocessingConfiguration.Load(section);
      ValidateNamingConfigurationResults(repConfig);
    }

    [Test]
    public void NamingInPascalCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Naming.PC");
      var repConfig = ReprocessingConfiguration.Load(section);
      ValidateNamingConfigurationResults(repConfig);
    }

    private static void ValidateNamingConfigurationResults(ReprocessingConfiguration repConfig)
    {
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Auto));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleUniqueConstraintViolationStrategy)));
    }
    #endregion

    #region mistype cases
    [Test]
    public void MistypeInLowCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Mistype.LC");
      var repConfig = ReprocessingConfiguration.Load(section);
      ValidateMistypeConfigurationResults(repConfig);
    }

    [Test]
    public void MistypeInUpperCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Mistype.UC");
      var repConfig = ReprocessingConfiguration.Load(section);
      ValidateMistypeConfigurationResults(repConfig);
    }

    [Test]
    public void MistypeInCamelCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Mistype.CC");
      var repConfig = ReprocessingConfiguration.Load(section);
      ValidateMistypeConfigurationResults(repConfig);
    }

    [Test]
    public void MistypeInPascalCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Mistype.PC");
      var repConfig = ReprocessingConfiguration.Load(section);
      ValidateMistypeConfigurationResults(repConfig);
    }

    private static void ValidateMistypeConfigurationResults(ReprocessingConfiguration repConfig)
    {
      CheckConfigIsDefault(repConfig);
    }

    #endregion

    protected static void CheckConfigIsDefault(ReprocessingConfiguration configuration)
    {
      Assert.That(configuration, Is.Not.Null);
      Assert.That(configuration.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.New));
      Assert.That(configuration.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleReprocessableExceptionStrategy)));
    }
  }
}
