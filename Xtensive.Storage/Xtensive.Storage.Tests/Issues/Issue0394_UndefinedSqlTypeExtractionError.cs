// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.09.11

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Storage.Tests.ObjectModel.Northwind;
using System.Linq;
using System;

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0394_UndefinedSqlTypeExtractionError : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer | StorageProvider.SqlServerCe);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.Create();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (Customer));
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var domainHandler = (DomainHandler) Domain.Handler;
          string createTableCommandText;
          if (Domain.Configuration.ConnectionInfo.Provider == WellKnown.Provider.SqlServer)
            createTableCommandText = "CREATE TABLE " + 
                                   domainHandler.Schema.Name + ".[TestTable] ([TestColumn] [money])";
          else
            createTableCommandText = "CREATE TABLE [TestTable] ([TestColumn] [money])";
          var sessionHandler = (SessionHandler) session.Handler;
          var queryExecutor = sessionHandler.GetService<IQueryExecutor>(true);
          queryExecutor.ExecuteNonQuery(createTableCommandText);
          transactionScope.Complete();
        }
      }

      var domain = BuildDomain(BuildConfiguration());
      using (var session = Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {
          var schema = ((DomainHandler) Domain.Handler).Schema;
          Assert.IsNull(schema.Tables.SingleOrDefault(table => table.Name=="TestTable"));
          transactionScope.Complete();
        }
      }
    }
  }
}