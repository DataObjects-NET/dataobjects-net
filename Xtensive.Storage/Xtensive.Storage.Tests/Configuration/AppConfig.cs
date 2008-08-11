// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.06

using System;
using System.Collections.Generic;
using System.Configuration;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using System.Reflection;

namespace Xtensive.Storage.Tests.Configuration
{
  [TestFixture]
  public class AppConfig
  {
    [Test]
    public void DomainConfig()
    {
      var configuration = Xtensive.Storage.Configuration.Configuration.Load("AppConfigTest");
      Assert.AreEqual(2, configuration.Count);
      var domainConfig = configuration[0];
      Log.Debug("SessionPoolSize: {0}", domainConfig.SessionPoolSize);
      Log.Debug("ConnectionInfo: {0}", domainConfig.ConnectionInfo);
      foreach (Type builder in domainConfig.Builders) {
        Log.Debug("Builder: {0}", builder.FullName);
      }
      foreach (Type type in domainConfig.Types) {
        Log.Debug("Type: {0}", type.FullName);
      }
      Log.Debug("NamingConvention.LetterCasePolicy: {0}", domainConfig.NamingConvention.LetterCasePolicy);
      Log.Debug("NamingConvention.NamespacePolicy: {0}", domainConfig.NamingConvention.NamespacePolicy);
      Log.Debug("NamingConvention.NamingRules: {0}", domainConfig.NamingConvention.NamingRules);
      foreach (KeyValuePair<string, string> namespaceSynonym in domainConfig.NamingConvention.NamespaceSynonyms) {
        Log.Debug("NamingConvention.NamespaceSynonym (key, value): {0} {1}", namespaceSynonym.Key, namespaceSynonym.Value);
      }
      var manualConfig = new DomainConfiguration("memory://localhost/");
      manualConfig.SessionPoolSize = 77;
      manualConfig.Builders.Add(typeof(string));
      manualConfig.Builders.Add(typeof(int));
      manualConfig.Types.Register(Assembly.Load("Xtensive.Storage.Tests"), "Xtensive.Storage.Tests");
      manualConfig.NamingConvention.LetterCasePolicy = LetterCasePolicy.Uppercase;
      manualConfig.NamingConvention.NamespacePolicy = NamespacePolicy.Hash;
      manualConfig.NamingConvention.NamingRules = NamingRules.UnderscoreDots;
      manualConfig.NamingConvention.NamespaceSynonyms.Add("Xtensive.Storage", "XS");
      manualConfig.NamingConvention.NamespaceSynonyms.Add("Xtensive.Messaging", "XM");
      manualConfig.NamingConvention.NamespaceSynonyms.Add("Xtensive.Indexing", "XI");
      // Assert.AreEqual(domainConfig, manualConfig);
    }

    [Test]
    public void SessionConfig()
    {
      var sessionConfiguration = (SessionConfiguration)ConfigurationManager.GetSection("SessionConfig");
      Log.Debug("UserName: {0}", sessionConfiguration.UserName);
      Log.Debug("CacheSize: {0}", sessionConfiguration.CacheSize);
    }
  }
}