// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.06

using System;
using System.Collections.Generic;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Core.Caching;
using Xtensive.Core.Linq;
using Xtensive.Core.Reflection;
using Xtensive.Core.Testing;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Building;
using Xtensive.Storage.Configuration;
using System.Reflection;
using Xtensive.Storage.Tests.Storage.TranscationsTest;
using SqlFactory = Xtensive.Sql.Dom.Sql;


namespace Xtensive.Storage.Tests.Configuration.UserDefinedMappings
{
  [CompilerContainer(typeof(SqlExpression), ConflictHandlingMethod.ReportError)]
  internal static class ArrayMappings
  {
    [Compiler(typeof(byte[]), "Length", TargetKind.PropertyGet)]
    public static SqlExpression ByteArrayLength(SqlExpression _this)
    {
      return SqlFactory.Length(_this);
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
      var c = DomainConfiguration.Load("AppConfigTest", "TestDomain3");
      c.Lock();
      Assert.AreEqual(c.CompilerContainers.Count, 1);
    }

    [Test]
    public void TestDomain2()
    {
      var c = DomainConfiguration.Load("AppConfigTest", "TestDomain1");
      Assert.IsNotNull(c);
    }

    [Test]
    public void TestWrongSection()
    {
      AssertEx.ThrowsInvalidOperationException(() => {
        var c = DomainConfiguration.Load("AppConfigTest1", "TestDomain1");
      });
    }

    [Test]
    public void TestWrongDomain()
    {
      AssertEx.ThrowsInvalidOperationException(() => {
        var c = DomainConfiguration.Load("AppConfigTest", "TestDomain0");
      });
    }
  }
}
