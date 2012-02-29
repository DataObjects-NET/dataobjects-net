// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.09.11

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers.Sql;
using Xtensive.Orm.Tests.ObjectModel.Northwind;
using System.Linq;
using System;

namespace Xtensive.Orm.Tests.Issues
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
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var domainHandler = (DomainHandler) Domain.Handler;
          string createTableCommandText;
          if (Domain.Configuration.ConnectionInfo.Provider == WellKnown.Provider.SqlServer)
            createTableCommandText = "CREATE TABLE " + 
                                   domainHandler.Schema.Name + ".[TestTable] ([TestColumn] [money])";
          else
            createTableCommandText = "CREATE TABLE [TestTable] ([TestColumn] [money])";
          var sessionHandler = (SessionHandler) session.Handler;
          var executor = sessionHandler.GetService<ISqlExecutor>();
          executor.ExecuteNonQuery(createTableCommandText);
          transactionScope.Complete();
        }
      }

      var domain = BuildDomain(BuildConfiguration());
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var schema = ((DomainHandler) Domain.Handler).Schema;
          Assert.IsNull(schema.Tables.SingleOrDefault(table => table.Name=="TestTable"));
          transactionScope.Complete();
        }
      }
    }
  }
}