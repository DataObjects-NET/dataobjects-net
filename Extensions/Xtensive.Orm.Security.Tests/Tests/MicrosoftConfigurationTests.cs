// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using Xtensive.Orm.Security.Configuration;

namespace Xtensive.Orm.Security.Tests.Configuration
{
  public sealed class JsonConfigurationTests : MicrosoftConfigurationTests
  {
    protected override ConfigTypes ConfigFormat => ConfigTypes.Json;

    protected override void AddConfigurationFile(IConfigurationBuilder configurationBuilder)
    {
      _ = configurationBuilder.AddJsonFile("securitysettings.json");
    }
  }

  public sealed class XmlConfigurationTests : MicrosoftConfigurationTests
  {
    protected override ConfigTypes ConfigFormat => ConfigTypes.Xml;

    protected override void AddConfigurationFile(IConfigurationBuilder configurationBuilder)
    {
      _ = configurationBuilder.AddXmlFile("SecuritySettings.config");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributeEmptyNamesTest(bool useRoot)
    {
      var secConfig = LoadConfiguration("Xtensive.Orm.Security.NameAttribute.NamesEmpty", useRoot);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributeEmptyAndNonExistentNameTest1(bool useRoot)
    {
      var secConfig = LoadConfiguration("Xtensive.Orm.Security.NameAttribute.NameExistPartially1", useRoot);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributeEmptyAndNonExistentNameTest2(bool useRoot)
    {
      var secConfig = LoadConfiguration("Xtensive.Orm.Security.NameAttribute.NameExistPartially2", useRoot);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributeAllNamesTest(bool useRoot)
    {
      var secConfig = LoadConfiguration("Xtensive.Orm.Security.NameAttribute.AllNames", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributeOnlyHashingServiceTest(bool useRoot)
    {
      var secConfig = LoadConfiguration("Xtensive.Orm.Security.NameAttribute.OnlyHashing", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributeOnlyAuthServiceTest(bool useRoot)
    {
      var secConfig = LoadConfiguration("Xtensive.Orm.Security.NameAttribute.OnlyAuth", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("plain"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributeLowCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration("Xtensive.Orm.Security.NameAttribute.LC", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributeUpperCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration("Xtensive.Orm.Security.NameAttribute.UC", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameAttributePascalCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration("Xtensive.Orm.Security.NameAttribute.PC", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }
  }


  [TestFixture]
  public abstract class MicrosoftConfigurationTests : TestCommon.MicrosoftConfigurationTestBase
  {
    protected SecurityConfiguration LoadConfiguration(string sectionName, bool useRoot)
    {
      return useRoot
        ? SecurityConfiguration.Load(configurationRoot, sectionName)
        : SecurityConfiguration.Load(configurationRoot.GetSection(sectionName));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void EmptySectionCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.Empty", useRoot);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void EmptyNamesTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.AllEmpty", useRoot);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyHashingServiceEmptyTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.OnlyHashing.Empty", useRoot);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyHashingServiceMd5Test(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.OnlyHashing.Md5", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("md5"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyHashingServiceSha1Test(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.OnlyHashing.Sha1", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyHashingServiceSha256Test(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.OnlyHashing.Sha256", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha256"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyHashingServiceSha384Test(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.OnlyHashing.Sha384", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha384"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyHashingServiceSha512Test(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.OnlyHashing.Sha512", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha512"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyAuthenticationServiceEmptyNameTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.OnlyAuth.Empty", useRoot);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyAuthenticationServiceNotDefaultTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.OnlyAuth", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("plain"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }

    #region Naming

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingInLowCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.Naming.LC", useRoot);
      ValidateNamingConfigurationResults(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingInUpperCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.Naming.UC", useRoot);
      ValidateNamingConfigurationResults(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingInCamelCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.Naming.CC", useRoot);
      ValidateNamingConfigurationResults(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NamingInPascalCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.Naming.PC", useRoot);
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
    [TestCase(true)]
    [TestCase(false)]
    public void MistypeInLowCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.Mistype.LC", useRoot);
      ValidateMistypeConfigurationResults(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MistypeInUpperCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.Mistype.UC", useRoot);
      ValidateMistypeConfigurationResults(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MistypeInCamelCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.Mistype.CC", useRoot);
      ValidateMistypeConfigurationResults(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MistypeInPascalCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.Mistype.PC", useRoot);
      ValidateMistypeConfigurationResults(secConfig);
    }

    private static void ValidateMistypeConfigurationResults(SecurityConfiguration secConfig)
    {
      CheckConfigurationIsDefault(secConfig);
    }

    #endregion

    #region Name as node

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NoNameNodesTest(bool useRoot)
    {
      IgnoreIfXml();

      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.Json.NameNode.AllEmpty", useRoot);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameNodesAreEmptyTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.Json.NameNode.NamesEmpty", useRoot);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyHashingServiceNameIsEmptyTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyHashing.Empty", useRoot);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyHashingServiceWithNameNodeMd5Test(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyHashing.Md5", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("md5"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyHashingServiceWithNameNodeSha1Test(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyHashing.Sha1", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha1"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyHashingServiceWithNameNodeSha256Test(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyHashing.Sha256", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha256"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyHashingServiceWithNameNodeSha384Test(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyHashing.Sha384", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha384"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyHashingServiceWithNameNodeSha512Test(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyHashing.Sha512", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("sha512"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("default"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyAuthenticationServiceWithNameNodeEmptyNameTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyAuth.Empty", useRoot);
      CheckConfigurationIsDefault(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void OnlyAuthenticationServiceWithNameNodeNotDefaultTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.OnlyAuth", useRoot);
      Assert.That(secConfig, Is.Not.Null);
      Assert.That(secConfig.HashingServiceName, Is.EqualTo("plain"));
      Assert.That(secConfig.AuthenticationServiceName, Is.EqualTo("notdefault"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameNodeInLowCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.Naming.LC", useRoot);
      ValidateNamingConfigurationResults(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameNodeInUpperCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.Naming.UC", useRoot);
      ValidateNamingConfigurationResults(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameNodeInCamelCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.Naming.CC", useRoot);
      ValidateNamingConfigurationResults(secConfig);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void NameNodeInPascalCaseTest(bool useRoot)
    {
      var secConfig = LoadConfiguration($"Xtensive.Orm.Security.{ConfigFormat}.NameNode.Naming.PC", useRoot);
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
