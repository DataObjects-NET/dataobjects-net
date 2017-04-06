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
using Xtensive.Orm.Tests.Upgrade.SchemaSharing.Model;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing
{
  public class StorageSchemaCapturer
  {
    private Dictionary<UpgradeStage, ModelMapping> mappings = new Dictionary<UpgradeStage, ModelMapping>();

    public void Register(UpgradeStage stage, ModelMapping mapping)
    {
      mappings.Add(stage, mapping);
    }

    public ModelMapping GetMappingForStage(UpgradeStage stage)
    {
      ModelMapping mapping;
      if (!mappings.TryGetValue(stage, out mapping))
        throw new Exception(string.Format("There is no model mappings for the {0} stage ", stage));
      return mapping;
    }

    public void Reset()
    {
      mappings.Clear();
    }
  }

  // Captures model mappings of both upgrading and final nodes for one build whether domain or node
  public class Upgrader : UpgradeHandler
  {
    private ModelMapping upgadingStageMapping;
    private ModelMapping finalStageMapping;

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnStage()
    {
      var stage = UpgradeContext.Stage;
      if (stage==UpgradeStage.Final)
        finalStageMapping = Session.Current.StorageNode.Mapping;
      else
        upgadingStageMapping = Session.Current.StorageNode.Mapping;
    }

    public override void OnComplete(Domain domain)
    {
      var mappingCapturer = domain.Extensions.Get<StorageSchemaCapturer>();
      if (mappingCapturer==null) {
        mappingCapturer = new StorageSchemaCapturer();
        domain.Extensions.Set(mappingCapturer);
      }
      mappingCapturer.Reset();
      mappingCapturer.Register(UpgradeStage.Upgrading, upgadingStageMapping);
      mappingCapturer.Register(UpgradeStage.Final, finalStageMapping);
    }
  }


  public class StorageNodeBuildingTest : AutoBuildTest
  {
    private const string MainNodeId = WellKnown.DefaultNodeId;
    private const string AdditionalNodeId = "Additional";

    private Dictionary<string, string> nodeToSchemaMap;

    public override void TestFixtureSetUp()
    {
      ProviderInfo = StorageProviderInfo.Instance.Info;
      nodeToSchemaMap = BuildNodeToSchemaMap();
    }

    [Test]
    public void RecreateTestWithoutSharingTest()
    {
      RunTest(DomainUpgradeMode.Recreate, false);
    }

    [Test]
    public void RecreateTestWithSharingTest()
    {
      RunTest(DomainUpgradeMode.Recreate, true);
    }

    [Test]
    public void PerformSafelyWithoutChangesWithoutSharingTest()
    {
      RunTest(DomainUpgradeMode.PerformSafely, false, false);
    }

    [Test]
    public void PerformSafelyWithoutChangesWithSharingTest()
    {
      RunTest(DomainUpgradeMode.PerformSafely, true, false);

    }

    [Test]
    public void PerformSafelyWithChangesWithoutSharing()
    {
      RunTest(DomainUpgradeMode.PerformSafely, false, true);
    }

    [Test]
    public void PerformSafelyWithChangesWithSharing()
    {
      RunTest(DomainUpgradeMode.PerformSafely, true, true);
    }

    [Test]
    public void PerformWithoutChangesWithoutSharingTest()
    {
      RunTest(DomainUpgradeMode.Perform, false, false);
    }

    [Test]
    public void PerformWithoutChangesWithSharingTest()
    {
      RunTest(DomainUpgradeMode.Perform, true, false);
    }

    [Test]
    public void PerformWithChangesWithSharingTest()
    {
      RunTest(DomainUpgradeMode.Perform, true, true);
    }

    [Test]
    public void PerformWithChangesWithoutSharingTest()
    {
      RunTest(DomainUpgradeMode.Perform, false, true);
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
    public void LegacySkipWithountSharingTest()
    {
      RunTest(DomainUpgradeMode.LegacySkip, false);
    }

    [Test]
    public void LegacySkipWithSharingTest()
    {
      RunTest(DomainUpgradeMode.LegacySkip, true);
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

    private void RunTest(DomainUpgradeMode upgradeMode, bool shareStorageSchemaOverNodes, bool withModelChanges = false)
    {
      if (upgradeMode==DomainUpgradeMode.Recreate) {
        RunRecreateTest(shareStorageSchemaOverNodes);
        return;
      }

      var isMultinode = nodeToSchemaMap.Count > 0;
      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      if (isMultinode)
        configuration.DefaultSchema = nodeToSchemaMap[MainNodeId];

      using (var initialDomain = BuildDomain(configuration)) {
        PopulateData(initialDomain, MainNodeId);
        if (isMultinode) {
          var nodeConfiguration = new NodeConfiguration(AdditionalNodeId);
          nodeConfiguration.SchemaMapping.Add(nodeToSchemaMap[MainNodeId], nodeToSchemaMap[AdditionalNodeId]);
          nodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
          initialDomain.StorageNodeManager.AddNode(nodeConfiguration);
          PopulateData(initialDomain, AdditionalNodeId);
        }
      }

      configuration = BuildConfiguration();
      configuration.UpgradeMode = upgradeMode;
      configuration.ShareStorageSchemaOverNodes = shareStorageSchemaOverNodes;
      configuration.Types.Register(typeof (Upgrader));
      if ((upgradeMode==DomainUpgradeMode.Perform || upgradeMode==DomainUpgradeMode.PerformSafely) && withModelChanges)
        configuration.Types.Register(typeof (TypeForUgrade)); // that gives us schema cache reset
      if (isMultinode)
        configuration.DefaultSchema = nodeToSchemaMap[MainNodeId];

      using (var domain = BuildDomain(configuration)) {
        ValidateMappings(domain, false);
        ValidateData(domain, MainNodeId);

        if (isMultinode) {
          var nodeConfiguration = new NodeConfiguration(AdditionalNodeId);
          nodeConfiguration.UpgradeMode = upgradeMode;
          nodeConfiguration.SchemaMapping.Add(nodeToSchemaMap[MainNodeId], nodeToSchemaMap[AdditionalNodeId]);
          domain.StorageNodeManager.AddNode(nodeConfiguration);

          ValidateMappings(domain, true);
          ValidateData(domain, AdditionalNodeId);
        }
      }
    }

    private void RunRecreateTest(bool shareStorageSchemaOverNodes)
    {
      var isMultinode = nodeToSchemaMap.Count > 0;

      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.ShareStorageSchemaOverNodes = shareStorageSchemaOverNodes;
      configuration.Types.Register(typeof (Upgrader));
      if (isMultinode)
        configuration.DefaultSchema = nodeToSchemaMap[MainNodeId];

      using (var domain = BuildDomain(configuration)) {
        ValidateMappings(domain, false);
        PopulateData(domain, MainNodeId);
        ValidateData(domain, MainNodeId);

        if (isMultinode) {
          var nodeConfiguration = new NodeConfiguration(AdditionalNodeId);
          nodeConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
          nodeConfiguration.SchemaMapping.Add(nodeToSchemaMap[MainNodeId], nodeToSchemaMap[AdditionalNodeId]);
          domain.StorageNodeManager.AddNode(nodeConfiguration);

          ValidateMappings(domain, true);
          if (shareStorageSchemaOverNodes)
            ValidateNodesShareSchema(domain);
          PopulateData(domain, AdditionalNodeId);
          ValidateData(domain, AdditionalNodeId);
        }
      }
    }

    private void PopulateData(Domain domain, string nodeId)
    {
      using (var session = domain.OpenSession()) {
        session.SelectStorageNode(nodeId);
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

    private void ValidateMappings(Domain domain, bool isAdditionalNode)
    {
      var capturer = domain.Extensions.Get<StorageSchemaCapturer>();
      ValidateFinalMappings(domain, capturer.GetMappingForStage(UpgradeStage.Final));
      if (domain.Configuration.UpgradeMode.IsMultistage())
        ValidateUpgradingMapping(domain, capturer.GetMappingForStage(UpgradeStage.Upgrading), isAdditionalNode);
    }

    private void ValidateNodesShareSchema(Domain domain)
    {
      var defaultNodeMapping = domain.StorageNodeManager.GetNode(MainNodeId).Mapping;
      var additionalNodeMapping = domain.StorageNodeManager.GetNode(AdditionalNodeId).Mapping;
      var entities = domain.Model.Types.Entities.Where(e => !e.IsSystem);
      foreach (var typeInfo in entities) {
        Assert.That(defaultNodeMapping[typeInfo], Is.SameAs(additionalNodeMapping[typeInfo]));
      }
    }

    private void ValidateData(Domain domain, string nodeId)
    {
      using (var session = domain.OpenSession()) {
        session.SelectStorageNode(nodeId);
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

    private void ValidateFinalMappings(Domain domain, ModelMapping mapping)
    {
      var readingNamesDenied = domain.Configuration.ShareStorageSchemaOverNodes;
      foreach (var schemaNode in mapping.GetAllSchemaNodes()) {
        Assert.DoesNotThrow(() => { var schema = schemaNode.Schema; });
        Assert.DoesNotThrow(() => { var catalog = schemaNode.Schema.Catalog; });
        Assert.DoesNotThrow(() => { var name = schemaNode.Name; });
        Assert.DoesNotThrow(() => { var dbName = schemaNode.DbName; });
        if (readingNamesDenied) {
          Assert.Throws<InvalidOperationException>(() => { var schemaName = schemaNode.Schema.Name; });
          Assert.Throws<InvalidOperationException>(() => { var schemaDbName = schemaNode.Schema.DbName; });
          Assert.Throws<InvalidOperationException>(() => { var catalogName = schemaNode.Schema.Catalog.Name; });
          Assert.Throws<InvalidOperationException>(() => { var catalogDbName = schemaNode.Schema.Catalog.DbName; });
        }
        else {
          Assert.DoesNotThrow(() => { var schemaName = schemaNode.Schema.Name; });
          Assert.DoesNotThrow(() => { var schemaDbName = schemaNode.Schema.DbName; });
          Assert.DoesNotThrow(() => { var catalogName = schemaNode.Schema.Catalog.Name; });
          Assert.DoesNotThrow(() => { var catalogDbName = schemaNode.Schema.Catalog.DbName; });
        }
      }
    }

    private void ValidateUpgradingMapping(Domain domain, ModelMapping mapping, bool isAdditionalNode)
    {
      foreach (var schemaNode in mapping.GetAllSchemaNodes()) {
        Assert.DoesNotThrow(() => { var schema = schemaNode.Schema; });
        Assert.DoesNotThrow(() => { var catalog = schemaNode.Schema.Catalog; });
        Assert.DoesNotThrow(() => { var name = schemaNode.Name; });
        Assert.DoesNotThrow(() => { var dbName = schemaNode.DbName; });
        if (!domain.Configuration.ShareStorageSchemaOverNodes) {
          Assert.DoesNotThrow(() => { var schemaName = schemaNode.Schema.Name; });
          Assert.DoesNotThrow(() => { var schemaDbName = schemaNode.Schema.DbName; });
          Assert.DoesNotThrow(() => { var catalogName = schemaNode.Schema.Catalog.Name; });
          Assert.DoesNotThrow(() => { var catalogDbName = schemaNode.Schema.Catalog.DbName; });
        }
        else if (isAdditionalNode) {
          Assert.DoesNotThrow(() => { var schemaName = schemaNode.Schema.Name; });
          Assert.DoesNotThrow(() => { var schemaDbName = schemaNode.Schema.DbName; });
          Assert.DoesNotThrow(() => { var catalogName = schemaNode.Schema.Catalog.Name; });
          Assert.DoesNotThrow(() => { var catalogDbName = schemaNode.Schema.Catalog.DbName; });
        }
        else {
          Assert.Throws<InvalidOperationException>(() => { var schemaName = schemaNode.Schema.Name; });
          Assert.Throws<InvalidOperationException>(() => { var schemaDbName = schemaNode.Schema.DbName; });
          Assert.Throws<InvalidOperationException>(() => { var catalogName = schemaNode.Schema.Catalog.Name; });
          Assert.Throws<InvalidOperationException>(() => { var catalogDbName = schemaNode.Schema.Catalog.DbName; });
        }
      }
    }

    private Dictionary<string, string> BuildNodeToSchemaMap()
    {
      if (ProviderInfo.Supports(ProviderFeatures.Multischema))
        return new Dictionary<string, string> {{MainNodeId, "Model1"}, {AdditionalNodeId, "Model2"}};
      return new Dictionary<string, string>();
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Product));
      configuration.Types.Register(typeof (Currency));
      configuration.Types.Register(typeof (PriceList));
      configuration.Types.Register(typeof (PriceListItem));
      return configuration;
    }
  }
}
