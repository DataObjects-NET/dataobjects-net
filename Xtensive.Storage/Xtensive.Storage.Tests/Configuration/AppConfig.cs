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

namespace Xtensive.Storage.Tests.Configuration
{
  [TestFixture]
  public class AppConfig
  {
    [Test]
    public void DomainConfig()
    {
      var domainConfig = (DomainConfiguration)ConfigurationManager.GetSection("DomainConfig");
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