// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.06

using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Linq;
using Xtensive.Core.Testing;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Configuration.UserDefinedMappings
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

namespace Xtensive.Storage.Tests.Configuration
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
      var defaultSession = configuration.Sessions["Default"];
      Assert.IsNotNull(defaultSession);
      Assert.AreEqual(10, defaultSession.BatchSize);
      var myCoolSession = configuration.Sessions["MyCoolSession"];
      Assert.IsNotNull(myCoolSession);
      Assert.AreEqual(100, myCoolSession.BatchSize);
      var clone = myCoolSession.Clone();
      Assert.AreEqual(100, clone.BatchSize);
    }
  }
}
