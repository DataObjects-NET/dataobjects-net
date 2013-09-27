// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.06

using System;
using System.Configuration;
using System.Linq;
using NUnit.Framework;
using Xtensive.Linq;
using Xtensive.Testing;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Configuration.UserDefinedMappings
{
  [CompilerContainer(typeof(SqlExpression), ConflictHandlingMethod.ReportError)]
  internal static class ArrayMappings
  {
    [Compiler(typeof(byte[]), "Length", TargetKind.PropertyGet)]
    public static SqlExpression ByteArrayLength(SqlExpression _this)
    {
      return SqlDml.BinaryLength(_this);
    }
  }
}

namespace Xtensive.Orm.Tests.Configuration
{
  [TestFixture]
  public class AppConfigTest
  {
    [Test]
    public void CustomMemberCompilerProvidersTest()
    {
      var configuration = DomainConfiguration.Load("AppConfigTest", "TestDomain3");
      configuration.Lock();
      Assert.AreEqual(1, configuration.Types.CompilerContainers.Count());
    }

    [Test]
    public void TestDomain2()
    {
      var configuration = DomainConfiguration.Load("AppConfigTest", "TestDomain1");
      Assert.IsNotNull(configuration);
    }

    [Test]
    public void TestWrongSection()
    {
      AssertEx.ThrowsInvalidOperationException(() => {
        var configuration = DomainConfiguration.Load("AppConfigTest1", "TestDomain1");
      });
    }

    [Test]
    public void TestWrongDomain()
    {
      AssertEx.ThrowsInvalidOperationException(() => {
        var configuration = DomainConfiguration.Load("AppConfigTest", "TestDomain0");
      });
    }

    [Test]
    public void BatchSizeTest()
    {
      var configuration = DomainConfiguration.Load("AppConfigTest", "TestDomain4");
      var defaultSession = configuration.Sessions[WellKnown.Sessions.Default];
      Assert.IsNotNull(defaultSession);
      Assert.AreEqual(10, defaultSession.BatchSize);
      var myCoolSession = configuration.Sessions["MyCoolSession"];
      Assert.IsNotNull(myCoolSession);
      Assert.AreEqual(100, myCoolSession.BatchSize);
      var clone = myCoolSession.Clone();
      Assert.AreEqual(100, clone.BatchSize);
    }

    [Test]
    public void EntityChangeRegistrySizeTest()
    {
      var configuration = DomainConfiguration.Load("AppConfigTest", "DomainWithCustomChangeRegistrySize");
      var defaultSession = configuration.Sessions[WellKnown.Sessions.Default];
      Assert.AreEqual(1000, defaultSession.EntityChangeRegistrySize);
      Assert.AreEqual(1000, defaultSession.Clone().EntityChangeRegistrySize);
    }

    [Test]
    public void SchemaSyncExceptionFormatTest()
    {
      var configuration = DomainConfiguration.Load("AppConfigTest", "DomainWithBriefSchemaSyncExceptions");
      Assert.That(configuration.SchemaSyncExceptionFormat, Is.EqualTo(SchemaSyncExceptionFormat.Brief));
      var clone = configuration.Clone();
      Assert.That(clone.SchemaSyncExceptionFormat, Is.EqualTo(SchemaSyncExceptionFormat.Brief));
    }

    [Test]
    public void NativeLibraryCacheFolderTest()
    {
      const string expected = @".\Native";
      var configuration = DomainConfiguration.Load("AppConfigTest", "DomainWithCustomNativeLibraryCacheFolder");
      Assert.That(configuration.NativeLibraryCacheFolder, Is.EqualTo(expected));
      var clone = configuration.Clone();
      Assert.That(clone.NativeLibraryCacheFolder, Is.EqualTo(expected));
    }

    [Test]
    public void ConnectionInitializationSqlTest()
    {
      const string expected = @"use [OtherDb]";
      var configuration = DomainConfiguration.Load("AppConfigTest", "DomainWithInitSql");
      Assert.That(configuration.ConnectionInitializationSql, Is.EqualTo(expected));
      var clone = configuration.Clone();
      Assert.That(clone.ConnectionInitializationSql, Is.EqualTo(expected));
    }

    [Test]
    public void AdvancedMappingTest()
    {
      var configuration = DomainConfiguration.Load("AppConfigTest", "AdvancedMappingTest");
      ValidateAdvancedMappingConfiguration(configuration);
      var clone = configuration.Clone();
      ValidateAdvancedMappingConfiguration(clone);

      var bad1 = configuration.Clone();
      bad1.DefaultDatabase = null;
      AssertEx.ThrowsInvalidOperationException(bad1.Lock);

      var bad2 = configuration.Clone();
      bad2.DefaultSchema = null;
      AssertEx.ThrowsInvalidOperationException(bad2.Lock);

      var good = configuration.Clone();
      good.DefaultDatabase = null;
      good.MappingRules.Clear();
      good.MappingRules.Map(GetType().Namespace).ToSchema("check");
      good.Lock();
    }

    private void ValidateAdvancedMappingConfiguration(DomainConfiguration configuration)
    {
      Assert.That(configuration.AllowCyclicDatabaseDependencies, Is.True);
      Assert.That(configuration.DefaultDatabase, Is.EqualTo("main"));

      Assert.That(configuration.MappingRules.Count, Is.EqualTo(2));
      var rule1 = configuration.MappingRules[0];
      Assert.That(rule1.Namespace, Is.EqualTo("Xtensive.Orm.Tests.Configuration"));
      Assert.That(rule1.Schema, Is.EqualTo("myschema"));
      var rule2 = configuration.MappingRules[1];
      Assert.That(rule2.Assembly, Is.EqualTo(GetType().Assembly));
      Assert.That(rule2.Database, Is.EqualTo("other"));

      Assert.That(configuration.Databases.Count, Is.EqualTo(2));
      var alias1 = configuration.Databases[0];
      Assert.That(alias1.Name, Is.EqualTo("main"));
      Assert.That(alias1.RealName, Is.EqualTo("DO40-Tests"));
      var alias2 = configuration.Databases[1];
      Assert.That(alias2.Name, Is.EqualTo("other"));
      Assert.That(alias2.RealName, Is.EqualTo("Other-DO40-Tests"));

      configuration.Lock(); // ensure configuration is correct
    }

    [Test]
    public void DotsAndHypensExclusiveOptionsTest()
    {
      var configuration = new DomainConfiguration();

      const NamingRules invalidRules1 = NamingRules.UnderscoreDots | NamingRules.UnderscoreHyphens | NamingRules.RemoveDots;
      const NamingRules invalidRules2 = NamingRules.RemoveDots | NamingRules.RemoveHyphens | NamingRules.UnderscoreHyphens;
      const NamingRules validRules1 = NamingRules.UnderscoreHyphens | NamingRules.UnderscoreDots;
      const NamingRules validRules2 = NamingRules.RemoveDots | NamingRules.RemoveHyphens;

      var convention = configuration.NamingConvention;

      AssertEx.ThrowsArgumentException(() => convention.NamingRules = invalidRules1);
      AssertEx.ThrowsArgumentException(() => convention.NamingRules = invalidRules2);

      convention.NamingRules = validRules1;
      convention.NamingRules = validRules2;
    }
    
    [Test]
    public void ReferencedConnectionStringTest()
    {
      var configuration = DomainConfiguration.Load("AppConfigTest", "DomainWithReferencedConnectionStrings");
      var domainConnectionString = ConfigurationManager.ConnectionStrings["DomainConnectionString"].ConnectionString;
      var sessionConnectionString = ConfigurationManager.ConnectionStrings["SessionConnectionString"].ConnectionString;
      ValidateConnectionString(domainConnectionString, configuration.ConnectionInfo);
      ValidateConnectionString(sessionConnectionString, configuration.Sessions.Default.ConnectionInfo);
    }

    private void ValidateConnectionString(string expected, ConnectionInfo actual)
    {
      Assert.That(actual, Is.Not.Null);
      Assert.That(actual.ConnectionString, Is.EqualTo(expected));
    }

    [Test]
    public void DefaultConfigurationTest()
    {
      var actualDomainConfiguration = DomainConfiguration.Load("AppConfigTest", "DomainWithoutConfiguration");
      var expectedDomainConfiguration = new DomainConfiguration();
      ValidateDomainConfiguration(expectedDomainConfiguration, actualDomainConfiguration);
      ValidateNamingCovention(expectedDomainConfiguration.NamingConvention, actualDomainConfiguration.NamingConvention);
      ValidateSessionConfiguration(new SessionConfiguration(), actualDomainConfiguration.Sessions.Default);
    }

    private void ValidateSessionConfiguration(SessionConfiguration expected, SessionConfiguration actual)
    {
      Assert.That(actual.BatchSize, Is.EqualTo(expected.BatchSize));
      Assert.That(actual.CacheSize, Is.EqualTo(expected.CacheSize));
      Assert.That(actual.CacheType, Is.EqualTo(expected.CacheType));
      Assert.That(actual.ConnectionInfo, Is.EqualTo(expected.ConnectionInfo));
      Assert.That(actual.DefaultCommandTimeout, Is.EqualTo(expected.DefaultCommandTimeout));
      Assert.That(actual.DefaultIsolationLevel, Is.EqualTo(expected.DefaultIsolationLevel));
      Assert.That(actual.EntityChangeRegistrySize, Is.EqualTo(expected.EntityChangeRegistrySize));
      Assert.That(actual.Options, Is.EqualTo(expected.Options));
      Assert.That(actual.Password, Is.EqualTo(expected.Password));
      Assert.That(actual.ReaderPreloading, Is.EqualTo(expected.ReaderPreloading));
      Assert.That(actual.ServiceContainerType, Is.EqualTo(expected.ServiceContainerType));
      Assert.That(actual.Type, Is.EqualTo(expected.Type));
      Assert.That(actual.UserName, Is.EqualTo(expected.UserName));
    }

    private static void ValidateDomainConfiguration(DomainConfiguration expected, DomainConfiguration actual)
    {
      Assert.That(actual.AutoValidation, Is.EqualTo(expected.AutoValidation));
      Assert.That(actual.ConnectionInfo, Is.EqualTo(expected.ConnectionInfo));
      Assert.That(actual.DefaultSchema, Is.EqualTo(expected.DefaultSchema));
      Assert.That(actual.ForcedServerVersion, Is.EqualTo(expected.ForcedServerVersion));
      Assert.That(actual.ForeignKeyMode, Is.EqualTo(expected.ForeignKeyMode));
      Assert.That(actual.IncludeSqlInExceptions, Is.EqualTo(expected.IncludeSqlInExceptions));
      Assert.That(actual.KeyCacheSize, Is.EqualTo(expected.KeyCacheSize));
      Assert.That(actual.KeyGeneratorCacheSize, Is.EqualTo(expected.KeyGeneratorCacheSize));
      Assert.That(actual.QueryCacheSize, Is.EqualTo(expected.QueryCacheSize));
      Assert.That(actual.RecordSetMappingCacheSize, Is.EqualTo(expected.RecordSetMappingCacheSize));
      Assert.That(actual.SchemaSyncExceptionFormat, Is.EqualTo(expected.SchemaSyncExceptionFormat));
      Assert.That(actual.ServiceContainerType, Is.EqualTo(expected.ServiceContainerType));
      Assert.That(actual.UpgradeMode, Is.EqualTo(expected.UpgradeMode));
      Assert.That(actual.ValidationMode, Is.EqualTo(expected.ValidationMode));
    }

    private static void ValidateNamingCovention(NamingConvention expected, NamingConvention actual)
    {
      Assert.That(actual.LetterCasePolicy, Is.EqualTo(expected.LetterCasePolicy));
      Assert.That(actual.NamespacePolicy, Is.EqualTo(expected.NamespacePolicy));
      Assert.That(actual.NamingRules, Is.EqualTo(expected.NamingRules));
    }
  }
}
