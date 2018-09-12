// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.08.31

using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests
{
  [TestFixture]
  public abstract class HasConfigurationAccessTest
  {
    public System.Configuration.Configuration Configuration
    {
      get { return GetConfigurationForTestAssembly(); }
    }

    protected DomainConfiguration LoadDomainConfiguration(string name)
    {
      return DomainConfiguration.Load(Configuration, name);
    }

    protected DomainConfiguration LoadDomainConfiguration(string sectionName, string name)
    {
      return DomainConfiguration.Load(Configuration, sectionName, name);
    }

    protected LoggingConfiguration LoadLoggingConfiguration(string sectionName)
    {
      return LoggingConfiguration.Load(Configuration, sectionName);
    }

    private System.Configuration.Configuration GetConfigurationForTestAssembly()
    {
      return GetType().Assembly.GetAssemblyConfiguration();
    }
  }
}