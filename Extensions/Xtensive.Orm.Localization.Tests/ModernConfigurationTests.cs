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
  public sealed class JsonConfigurationTest : ModernConfigurationTest
  {
    protected override ConfigTypes ConfigFormat => ConfigTypes.Json;

    protected override void AddConfigurationFile(IConfigurationBuilder configurationBuilder)
    {
      _ = configurationBuilder.AddJsonFile("localizationsettings.json");
    }
  }

  public sealed class XmlConfigurationTest : ModernConfigurationTest
  {
    protected override ConfigTypes ConfigFormat => ConfigTypes.Xml;

    protected override void AddConfigurationFile(IConfigurationBuilder configurationBuilder)
    {
      _ = configurationBuilder.AddXmlFile("LocalizationSettings.config");
    }

    [Test]
    public void NameAttributeEmptyNameTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Localization.NameAttribute.NameEmpty");
      var locConfig = LocalizationConfiguration.Load(section);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    public void NameAttributeAbsentTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Localization.NameAttribute.None");
      var locConfig = LocalizationConfiguration.Load(section);
      CheckConfigurationIsDefault(locConfig);
    }


    [Test]
    public void NameAttributeDefinedTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Localization.NameAttribute.Defined");
      var locConfig = LocalizationConfiguration.Load(section);
      Assert.That(locConfig, Is.Not.Null);
      Assert.That(locConfig.DefaultCulture, Is.EqualTo(expectedCulture));
    }

    [Test]
    public void NameAttributeFaultyValueTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Localization.NameAttribute.FaultyValue");
      var locConfig = LocalizationConfiguration.Load(section);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    public void NameAttributeLowCase()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Localization.NameAttribute.LC");
      var locConfig = LocalizationConfiguration.Load(section);
      Assert.That(locConfig, Is.Not.Null);
      Assert.That(locConfig.DefaultCulture, Is.EqualTo(expectedCulture));
    }

    [Test]
    public void NameAttributeUpperCase()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Localization.NameAttribute.UC");
      var locConfig = LocalizationConfiguration.Load(section);
      Assert.That(locConfig, Is.Not.Null);
      Assert.That(locConfig.DefaultCulture, Is.EqualTo(expectedCulture));
    }

    [Test]
    public void NameAttributePascalCase()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Localization.NameAttribute.PC");
      var locConfig = LocalizationConfiguration.Load(section);
      Assert.That(locConfig, Is.Not.Null);
      Assert.That(locConfig.DefaultCulture, Is.EqualTo(expectedCulture));
    }
  }

  [TestFixture]
  public abstract class ModernConfigurationTest : TestCommon.ModernConfigurationTestBase
  {
    protected readonly CultureInfo defaultCulture = new CultureInfo("en-US");
    protected readonly CultureInfo expectedCulture = new CultureInfo("es-ES");

    private CultureInfo resetCulture;

    public override void BeforeAllTestsSetUp()
    {
      base.BeforeAllTestsSetUp();
      
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
    public void EmptySectionCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.Empty");
      var locConfig = LocalizationConfiguration.Load(section);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    public void EmptyName()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.NameEmpty");
      var locConfig = LocalizationConfiguration.Load(section);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    public void NameDefined()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.NameDefined");
      var locConfig = LocalizationConfiguration.Load(section);
      Assert.That(locConfig, Is.Not.Null);
      Assert.That(locConfig.DefaultCulture, Is.EqualTo(expectedCulture));
    }

    [Test]
    public void FaultyValue()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.FaultyValue");
      var locConfig = LocalizationConfiguration.Load(section);
      CheckConfigurationIsDefault(locConfig);
    }


    #region Naming
    [Test]
    public void NamingInLowCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.Naming.LC");
      var locConfig = LocalizationConfiguration.Load(section);
      ValidateNamingConfigurationResults(locConfig);
    }

    [Test]
    public void NamingInUpperCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.Naming.UC");
      var locConfig = LocalizationConfiguration.Load(section);
      ValidateNamingConfigurationResults(locConfig);
    }

    [Test]
    public void NamingInCamelCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.Naming.CC");
      var locConfig = LocalizationConfiguration.Load(section);
      ValidateNamingConfigurationResults(locConfig);
    }

    [Test]
    public void NamingInPascalCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.Naming.PC");
      var locConfig = LocalizationConfiguration.Load(section);
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
    public void MistypeInLowCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.Mistype.LC");
      var locConfig = LocalizationConfiguration.Load(section);
      ValidateMistypeConfigurationResults(locConfig);
    }

    [Test]
    public void MistypeInUpperCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.Mistype.UC");
      var locConfig = LocalizationConfiguration.Load(section);
      ValidateMistypeConfigurationResults(locConfig);
    }

    [Test]
    public void MistypeInCamelCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.Mistype.CC");
      var locConfig = LocalizationConfiguration.Load(section);
      ValidateMistypeConfigurationResults(locConfig);
    }

    [Test]
    public void MistypeInPascalCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.Mistype.PC");
      var locConfig = LocalizationConfiguration.Load(section);
      ValidateMistypeConfigurationResults(locConfig);
    }

    private void ValidateMistypeConfigurationResults(LocalizationConfiguration locConfig)
    {
      CheckConfigurationIsDefault(locConfig);
    }

    #endregion

    #region Name as node

    [Test]
    public void NoNameNodes()
    {
      IgnoreIfXml();

      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.Empty");
      var locConfig = LocalizationConfiguration.Load(section);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    public void NameNodeIsEmpty()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.NameEmpty");
      var locConfig = LocalizationConfiguration.Load(section);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    public void DefinedNameNode()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.Espaniol");
      var locConfig = LocalizationConfiguration.Load(section);
      Assert.That(locConfig, Is.Not.Null);
      Assert.That(locConfig.DefaultCulture, Is.EqualTo(expectedCulture));
    }

    [Test]
    public void FaultyNameNodeValue()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.FaultyValue");
      var locConfig = LocalizationConfiguration.Load(section);
      CheckConfigurationIsDefault(locConfig);
    }

    [Test]
    public void NameNodeInLowCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.Naming.LC");
      var locConfig = LocalizationConfiguration.Load(section);
      ValidateNamingConfigurationResults(locConfig);
    }

    [Test]
    public void NameNodeInUpperCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.Naming.UC");
      var locConfig = LocalizationConfiguration.Load(section);
      ValidateNamingConfigurationResults(locConfig);
    }

    [Test]
    public void NameNodeInCamelCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.Naming.CC");
      var locConfig = LocalizationConfiguration.Load(section);
      ValidateNamingConfigurationResults(locConfig);
    }

    [Test]
    public void NameNodeInPascalCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Localization.{ConfigFormat}.NameNode.Naming.PC");
      var locConfig = LocalizationConfiguration.Load(section);
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
