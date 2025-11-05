// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Xtensive.Orm.Reprocessing.Configuration;

namespace Xtensive.Orm.Reprocessing.Tests.Configuration
{
  public sealed class JsonConfigurationTest : MicrosoftConfigurationTest
  {
    protected override ConfigTypes ConfigFormat => ConfigTypes.Json;

    protected override void AddConfigurationFile(IConfigurationBuilder configurationBuilder)
    {
      _ = configurationBuilder.AddJsonFile("reprocessingsettings.json");
    }
  }

  public sealed class XmlConfigurationTest : MicrosoftConfigurationTest
  {
    protected override ConfigTypes ConfigFormat => ConfigTypes.Xml;

    protected override void AddConfigurationFile(IConfigurationBuilder configurationBuilder)
    {
      _ = configurationBuilder.AddXmlFile("ReprocessingSettings.config");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void EmptyNodesTest(bool useRoot)
    {
      var repConfig = LoadConfiguration("Xtensive.Orm.Reprocessing.Xml.EmptyNodes", useRoot);
      CheckConfigIsDefault(repConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void EmptyValuesTest(bool useRoot)
    {
      var repConfig = LoadConfiguration("Xtensive.Orm.Reprocessing.Attributes.EmptyValues", useRoot);
      CheckConfigIsDefault(repConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void EmptyTransactionModeOnlyTest(bool useRoot)
    {
      var repConfig = LoadConfiguration("Xtensive.Orm.Reprocessing.Attributes.EmptyTMOnly", useRoot);
      CheckConfigIsDefault(repConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void EmptyStrategyOnlyTest(bool useRoot)
    {
      var repConfig = LoadConfiguration("Xtensive.Orm.Reprocessing.Attributes.EmptyStrategyOnly", useRoot);
      CheckConfigIsDefault(repConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void AllAttributesHasValuesTest(bool useRoot)
    {
      var repConfig = LoadConfiguration("Xtensive.Orm.Reprocessing.Attributes.All", useRoot);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Auto));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleUniqueConstraintViolationStrategy)));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyTMAttributeHasValueTest(bool useRoot)
    {
      var repConfig = LoadConfiguration("Xtensive.Orm.Reprocessing.Attributes.OnlyTM", useRoot);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Auto));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleReprocessableExceptionStrategy)));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyStrategyAttributeHasValueTest(bool useRoot)
    {
      var repConfig = LoadConfiguration("Xtensive.Orm.Reprocessing.Attributes.OnlyStrategy", useRoot);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.New));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleUniqueConstraintViolationStrategy)));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void AttributesInLowCase(bool useRoot)
    {
      var repConfig = LoadConfiguration("Xtensive.Orm.Reprocessing.Attributes.LC", useRoot);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Auto));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleUniqueConstraintViolationStrategy)));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void AttributesInUpperCase(bool useRoot)
    {
      var repConfig = LoadConfiguration("Xtensive.Orm.Reprocessing.Attributes.UC", useRoot);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Auto));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleUniqueConstraintViolationStrategy)));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void AttributesInPascalCase(bool useRoot)
    {
      var repConfig = LoadConfiguration("Xtensive.Orm.Reprocessing.Attributes.PC", useRoot);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Auto));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleUniqueConstraintViolationStrategy)));
    }
  }

  public abstract class MicrosoftConfigurationTest : TestCommon.MicrosoftConfigurationTestBase
  {
    protected ReprocessingConfiguration LoadConfiguration(string sectionName, bool useRoot)
    {
      return useRoot
        ? ReprocessingConfiguration.Load(configurationRoot, sectionName)
        : ReprocessingConfiguration.Load(configurationRoot.GetSection(sectionName));
    }


    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void EmptySectionCase(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Empty", useRoot);
      CheckConfigIsDefault(repConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void EmptyNames(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.AllEmpty", useRoot);
      CheckConfigIsDefault(repConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyTransactionOpenModeAndEmpty(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyTM.Empty", useRoot);
      CheckConfigIsDefault(repConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyTransactionOpenModeAuto(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyTM.Auto", useRoot);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Auto));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleReprocessableExceptionStrategy)));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyTransactionOpenModeNew(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyTM.New", useRoot);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.New));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleReprocessableExceptionStrategy)));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyTransactionOpenModeDefault(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyTM.Default", useRoot);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.Default));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleReprocessableExceptionStrategy)));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyStrategyAndEmpty(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyStrategy.Empty", useRoot);
      CheckConfigIsDefault(repConfig);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.New));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleReprocessableExceptionStrategy)));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyStrategyHandleReprocessible(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyStrategy.HandleReprocessible", useRoot);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.New));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleReprocessableExceptionStrategy)));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyStrategyHandleUnique(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyStrategy.HandleUnique", useRoot);
      Assert.That(repConfig, Is.Not.Null);
      Assert.That(repConfig.DefaultTransactionOpenMode, Is.EqualTo(TransactionOpenMode.New));
      Assert.That(repConfig.DefaultExecuteStrategy, Is.EqualTo(typeof(HandleUniqueConstraintViolationStrategy)));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyStrategyNonExistent(bool useRoot)
    {
      _ = Assert.Throws<InvalidOperationException>(
        () => LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.OnlyStrategy.NonExistent", useRoot));
    }


    #region Naming

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingInLowCase(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Naming.LC", useRoot);
      ValidateNamingConfigurationResults(repConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingInUpperCase(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Naming.UC", useRoot);
      ValidateNamingConfigurationResults(repConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingInCamelCase(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Naming.CC", useRoot);
      ValidateNamingConfigurationResults(repConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingInPascalCase(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Naming.PC", useRoot);
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
    [TestCase(true)]
    [TestCase(false)]
    public void MistypeInLowCase(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Mistype.LC", useRoot);
      ValidateMistypeConfigurationResults(repConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MistypeInUpperCase(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Mistype.UC", useRoot);
      ValidateMistypeConfigurationResults(repConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MistypeInCamelCase(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Mistype.CC", useRoot);
      ValidateMistypeConfigurationResults(repConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MistypeInPascalCase(bool useRoot)
    {
      var repConfig = LoadConfiguration($"Xtensive.Orm.Reprocessing.{ConfigFormat}.Mistype.PC", useRoot);
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
