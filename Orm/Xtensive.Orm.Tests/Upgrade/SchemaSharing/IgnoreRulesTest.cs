// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.03.03

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Orm.Tests.Upgrade.SchemaSharing.Model;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing
{
  [TestFixture]
  public class IgnoreRulesTest : AutoBuildTest
  {
    private const string MainNodeId = WellKnown.DefaultNodeId;
    private const string AdditionalNodeId = "Additional";

    private SqlDriver driver;
    private Dictionary<string, string> nodeToSchemaMap;

    public override void TestFixtureSetUp()
    {
      ProviderInfo = StorageProviderInfo.Instance.Info;
      driver = TestSqlDriver.Create(DomainConfigurationFactory.Create().ConnectionInfo);
      nodeToSchemaMap = BuildNodeToSchemaMap();
    }

    #region Tests
    [Test]
    public void PerformWithoutSharingTest()
    {
      RunTest(DomainUpgradeMode.Perform, false);
    }

    [Test]
    public void PerformWithSharingTest()
    {
      RunTest(DomainUpgradeMode.Perform, true);
    }

    [Test]
    public void PerformSafelyWithoutSharingTest()
    {
      RunTest(DomainUpgradeMode.PerformSafely, false);
    }

    [Test]
    public void PerformSafelyWithSharing()
    {
      RunTest(DomainUpgradeMode.PerformSafely, true);
    }

    [Test]
    public void ValidateWithoutSharingTest()
    {
      RunTest(DomainUpgradeMode.Validate, false);
    }

    [Test]
    public void ValidateWithSharingTest()
    {
      RunTest(DomainUpgradeMode.Validate, true);
    }

    [Test]
    public void SkipWithoutSharingTest()
    {
      RunTest(DomainUpgradeMode.Skip, false);
    }

    [Test]
    public void SkipWithSharingTest()
    {
      RunTest(DomainUpgradeMode.Skip, true);
    }

    [Test]
    public void LegacyValidateWithoutSharingTest()
    {
      RunTest(DomainUpgradeMode.LegacyValidate, false);
    }

    [Test]
    public void LegacyValidateWithSharingTest()
    {
      RunTest(DomainUpgradeMode.LegacyValidate, true);
    }

    [Test]
    public void LegacySkipWithoutSharingTest()
    {
      RunTest(DomainUpgradeMode.LegacySkip, false);
    }

    [Test]
    public void LegacySkipWithSharingTest()
    {
      RunTest(DomainUpgradeMode.LegacySkip, true);
    }
    #endregion

    private void RunTest(DomainUpgradeMode upgradeMode, bool shareStorageSchemaOverNodes)
    {
      Catalog catalog;
      using (var initialDomain = BuildInitialDomain()) {
        catalog = ExtractCatalog();
        ValidateInitialCatalog(catalog);
        PopulateData(initialDomain);
      }
      ManuallyInsertIgnoredItems(catalog);
      catalog = ExtractCatalog();
      ValidateModifiedCatalogs(catalog);
      PopulateIgnoredData(catalog);

      using (var domain = BuildDomain(upgradeMode, shareStorageSchemaOverNodes))
        ValidateDomainData(domain);

      catalog = ExtractCatalog();
      ValidateModifiedCatalogs(catalog);
      ValidateIgnoredData(catalog);
    }

    #region Domain-level operations

    private Domain BuildInitialDomain()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (Product).Assembly, typeof (Product).Namespace);
      if (nodeToSchemaMap.Count > 0) {
        configuration.DefaultSchema = nodeToSchemaMap[MainNodeId];
        var nodeConfiguration = new NodeConfiguration(AdditionalNodeId);
        nodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
        nodeConfiguration.SchemaMapping.Add(configuration.DefaultSchema, nodeToSchemaMap[AdditionalNodeId]);
        var domain = BuildDomain(configuration);
        domain.StorageNodeManager.AddNode(nodeConfiguration);
        return domain;
      }
      return BuildDomain(configuration);
    }

    private Domain BuildDomain(DomainUpgradeMode upgradeMode, bool shareStorageSchemaOverNodes)
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = upgradeMode;
      configuration.IgnoreRules = GetIgnoreRules();
      configuration.ShareStorageSchemaOverNodes = shareStorageSchemaOverNodes;
      configuration.Types.Register(typeof (Product).Assembly, typeof (Product).Namespace);

      if (nodeToSchemaMap.Count > 0) {
        configuration.DefaultSchema = nodeToSchemaMap[MainNodeId];
        var nodeConfiguration = new NodeConfiguration(AdditionalNodeId);
        nodeConfiguration.UpgradeMode = upgradeMode;
        nodeConfiguration.SchemaMapping.Add(nodeToSchemaMap[MainNodeId], nodeToSchemaMap[AdditionalNodeId]);
        var domain = BuildDomain(configuration);
        domain.StorageNodeManager.AddNode(nodeConfiguration);
        return domain;
      }
      return BuildDomain(configuration);
    }

    private void PopulateData(Domain domain)
    {
      var storageNodes = (nodeToSchemaMap.Count > 0)
        ? nodeToSchemaMap.Keys.ToList()
        : new List<string>() {MainNodeId};

      foreach (var storageNode in storageNodes) {
        using (var session = domain.OpenSession()) {
          session.SelectStorageNode(storageNode);
          using (var transaction = session.OpenTransaction()) {
            var euro = new Currency() {Name = "Euro", ShortName = "EU"};
            var dollar = new Currency() {Name = "Dollar", ShortName = "USD"};
            var ruble = new Currency() {Name = "Ruble", ShortName = "RUB"};

            var product1 = new Product() {Name = "Product 1"};
            var product2 = new Product() {Name = "Product 2"};
            var product3 = new Product() {Name = "Product 3"};

            var priceList1 = new PriceList() {CreatedOn = DateTime.Now, IsArchived = false};
            priceList1.Items.Add(new PriceListItem() {Currency = ruble, Product = product1, Price = 1500});
            priceList1.Items.Add(new PriceListItem() {Currency = ruble, Product = product2, Price = 15000});
            priceList1.Items.Add(new PriceListItem() {Currency = ruble, Product = product3, Price = 150000});
            transaction.Complete();
          }
        }
      }
    }

    private void ValidateDomainData(Domain domain)
    {
      var storageNodes = (nodeToSchemaMap.Count > 0)
        ? nodeToSchemaMap.Keys.ToList()
        : new List<string>() {MainNodeId};

      foreach (var storageNode in storageNodes) {
        using (var session = domain.OpenSession()) {
          session.SelectStorageNode(storageNode);
          using (var transaction = session.OpenTransaction()) {
            var currencies = session.Query.All<Currency>().OrderBy(c => c.Name).ToArray();
            Assert.That(currencies.Length, Is.EqualTo(3));
            Assert.That(currencies[0].Name, Is.EqualTo("Dollar"));
            Assert.That(currencies[1].Name, Is.EqualTo("Euro"));
            Assert.That(currencies[2].Name, Is.EqualTo("Ruble"));

            var products = session.Query.All<Product>().OrderBy(c => c.Name).ToArray();
            Assert.That(products.Length, Is.EqualTo(3));
            Assert.That(products[0].Name, Is.EqualTo("Product 1"));
            Assert.That(products[1].Name, Is.EqualTo("Product 2"));
            Assert.That(products[2].Name, Is.EqualTo("Product 3"));

            var priceList = session.Query.All<PriceList>().Single();
            Assert.That(priceList.IsArchived, Is.False);
            Assert.That(priceList.Items.Count(), Is.EqualTo(3));
            Assert.That(priceList.Items.All(i => i.Currency==currencies[2]), Is.True);
          }
        }
      }
    }

    #endregion

    #region Catalog-level operations

    private void ValidateInitialCatalog(Catalog catalog)
    {
      var validatableSchemas = new List<Schema>();
      if (nodeToSchemaMap.Count > 0) {
        validatableSchemas.Add(catalog.Schemas[nodeToSchemaMap[MainNodeId]]);
        validatableSchemas.Add(catalog.Schemas[nodeToSchemaMap[AdditionalNodeId]]);
      }
      else
        validatableSchemas.Add(catalog.DefaultSchema);

      foreach (var schema in validatableSchemas) {
        var productTable = schema.Tables["Product"];
        Assert.That(productTable, Is.Not.Null);
        Assert.That(productTable.TableColumns["Id"], Is.Not.Null);
        Assert.That(productTable.TableColumns["Name"], Is.Not.Null);

        var priceListTable = schema.Tables["PriceList"];
        Assert.That(priceListTable, Is.Not.Null);
        Assert.That(priceListTable.TableColumns["Id"], Is.Not.Null);
        Assert.That(priceListTable.TableColumns["CreatedOn"], Is.Not.Null);
        Assert.That(priceListTable.TableColumns["IsArchived"], Is.Not.Null);

        var priceListItemTable = schema.Tables["PriceListItem"];
        Assert.That(priceListItemTable, Is.Not.Null);
        Assert.That(priceListItemTable.TableColumns["Id"], Is.Not.Null);
        Assert.That(priceListItemTable.TableColumns["Price"], Is.Not.Null);
        Assert.That(priceListItemTable.TableColumns["PriceList.Id"], Is.Not.Null);
        Assert.That(priceListItemTable.TableColumns["Product.Id"], Is.Not.Null);
        Assert.That(priceListItemTable.TableColumns["Currency.Id"], Is.Not.Null);

        var currencyTable = schema.Tables["Currency"];
        Assert.That(currencyTable, Is.Not.Null);
        Assert.That(currencyTable.TableColumns["Id"], Is.Not.Null);
        Assert.That(currencyTable.TableColumns["Name"], Is.Not.Null);
        Assert.That(currencyTable.TableColumns["ShortName"], Is.Not.Null);
        Assert.That(currencyTable.TableColumns["Name"], Is.Not.Null);
        Assert.That(currencyTable.TableColumns["Symbol"], Is.Not.Null);
      }
    }

    private void ManuallyInsertIgnoredItems(Catalog catalog)
    {
      var validatableSchemas = new List<Schema>();
      if (nodeToSchemaMap.Count > 0) {
        validatableSchemas.Add(catalog.Schemas[nodeToSchemaMap[MainNodeId]]);
        validatableSchemas.Add(catalog.Schemas[nodeToSchemaMap[AdditionalNodeId]]);
      }
      else
        validatableSchemas.Add(catalog.DefaultSchema);

      foreach (var schema in validatableSchemas) {
        using (var connection = driver.CreateConnection()) {
          connection.Open();

          var productTable = schema.Tables["Product"];
          var hiddenNameColumn = productTable.CreateColumn("HiddenName", GetTypeForString(255));
          hiddenNameColumn.IsNullable = true;
          using (var command = connection.CreateCommand(driver.Compile(SqlDdl.Alter(productTable, SqlDdl.AddColumn(hiddenNameColumn))).GetCommandText()))
            command.ExecuteNonQuery();

          var priceListTable = schema.Tables["PriceList"];
          var hiddenCommentColumn = priceListTable.CreateColumn("HiddenComment", GetTypeForString(255));
          hiddenCommentColumn.IsNullable = true;
          using (var command = connection.CreateCommand(driver.Compile(SqlDdl.Alter(priceListTable, SqlDdl.AddColumn(hiddenCommentColumn))).GetCommandText()))
            command.ExecuteNonQuery();

          var currencyTable = schema.Tables["Currency"];
          var prefixColumnTemplate = "NotInDomain{0}";
          var columns = new[] {"Column1", "Column2", "Column3"};
          foreach (var column in columns) {
            var prefixColumn = currencyTable.CreateColumn(string.Format(prefixColumnTemplate, column), GetTypeForString(255));
            prefixColumn.IsNullable = true;
            using (var command = connection.CreateCommand(driver.Compile(SqlDdl.Alter(currencyTable, SqlDdl.AddColumn(prefixColumn))).GetCommandText()))
              command.ExecuteNonQuery();
          }

          var ignoredTable = schema.CreateTable("HiddenTable");
          var idColumn = ignoredTable.CreateColumn("Id", new SqlValueType(SqlType.Int64));
          idColumn.IsNullable = false;
          var name = ignoredTable.CreateColumn("Name", GetTypeForString(255));
          name.IsNullable = false;
          var pk = ignoredTable.CreatePrimaryKey("PK_HiddenTable", idColumn);

          using (var command = connection.CreateCommand(SqlDdl.Create(ignoredTable)))
            command.ExecuteNonQuery();

          var notInDomainTable1 = schema.CreateTable("NotInDomain1");
          idColumn = notInDomainTable1.CreateColumn("Id", new SqlValueType(SqlType.Int64));
          idColumn.IsNullable = false;
          name = notInDomainTable1.CreateColumn("Name", GetTypeForString(255));
          name.IsNullable = false;
          pk = notInDomainTable1.CreatePrimaryKey("PK_NotInDomain1", idColumn);

          using (var command = connection.CreateCommand(SqlDdl.Create(notInDomainTable1)))
            command.ExecuteNonQuery();

          var notInDomainTable2 = schema.CreateTable("NotInDomain2");
          idColumn = notInDomainTable2.CreateColumn("Id", new SqlValueType(SqlType.Int64));
          idColumn.IsNullable = false;
          name = notInDomainTable2.CreateColumn("Name", GetTypeForString(255));
          name.IsNullable = false;
          pk = notInDomainTable2.CreatePrimaryKey("PK_NotInDomain2", idColumn);

          using (var command = connection.CreateCommand(SqlDdl.Create(notInDomainTable2)))
            command.ExecuteNonQuery();

          var notInDomainTable3 = schema.CreateTable("NotInDomain3");
          idColumn = notInDomainTable3.CreateColumn("Id", new SqlValueType(SqlType.Int64));
          idColumn.IsNullable = false;
          name = notInDomainTable3.CreateColumn("Name", GetTypeForString(255));
          name.IsNullable = false;
          pk = notInDomainTable3.CreatePrimaryKey("PK_NotInDomain3", idColumn);

          using (var command = connection.CreateCommand(SqlDdl.Create(notInDomainTable3)))
            command.ExecuteNonQuery();
        }
      }
    }

    private void PopulateIgnoredData(Catalog catalog)
    {
      var validatableSchemas = new List<Schema>();
      if (nodeToSchemaMap.Count > 0) {
        validatableSchemas.Add(catalog.Schemas[nodeToSchemaMap[MainNodeId]]);
        validatableSchemas.Add(catalog.Schemas[nodeToSchemaMap[AdditionalNodeId]]);
      }
      else
        validatableSchemas.Add(catalog.DefaultSchema);

      foreach (var schema in validatableSchemas) {
        using (var connecton = driver.CreateConnection()) {
          connecton.Open();

          var productTable = schema.Tables["Product"];
          var @ref = SqlDml.TableRef(productTable);
          var update = SqlDml.Update(@ref);
          update.Values.Add(@ref["HiddenName"], SqlDml.Literal("Hidden name"));
          using (var command = connecton.CreateCommand(update))
            command.ExecuteNonQuery();

          var priceListTable = schema.Tables["PriceList"];
          @ref = SqlDml.TableRef(priceListTable);
          update = SqlDml.Update(@ref);
          update.Values.Add(@ref["HiddenComment"], SqlDml.Literal("Some hidden comment"));
          using (var command = connecton.CreateCommand(update))
            command.ExecuteNonQuery();

          var currencyTable = schema.Tables["Currency"];
          @ref = SqlDml.TableRef(currencyTable);
          update = SqlDml.Update(@ref);
          update.Values.Add(@ref["NotInDomainColumn1"], SqlDml.Literal("Not in domain"));
          update.Values.Add(@ref["NotInDomainColumn2"], SqlDml.Literal("Not in domain"));
          update.Values.Add(@ref["NotInDomainColumn3"], SqlDml.Literal("Not in domain"));
          using (var command = connecton.CreateCommand(update))
            command.ExecuteNonQuery();
        }
      }
    }

    private void ValidateModifiedCatalogs(Catalog catalog)
    {
      var validatableSchemas = new List<Schema>();
      if (nodeToSchemaMap.Count > 0) {
        validatableSchemas.Add(catalog.Schemas[nodeToSchemaMap[MainNodeId]]);
        validatableSchemas.Add(catalog.Schemas[nodeToSchemaMap[AdditionalNodeId]]);
      }
      else
        validatableSchemas.Add(catalog.DefaultSchema);

      foreach (var schema in validatableSchemas) {
        var productTable = schema.Tables["Product"];
        Assert.That(productTable, Is.Not.Null);
        Assert.That(productTable.TableColumns["Id"], Is.Not.Null);
        Assert.That(productTable.TableColumns["Name"], Is.Not.Null);
        Assert.That(productTable.TableColumns["HiddenName"], Is.Not.Null);

        var priceListTable = schema.Tables["PriceList"];
        Assert.That(priceListTable, Is.Not.Null);
        Assert.That(priceListTable.TableColumns["Id"], Is.Not.Null);
        Assert.That(priceListTable.TableColumns["CreatedOn"], Is.Not.Null);
        Assert.That(priceListTable.TableColumns["IsArchived"], Is.Not.Null);
        Assert.That(priceListTable.TableColumns["HiddenComment"], Is.Not.Null);

        var priceListItemTable = schema.Tables["PriceListItem"];
        Assert.That(priceListItemTable, Is.Not.Null);
        Assert.That(priceListItemTable.TableColumns["Id"], Is.Not.Null);
        Assert.That(priceListItemTable.TableColumns["Price"], Is.Not.Null);
        Assert.That(priceListItemTable.TableColumns["PriceList.Id"], Is.Not.Null);
        Assert.That(priceListItemTable.TableColumns["Product.Id"], Is.Not.Null);
        Assert.That(priceListItemTable.TableColumns["Currency.Id"], Is.Not.Null);

        var currencyTable = schema.Tables["Currency"];
        Assert.That(currencyTable, Is.Not.Null);
        Assert.That(currencyTable.TableColumns["Id"], Is.Not.Null);
        Assert.That(currencyTable.TableColumns["Name"], Is.Not.Null);
        Assert.That(currencyTable.TableColumns["ShortName"], Is.Not.Null);
        Assert.That(currencyTable.TableColumns["Name"], Is.Not.Null);
        Assert.That(currencyTable.TableColumns["Symbol"], Is.Not.Null);
        Assert.That(currencyTable.TableColumns["NotInDomainColumn1"], Is.Not.Null);
        Assert.That(currencyTable.TableColumns["NotInDomainColumn2"], Is.Not.Null);
        Assert.That(currencyTable.TableColumns["NotInDomainColumn3"], Is.Not.Null);

        var hiddenTable = schema.Tables["HiddenTable"];
        Assert.That(hiddenTable, Is.Not.Null);
        Assert.That(hiddenTable.TableColumns["Id"], Is.Not.Null);
        Assert.That(hiddenTable.TableColumns["Name"], Is.Not.Null);

        var notInDomain1 = schema.Tables["NotInDomain1"];
        Assert.That(notInDomain1, Is.Not.Null);
        Assert.That(notInDomain1.TableColumns["Id"], Is.Not.Null);
        Assert.That(notInDomain1.TableColumns["Name"], Is.Not.Null);

        var notInDomain2 = schema.Tables["NotInDomain1"];
        Assert.That(notInDomain2, Is.Not.Null);
        Assert.That(notInDomain2.TableColumns["Id"], Is.Not.Null);
        Assert.That(notInDomain2.TableColumns["Name"], Is.Not.Null);

        var notInDomain3 = schema.Tables["NotInDomain1"];
        Assert.That(notInDomain3, Is.Not.Null);
        Assert.That(notInDomain3.TableColumns["Id"], Is.Not.Null);
        Assert.That(notInDomain3.TableColumns["Name"], Is.Not.Null);
      }
    }

    private void ValidateIgnoredData(Catalog catalog)
    {
      var validatableSchemas = new List<Schema>();
      if (nodeToSchemaMap.Count > 0) {
        validatableSchemas.Add(catalog.Schemas[nodeToSchemaMap[MainNodeId]]);
        validatableSchemas.Add(catalog.Schemas[nodeToSchemaMap[AdditionalNodeId]]);
      }
      else
        validatableSchemas.Add(catalog.DefaultSchema);

      foreach (var schema in validatableSchemas) {
        using (var connection = driver.CreateConnection()) {
          connection.Open();

          var productTable = schema.Tables["Product"];
          var @ref = SqlDml.TableRef(productTable);
          var selectAll = SqlDml.Select(@ref);
          selectAll.Columns.Add(SqlDml.Column(SqlDml.Count()), "c0unt");

          var selectWhere = SqlDml.Select(@ref);
          selectWhere.Columns.Add(SqlDml.Column(SqlDml.Count()), "c0unt");
          selectWhere.Where = @ref["HiddenName"]==SqlDml.Literal("Hidden name");
          using (var expectedCountCommand = connection.CreateCommand(selectAll))
          using (var actualCountCommand = connection.CreateCommand(selectWhere)) {
            var expectedCount = Convert.ToInt64(expectedCountCommand.ExecuteScalar());
            var actualCount = Convert.ToInt64(actualCountCommand.ExecuteScalar());

            Assert.That(actualCount, Is.EqualTo(expectedCount));
          }

          var priceListTable = schema.Tables["PriceList"];
          @ref = SqlDml.TableRef(priceListTable);
          selectAll = SqlDml.Select(@ref);
          selectAll.Columns.Add(SqlDml.Column(SqlDml.Count()), "c0unt");

          selectWhere = SqlDml.Select(@ref);
          selectWhere.Columns.Add(SqlDml.Column(SqlDml.Count()), "c0unt");
          selectWhere.Where = @ref["HiddenComment"]==SqlDml.Literal("Some hidden comment");
          using (var expectedCountCommand = connection.CreateCommand(selectAll))
          using (var actualCountCommand = connection.CreateCommand(selectWhere)) {
            var expectedCount = Convert.ToInt64(expectedCountCommand.ExecuteScalar());
            var actualCount = Convert.ToInt64(actualCountCommand.ExecuteScalar());

            Assert.That(actualCount, Is.EqualTo(expectedCount));
          }

          var currencyTable = schema.Tables["Currency"];
          @ref = SqlDml.TableRef(currencyTable);
          selectAll = SqlDml.Select(@ref);
          selectAll.Columns.Add(SqlDml.Column(SqlDml.Count()), "c0unt");

          selectWhere = SqlDml.Select(@ref);
          selectWhere.Columns.Add(SqlDml.Column(SqlDml.Count()), "c0unt");
          selectWhere.Where = @ref["NotInDomainColumn1"]=="Not in domain" && @ref["NotInDomainColumn1"]=="Not in domain" && @ref["NotInDomainColumn1"]=="Not in domain";

          using (var expectedCountCommand = connection.CreateCommand(selectAll))
          using (var actualCountCommand = connection.CreateCommand(selectWhere)) {
            var expectedCount = Convert.ToInt64(expectedCountCommand.ExecuteScalar());
            var actualCount = Convert.ToInt64(actualCountCommand.ExecuteScalar());

            Assert.That(actualCount, Is.EqualTo(expectedCount));
          }
        }
      }
    }

    #endregion

    private IgnoreRuleCollection GetIgnoreRules()
    {
      var collection = new IgnoreRuleCollection();
      collection.IgnoreTable("HiddenTable");
      collection.IgnoreTable("NotInDomain*");
      collection.IgnoreColumn("NotInDomain*").WhenTable("Currency");
      collection.IgnoreColumn("HiddenComment").WhenTable("PriceList");
      collection.IgnoreColumn("HiddenName").WhenTable("Product");

      return collection;
    }

    private SqlValueType GetTypeForString(int? length)
    {
      return driver.TypeMappings.Mappings[typeof (string)].MapType(length, null, null);
    }

    private Dictionary<string, string> BuildNodeToSchemaMap()
    {
      if (ProviderInfo.Supports(ProviderFeatures.Multischema))
        return new Dictionary<string, string> {{MainNodeId, "Model1"}, {AdditionalNodeId, "Model2"}};
      return new Dictionary<string, string>();
    }

    private Catalog ExtractCatalog()
    {
      using (var connection = driver.CreateConnection()) {
        connection.Open();
        if (nodeToSchemaMap.Count > 0)
          return driver.ExtractCatalog(connection);
        return driver.ExtractDefaultSchema(connection).Catalog;
      }
    }
  }
}

