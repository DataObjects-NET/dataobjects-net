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
    public static SqlExpression ByteArrayLength(SqlExpression this_)
    {
      return SqlFactory.Length(this_);
    }
  }
}

namespace Xtensive.Storage.Tests.Configuration
{
  [TestFixture]
  public class AppConfigTest
  {
    [Test]
    public void TestDomain1()
    {
      var c1 = DomainConfiguration.Load("AppConfigTest", "TestDomain1");
      Log.Debug("SessionPoolSize: {0}", c1.SessionPoolSize);
      Log.Debug("ConnectionInfo: {0}", c1.ConnectionInfo);
      foreach (Type builder in c1.Builders) {
        Log.Debug("Builder: {0}", builder.FullName);
      }
      foreach (Type type in c1.Types) {
        Log.Debug("Type: {0}", type.FullName);
      }
      Log.Debug("NamingConvention.LetterCasePolicy: {0}", c1.NamingConvention.LetterCasePolicy);
      Log.Debug("NamingConvention.NamespacePolicy: {0}", c1.NamingConvention.NamespacePolicy);
      Log.Debug("NamingConvention.NamingRules: {0}", c1.NamingConvention.NamingRules);
      foreach (KeyValuePair<string, string> namespaceSynonym in c1.NamingConvention.NamespaceSynonyms) {
        Log.Debug("NamingConvention.NamespaceSynonym (key, value): {0} {1}", namespaceSynonym.Key, namespaceSynonym.Value);
      }
      Log.Debug("Session settings. UserName: {0}, CacheSize: {1}, CacheType: {2}", c1.Sessions.Default.UserName, c1.Sessions.Default.CacheSize, c1.Sessions.Default.CacheType);

      var c2 = new DomainConfiguration("memory://localhost/")
        {
          SessionPoolSize = 77,
          Name = "TestDomain1"
        };
      c2.Builders.Add(typeof (string));
      c2.Builders.Add(typeof (int));
      c2.Types.Register(Assembly.Load("Xtensive.Storage.Tests"), "Xtensive.Storage.Tests");
      c2.NamingConvention.LetterCasePolicy = LetterCasePolicy.Uppercase;
      c2.NamingConvention.NamespacePolicy = NamespacePolicy.Hash;
      c2.NamingConvention.NamingRules = NamingRules.UnderscoreDots;
      c2.NamingConvention.NamespaceSynonyms.Add("Xtensive.Storage", "XS");
      c2.NamingConvention.NamespaceSynonyms.Add("Xtensive.Messaging", "XM");
      c2.NamingConvention.NamespaceSynonyms.Add("Xtensive.Indexing", "XI");
      c2.Sessions.Add(new SessionConfiguration {CacheSize = 111, UserName = "User", DefaultIsolationLevel = IsolationLevel.Snapshot});
      c2.Sessions.Add(new SessionConfiguration {Name = "UserSession", CacheSize = 324, Password = "222"});
      c2.Sessions.Add(new SessionConfiguration {Name = "System", UserName = "dfdfdfd", Password = "333", Options = SessionOptions.AmbientTransactions});
      c2.Sessions.Add(new SessionConfiguration { Name = "UserSession2", CacheType = SessionCacheType.Infinite});
      Assert.AreEqual(c1, c2);
    }

    [Test]
    public void TestSessionConfiguration()
    {
      var c = DomainConfiguration.Load("AppConfigTest", "TestDomain1");
      c.Lock();
      Assert.AreEqual(c.Sessions.Default, new SessionConfiguration { CacheSize = 111, UserName = "User", DefaultIsolationLevel = IsolationLevel.Snapshot });
      Assert.AreEqual(c.Sessions.Service, new SessionConfiguration { Name = "Service", UserName = "User", CacheSize = 111, DefaultIsolationLevel = IsolationLevel.Snapshot });
      Assert.AreEqual(c.Sessions.Generator, new SessionConfiguration { Name = "Generator", UserName = "User", CacheSize = 111, DefaultIsolationLevel = IsolationLevel.Snapshot });
      Assert.AreEqual(c.Sessions.System, new SessionConfiguration { Name = "System", UserName = "dfdfdfd", Password = "333", Options = SessionOptions.AmbientTransactions, CacheSize = 111, DefaultIsolationLevel = IsolationLevel.Snapshot });
      Assert.AreEqual(c.Sessions["UserSession"], new SessionConfiguration { UserName = "User", CacheSize = 324, Password = "222", DefaultIsolationLevel = IsolationLevel.Snapshot });
    }

    [Test]
    public void CompilerExtensionsTest()
    {
      var c = DomainConfiguration.Load("AppConfigTest", "TestDomain3");
      c.Lock();
      Assert.AreEqual(c.Mappings.Count, 1);
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
