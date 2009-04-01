// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Storage.Tests;
using Xtensive.Storage.Tests.Storage.Providers.Sql;

namespace Xtensive.Indexing.Tests.Storage
{
  [Serializable]
  public class ConvertModelTest : AutoBuildTest
  {
    private Model model;


    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(A).Namespace);
      return config;
    }

    protected override Xtensive.Storage.Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      // Get current SQL model
      var domainHandler = domain.Handlers.DomainHandler;
      using (var connection = (SqlConnection)((DomainHandler)domainHandler).ConnectionProvider.CreateConnection(configuration.ConnectionInfo.ToString()))
      {
        model = new SqlModelProvider(connection).Build();
        return domain;
      }
    }

    [Test]
    public void BaseTest()
    {
      var storage = new SqlModelConverter().Convert(model.DefaultServer.DefaultCatalog.DefaultSchema);
      storage.Dump();
    }
  }
}