// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using Xtensive.Orm.Security.Configuration;

namespace Xtensive.Orm.Security.Tests.Configuration
{
  public sealed class JsonConfigurationTests : ModernConfigurationTests
  {
    protected override ConfigTypes ConfigFormat => ConfigTypes.Json;

    protected override void AddConfigurationFile(IConfigurationBuilder configurationBuilder)
    {
      _ = configurationBuilder.AddJsonFile("securitysettings.json");
    }
  }

  public sealed class XmlConfigurationTests : ModernConfigurationTests
  {
    protected override ConfigTypes ConfigFormat => ConfigTypes.Xml;

    protected override void AddConfigurationFile(IConfigurationBuilder configurationBuilder)
    {
      _ = configurationBuilder.AddXmlFile("SecuritySettings.config");
    }

    [Test]
    public void NameAttributeEmptyNamesTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Security.NameAttribute.NamesEmpty");
      var secConfig = SecurityConfiguration.Load(section);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    public void NameAttributeEmptyAndNonExistentNameTest1()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Security.NameAttribute.NameExistPartially1");
      var secConfig = SecurityConfiguration.Load(section);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    public void NameAttributeEmptyAndNonExistentNameTest2()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Security.NameAttribute.NameExistPartially2");
      var secConfig = SecurityConfiguration.Load(section);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    public void NameAttributeAllNamesTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Security.NameAttribute.AllNames");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }

    [Test]
    public void NameAttributeOnlyHashingServiceTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Security.NameAttribute.OnlyHashing");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    public void NameAttributeOnlyAuthServiceTest()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Security.NameAttribute.OnlyAuth");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("plain"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }

    [Test]
    public void NameAttributeLowCase()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Security.NameAttribute.LC");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }

    [Test]
    public void NameAttributeUpperCase()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Security.NameAttribute.UC");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }

    [Test]
    public void NameAttributePascalCase()
    {
      var section = GetAndCheckConfigurationSection("Xtensive.Orm.Security.NameAttribute.PC");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }
  }


  [TestFixture]
  public abstract class ModernConfigurationTests : TestCommon.ModernConfigurationTestBase
  {
    [Test]
    public void EmptySectionCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.Empty");
      var secConfig = SecurityConfiguration.Load(section);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    public void EmptyNames()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.AllEmpty");
      var secConfig = SecurityConfiguration.Load(section);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    public void OnlyHashingServiceEmpty()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.OnlyHashing.Empty");
      var secConfig = SecurityConfiguration.Load(section);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    public void OnlyHashingServiceMd5()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.OnlyHashing.Md5");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("md5"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    public void OnlyHashingServiceSha1()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.OnlyHashing.Sha1");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    public void OnlyHashingServiceSha256()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.OnlyHashing.Sha256");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha256"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    public void OnlyHashingServiceSha384()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.OnlyHashing.Sha384");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha384"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    public void OnlyHashingServiceSha512()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.OnlyHashing.Sha512");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha512"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    public void OnlyAuthenticationServiceEmptyName()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.OnlyAuth.Empty");
      var secConfig = SecurityConfiguration.Load(section);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    public void OnlyAuthenticationServiceNotDefault()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.OnlyAuth");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("plain"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }


    #region Naming
    [Test]
    public void NamingInLowCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.Naming.LC");
      var secConfig = SecurityConfiguration.Load(section);
      ValidateNamingConfigurationResults(secConfig);
    }

    [Test]
    public void NamingInUpperCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.Naming.UC");
      var secConfig = SecurityConfiguration.Load(section);
      ValidateNamingConfigurationResults(secConfig);
    }

    [Test]
    public void NamingInCamelCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.Naming.CC");
      var secConfig = SecurityConfiguration.Load(section);
      ValidateNamingConfigurationResults(secConfig);
    }

    [Test]
    public void NamingInPascalCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.Naming.PC");
      var secConfig = SecurityConfiguration.Load(section);
      ValidateNamingConfigurationResults(secConfig);
    }

    private static void ValidateNamingConfigurationResults(SecurityConfiguration secConfig)
    {
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }
    #endregion

    #region mistype cases
    [Test]
    public void MistypeInLowCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.Mistype.LC");
      var secConfig = SecurityConfiguration.Load(section);
      ValidateMistypeConfigurationResults(secConfig);
    }

    [Test]
    public void MistypeInUpperCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.Mistype.UC");
      var secConfig = SecurityConfiguration.Load(section);
      ValidateMistypeConfigurationResults(secConfig);
    }

    [Test]
    public void MistypeInCamelCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.Mistype.CC");
      var secConfig = SecurityConfiguration.Load(section);
      ValidateMistypeConfigurationResults(secConfig);
    }

    [Test]
    public void MistypeInPascalCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.Mistype.PC");
      var secConfig = SecurityConfiguration.Load(section);
      ValidateMistypeConfigurationResults(secConfig);
    }

    private static void ValidateMistypeConfigurationResults(SecurityConfiguration secConfig)
    {
      CheckConfigurationIsDefault(secConfig);
    }

    #endregion

    #region Name as node 

    [Test]
    public void NoNameNodes()
    {
      IgnoreIfXml();

      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.Json.NameNode.AllEmpty");
      var secConfig = SecurityConfiguration.Load(section);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    public void NameNodesAreEmpty()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.Json.NameNode.NamesEmpty");
      var secConfig = SecurityConfiguration.Load(section);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    public void OnlyHashingServiceNameIsEmpty()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyHashing.Empty");
      var secConfig = SecurityConfiguration.Load(section);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    public void OnlyHashingServiceWithNameNodeMd5()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyHashing.Md5");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("md5"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    public void OnlyHashingServiceWithNameNodeSha1()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyHashing.Sha1");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    public void OnlyHashingServiceWithNameNodeSha256()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyHashing.Sha256");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha256"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    public void OnlyHashingServiceWithNameNodeSha384()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyHashing.Sha384");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha384"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    public void OnlyHashingServiceWithNameNodeSha512()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyHashing.Sha512");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha512"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    public void OnlyAuthenticationServiceWithNameNodeEmptyName()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyAuth.Empty");
      var secConfig = SecurityConfiguration.Load(section);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    public void OnlyAuthenticationServiceWithNameNodeNotDefault()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyAuth");
      var secConfig = SecurityConfiguration.Load(section);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("plain"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }

    [Test]
    public void NameNodeInLowCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.Naming.LC");
      var secConfig = SecurityConfiguration.Load(section);
      ValidateNamingConfigurationResults(secConfig);
    }

    [Test]
    public void NameNodeInUpperCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.Naming.UC");
      var secConfig = SecurityConfiguration.Load(section);
      ValidateNamingConfigurationResults(secConfig);
    }

    [Test]
    public void NameNodeInCamelCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.Naming.CC");
      var secConfig = SecurityConfiguration.Load(section);
      ValidateNamingConfigurationResults(secConfig);
    }

    [Test]
    public void NameNodeInPascalCase()
    {
      var section = GetAndCheckConfigurationSection($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.Naming.PC");
      var secConfig = SecurityConfiguration.Load(section);
      ValidateNamingConfigurationResults(secConfig);
    }

    #endregion

    protected static void CheckConfigurationIsDefault(SecurityConfiguration secConfig)
    {
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("plain"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }
  }
}
