// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.03.02

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;
using Xtensive.Orm.Tests.Upgrade.SchemaSharing.Model;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing
{
  public class MakingSchemaExtractionResultSharedTest : AutoBuildTest
  {
    private SqlDriver driver;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (Product).Assembly, typeof (Product).Namespace);
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      driver = TestSqlDriver.Create(DomainConfigurationFactory.Create().ConnectionInfo);
    }

    [Test]
    public void MainTest()
    {
      SchemaExtractionResult extractionResult;
      using (var connection = driver.CreateConnection()) {
        connection.Open();
        var defaultSchema = driver.ExtractDefaultSchema(connection);
        extractionResult = new SchemaExtractionResult();
        extractionResult.Catalogs.Add(defaultSchema.Catalog);
      }

      Assert.That(extractionResult.IsShared, Is.False);
      Assert.DoesNotThrow(() => { var name = extractionResult.Catalogs.First().Name; });
      Assert.DoesNotThrow(() => { var name = extractionResult.Catalogs.First().DbName; });
      Assert.DoesNotThrow(() => { var name = extractionResult.Catalogs.First().DefaultSchema.Name; });
      Assert.DoesNotThrow(() => { var name = extractionResult.Catalogs.First().DefaultSchema.DbName; });
      Assert.DoesNotThrow(() => { var name = extractionResult.Catalogs.First().DefaultSchema.Tables["Product"].Name; });
      Assert.DoesNotThrow(() => { var name = extractionResult.Catalogs.First().DefaultSchema.Tables["Product"].DbName; });

      extractionResult.MakeShared();

      Assert.That(extractionResult.IsShared, Is.True);
      Assert.Throws<InvalidOperationException>(() => { var name = extractionResult.Catalogs.First().Name; });
      Assert.Throws<InvalidOperationException>(() => { var name = extractionResult.Catalogs.First().DbName; });
      Assert.Throws<InvalidOperationException>(() => { var name = extractionResult.Catalogs.First().DefaultSchema.Name; });
      Assert.Throws<InvalidOperationException>(() => { var name = extractionResult.Catalogs.First().DefaultSchema.DbName; });
      Assert.DoesNotThrow(() => { var name = extractionResult.Catalogs.First().DefaultSchema.Tables["Product"].Name; });
      Assert.DoesNotThrow(() => { var name = extractionResult.Catalogs.First().DefaultSchema.Tables["Product"].DbName; });
    }
  }
}
