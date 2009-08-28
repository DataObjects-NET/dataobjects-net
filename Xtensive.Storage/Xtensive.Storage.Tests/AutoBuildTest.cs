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
    protected StorageProtocol Protocol { get; private set; }

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

    protected void EnsureProtocolIs(StorageProtocol allowedProtocols)
    {
      if ((Protocol & allowedProtocols) == 0)
        throw new IgnoreException(
          string.Format("This test is not suitable for '{0}' protocol", protocolName));
    }

    protected void EnsureProtocolIsNot(StorageProtocol disallowedProtocols)
    {
      EnsureProtocolIs(~disallowedProtocols);
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
      case WellKnown.Protocol.Memory:
        Protocol = StorageProtocol.Memory;
        break;
      case WellKnown.Protocol.SqlServer:
        Protocol = StorageProtocol.SqlServer;
        break;
      case WellKnown.Protocol.PostgreSql:
        Protocol = StorageProtocol.PostgreSql;
        break;
      case WellKnown.Protocol.Oracle:
        Protocol = StorageProtocol.Oracle;
        break;
      default:
        throw new ArgumentOutOfRangeException();
      }
    }
  }
}
