// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Globalization;
using System.Threading;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Xtensive.Orm.Localization.Configuration;

namespace Xtensive.Orm.Localization.Tests.Configuration
{
  public sealed class JsonConfigurationTest : MicrosoftConfigurationTest
  {
    protected override ConfigTypes ConfigFormat => ConfigTypes.Json;

    protected override void AddConfigurationFile(IConfigurationBuilder configurationBuilder)
    {
      _ = configurationBuilder.AddJsonFile("localizationsettings.json");
    }
  }

  public sealed class XmlConfigurationTest : MicrosoftConfigurationTest
  {
    protected override ConfigTypes ConfigFormat => ConfigTypes.Xml;

    protected override void AddConfigurationFile(IConfigurationBuilder configurationBuilder)
    {
      _ = configurationBuilder.AddXmlFile("LocalizationSettings.config");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributeEmptyNameTest(bool useRoot)
    {
      var locConfig = LoadConfiguration("Xtensive.Orm.Localization.NameAttribute.NameEmpty", useRoot);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributeAbsentTest(bool useRoot)
    {
      var locConfig = LoadConfiguration("Xtensive.Orm.Localization.NameAttribute.None", useRoot);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributeDefinedTest(bool useRoot)
    {
      var locConfig = LoadConfiguration("Xtensive.Orm.Localization.NameAttribute.Defined", useRoot);
      Assert.That(locConfig, Is.Not.Null);
      Assert.That(locConfig.DefaultCulture, Is.EqualTo(expectedCulture));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributeFaultyValueTest(bool useRoot)
    {
      var locConfig = LoadConfiguration("Xtensive.Orm.Localization.NameAttribute.FaultyValue", useRoot);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributeLowCase(bool useRoot)
    {
      var locConfig = LoadConfiguration("Xtensive.Orm.Localization.NameAttribute.LC", useRoot);
      Assert.That(locConfig, Is.Not.Null);
      Assert.That(locConfig.DefaultCulture, Is.EqualTo(expectedCulture));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributeUpperCase(bool useRoot)
    {
      var locConfig = LoadConfiguration("Xtensive.Orm.Localization.NameAttribute.UC", useRoot);
      Assert.That(locConfig, Is.Not.Null);
      Assert.That(locConfig.DefaultCulture, Is.EqualTo(expectedCulture));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributePascalCase(bool useRoot)
    {
      var locConfig = LoadConfiguration("Xtensive.Orm.Localization.NameAttribute.PC", useRoot);
      Assert.That(locConfig, Is.Not.Null);
      Assert.That(locConfig.DefaultCulture, Is.EqualTo(expectedCulture));
    }
  }

  [TestFixture]
  public abstract class MicrosoftConfigurationTest : TestCommon.MicrosoftConfigurationTestBase
  {
    protected readonly CultureInfo defaultCulture = new CultureInfo("en-US");
    protected readonly CultureInfo expectedCulture = new CultureInfo("es-ES");

    private CultureInfo resetCulture;

    public override void BeforeAllTestsSetUp()
    {
      base.BeforeAllTestsSetUp();
      
    }

    protected LocalizationConfiguration LoadConfiguration(string sectionName, bool useRoot)
    {
      return useRoot
        ? LocalizationConfiguration.Load(configurationRoot, sectionName)
        : LocalizationConfiguration.Load(configurationRoot.GetSection(sectionName));
    }

    [SetUp]
    public void SetUp()
    {
      resetCulture = Thread.CurrentThread.CurrentCulture;
      Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
    }

    [TearDown]
    public void TearDown()
    {
      Thread.CurrentThread.CurrentCulture = resetCulture;
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void EmptySectionCase(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.Empty", useRoot);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void EmptyName(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.NameEmpty", useRoot);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameDefined(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.NameDefined", useRoot);
      Assert.That(locConfig, Is.Not.Null);
      Assert.That(locConfig.DefaultCulture, Is.EqualTo(expectedCulture));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void FaultyValue(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.FaultyValue", useRoot);
      CheckConfigurationIsDefault(locConfig);
    }

    #region Naming

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingInLowCase(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.Naming.LC", useRoot);
      ValidateNamingConfigurationResults(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingInUpperCase(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.Naming.UC", useRoot);
      ValidateNamingConfigurationResults(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingInCamelCase(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.Naming.CC", useRoot);
      ValidateNamingConfigurationResults(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingInPascalCase(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.Naming.PC", useRoot);
      ValidateNamingConfigurationResults(locConfig);
    }

    private void ValidateNamingConfigurationResults(LocalizationConfiguration locConfig)
    {
      Assert.That(locConfig, Is.Not.Null);
      Assert.That(locConfig.DefaultCulture, Is.EqualTo(expectedCulture));
    }

    #endregion

    #region mistype cases

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MistypeInLowCase(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.Mistype.LC", useRoot);
      ValidateMistypeConfigurationResults(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MistypeInUpperCase(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.Mistype.UC", useRoot);
      ValidateMistypeConfigurationResults(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MistypeInCamelCase(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.Mistype.CC", useRoot);
      ValidateMistypeConfigurationResults(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MistypeInPascalCase(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.Mistype.PC", useRoot);
      ValidateMistypeConfigurationResults(locConfig);
    }

    private void ValidateMistypeConfigurationResults(LocalizationConfiguration locConfig)
    {
      CheckConfigurationIsDefault(locConfig);
    }

    #endregion

    #region Name as node

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NoNameNodes(bool useRoot)
    {
      IgnoreIfXml();

      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.Empty", useRoot);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameNodeIsEmpty(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.NameEmpty", useRoot);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void DefinedNameNode(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.Espaniol", useRoot);
      Assert.That(locConfig, Is.Not.Null);
      Assert.That(locConfig.DefaultCulture, Is.EqualTo(expectedCulture));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void FaultyNameNodeValue(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.FaultyValue", useRoot);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameNodeInLowCase(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.Naming.LC", useRoot);
      ValidateNamingConfigurationResults(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameNodeInUpperCase(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.Naming.UC", useRoot);
      ValidateNamingConfigurationResults(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameNodeInCamelCase(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.Naming.CC", useRoot);
      ValidateNamingConfigurationResults(locConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameNodeInPascalCase(bool useRoot)
    {
      var locConfig = LoadConfiguration($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.Naming.PC", useRoot);
      ValidateNamingConfigurationResults(locConfig);
    }

    #endregion

    protected void CheckConfigurationIsDefault(LocalizationConfiguration locConfig)
    {
      Assert.That(locConfig, Is.Not.Null);
      Assert.That(locConfig.DefaultCulture, Is.EqualTo(defaultCulture));
    }
  }
}
