// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.09.11

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Storage.Tests.ObjectModel.Northwind;
using System.Linq;

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0394_UndefinedSqlTypeExtractionError : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      EnsureProtocolIs(StorageProtocol.SqlServer | StorageProtocol.SqlServerCe);
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
          var createTableCommandText = "CREATE TABLE " + 
            domainHandler.Schema.Name + ".[TestTable] ([TestColumn] [money])";
          var sessionHandler = (SessionHandler) session.Handler;
          sessionHandler.ExecuteNonQuery(createTableCommandText);
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