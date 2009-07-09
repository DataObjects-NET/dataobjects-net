// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.31

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Configuration;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Tests
{
  [TestFixture]
  public abstract class AutoBuildTest
  {
    private string protocolName;
    private StorageProtocols protocol;

    protected Domain Domain { get; private set; }
    
    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp()
    {
      DomainConfiguration config = BuildConfiguration();
      SelectProtocol(config);
      CheckRequirements();
      Domain = BuildDomain(config);
    }

    [TestFixtureTearDown]
    public virtual void TestFixtureTearDown()
    {
      Domain.DisposeSafely();
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      return DomainConfigurationFactory.Create();
    }

    protected virtual void CheckRequirements()
    {
    }

    protected void EnsureIs(StorageProtocols allowedProtocols)
    {
      if ((protocol & allowedProtocols) == 0)
        throw new IgnoreException(string.Format("This test is not suitable for '{0}' protocol", protocolName));
    }

    protected virtual Domain BuildDomain(DomainConfiguration configuration)
    {
      try {
        return Domain.Build(configuration);
      }
      catch (Exception e) {
        Log.Error(GetType().GetFullName());
        Log.Error(e);
        throw;
      }
    }

    private void SelectProtocol(DomainConfiguration config)
    {
      protocolName = config.ConnectionInfo.Protocol;
      switch (protocolName) {
      case "mssql2005":
        protocol = StorageProtocols.SqlServer;
        break;
      case "pgsql":
        protocol = StorageProtocols.PostgreSql;
        break;
      case "memory":
        protocol = StorageProtocols.Memory;
        break;
      default:
        throw new ArgumentOutOfRangeException();
      }
    }
  }
}
