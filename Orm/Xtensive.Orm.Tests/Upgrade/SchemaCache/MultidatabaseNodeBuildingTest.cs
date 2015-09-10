// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.09.04

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Model1 = Xtensive.Orm.Tests.Upgrade.ExtractedSchemaCaching.MultidatabaseNodeBuildingTestModel.Model1;
using Model2 = Xtensive.Orm.Tests.Upgrade.ExtractedSchemaCaching.MultidatabaseNodeBuildingTestModel.Model2;

namespace Xtensive.Orm.Tests.Upgrade.ExtractedSchemaCaching.MultidatabaseNodeBuildingTestModel
{
  namespace Model1
  {
    [HierarchyRoot]
    public class TestEntity : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string TestField { get; set; }
    }
  }

  namespace Model2
  {
    public interface IPriceable
    {
      double Price { get; }
    }

    public enum ServiceKind
    {
      Technical,
      Juriditial,
      Other
    }

    public abstract class BaseEntity : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field(Nullable = false)]
      public DateTime CreationDate { get; set; }

      public BaseEntity(Session session)
        : base(session)
      {
        CreationDate = DateTime.Now;
      }
    }

    [HierarchyRoot(InheritanceSchema.SingleTable)]
    [Index("DocumentNumber", Unique = true)]
    public abstract class Invoice : BaseEntity
    {
      [Field(Nullable = false)]
      public string DocumentNumber { get; set; }

      [Field]
      public EntitySet<InvoiceItem> Items { get; set; }

      public Invoice(Session session)
        : base(session)
      { }
    }

    public class RealInvoice : Invoice
    {
      [Field]
      public string Comment { get; set; }

      public RealInvoice(Session session)
        : base(session)
      { }
    }

    [HierarchyRoot]
    public abstract class InvoiceItem : BaseEntity, IPriceable
    {
      [Field(Nullable = false, DefaultValue = 0.00d)]
      public double Price { get; set; }

      public InvoiceItem(Session session)
        : base(session)
      { }
    }

    public class ServiceItem : InvoiceItem
    {
      [Field]
      public Service Service { get; set; }

      public ServiceItem(Session session)
        : base(session)
      {
      }
    }

    public class ProductItem : InvoiceItem
    {
      [Field]
      public Product Product { get; set; }

      [Field(Nullable = false)]
      public int Count { get; set; }

      [Field]
      public Measure Measure { get; set; }

      public ProductItem(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class Service : BaseEntity
    {
      [Field(Nullable = false, Indexed = true)]
      public string Name { get; set; }

      [Field(Nullable = false)]
      public ServiceKind ServiceKind { get; set; }

      [Field(Nullable = false, DefaultValue = false)]
      public bool IsArchived { get; set; }

      public Service(Session session)
        : base(session)
      { }
    }

    [HierarchyRoot]
    public class Product : BaseEntity
    {
      [Field(Nullable = false, Indexed = true)]
      public string Name { get; set; }

      [Field(Nullable = false, DefaultValue = false)]
      public bool IsArchived { get; set; }

      public Product(Session session)
        : base(session)
      { }
    }

    [HierarchyRoot]
    public class Measure : BaseEntity
    {
      [Field]
      public string Name { get; set; }

      public Measure(Session session)
        : base(session)
      { }
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.ExtractedSchemaCaching
{
  public class MultidatabaseNodeBuildingTest : NodeBuildingTest
  {
    protected override DomainConfiguration GetDomainConfiguration(DomainUpgradeMode upgradeMode)
    {
      var configuration = BuildConfiguration();
      configuration.DefaultDatabase = "DO-Tests-1";
      configuration.DefaultSchema = "dbo";
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(GetType().Assembly, typeof(Model2.BaseEntity).Namespace);
      configuration.Types.Register(GetType().Assembly, typeof(Model1.TestEntity).Namespace);
      configuration.MappingRules.Map(GetType().Assembly, typeof(Model2.BaseEntity).Namespace).To("DO-Tests-1", "dbo");
      configuration.MappingRules.Map(GetType().Assembly, typeof(Model1.TestEntity).Namespace).To("DO-Tests-2", "dbo");

      return configuration;
    }

    protected override NodeConfiguration GetFirstNodeConfiguration(DomainUpgradeMode upgradeMode)
    {
      var nodeConfiguration = new NodeConfiguration("Test1");
      nodeConfiguration.UpgradeMode = upgradeMode;
      nodeConfiguration.DatabaseMapping.Add("DO-Tests-1", "DO-Tests-3");
      nodeConfiguration.DatabaseMapping.Add("DO-Tests-2", "DO-Tests-4");
      return nodeConfiguration;
    }

    protected override NodeConfiguration GetSecondNodeConfiguration(DomainUpgradeMode upgradeMode)
    {
      return null;
    }

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }

    protected override void RunQueries(Domain domain)
    {
      RunQueriesForNode(domain, WellKnown.DefaultNodeId);
      RunQueriesForNode(domain, "Test1");
    }

    private void RunQueriesForNode(Domain domain, string storageNodeId)
    {
      CreateQueries(domain, storageNodeId);
      SelectQueries(domain, storageNodeId);
      ChangeQueries(domain, storageNodeId);
      RemoveQueries(domain, storageNodeId);
    }

    private void CreateQueries(Domain domain, string storageNodeId)
    {
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.SelectStorageNode(storageNodeId);
        var measures = (new string[]{"kg", "item", "box"}).Select(el => new Model2.Measure(session){Name = el}).ToList();

        var invoice = new Model2.RealInvoice(session){DocumentNumber = "1"};
        invoice.Comment = "Initial comment";

        int counter = 0;
        foreach (var measure in measures) {
          invoice.Items.Add(new Model2.ProductItem(session){
            Product = new Model2.Product(session){
              Name = string.Format("Product which measure is {0}", measure.Name),
            },
            Count = counter,
            Price = counter * 0.89
          });
          invoice.Items.Add(new Model2.ProductItem(session){
            Product = new Model2.Product(session){
              Name = string.Format("Another product which measure is {0}", measure.Name),
            },
            Count = counter,
            Price = counter * 0.89
          });
          counter++;
        }

        for (var i = 0; i < 6; i++) {
          invoice.Items.Add(new Model2.ServiceItem(session){
            Service = new Model2.Service(session){
              Name = string.Format("Service #{0}", i),
            },
            Price = (i + 1) * 0.89
          });
        }
        transaction.Complete();
      }
    }

    private void SelectQueries(Domain domain, string storageNode)
    {
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.SelectStorageNode(storageNode);
        Assert.That(session.Query.All<Model2.Measure>().Count(), Is.EqualTo(3));
        Assert.That(session.Query.All<Model2.Product>().Count(), Is.EqualTo(6));
        Assert.That(session.Query.All<Model2.ProductItem>().Count(), Is.EqualTo(6));
        Assert.That(session.Query.All<Model2.Service>().Count(), Is.EqualTo(6));
        Assert.That(session.Query.All<Model2.ServiceItem>().Count(), Is.EqualTo(6));
        Assert.That(session.Query.All<Model2.InvoiceItem>().Count(), Is.EqualTo(12));
        Assert.That(session.Query.All<Model2.Invoice>().Count(), Is.EqualTo(1));
        Assert.That(session.Query.All<Model2.Invoice>().First().Items.Count(), Is.EqualTo(12));
        transaction.Complete();
      }
    }

    private void ChangeQueries(Domain domain, string storageNode)
    {
      var oldPrices = new Dictionary<int, double>();
      using (var session = domain.OpenSession()) {
        session.SelectStorageNode(storageNode);
        using (var transaction = session.OpenTransaction()) {

          foreach (var item in session.Query.All<Model2.InvoiceItem>().OrderBy(el => el.Id)) {
            oldPrices.Add(item.Id, item.Price);
            item.Price += 23;
          }
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          foreach (var item in session.Query.All<Model2.InvoiceItem>().OrderBy(el => el.Id)) {
            Assert.That(oldPrices[item.Id] + 23, Is.EqualTo(item.Price));
          }
        }
      }
    }

    private void RemoveQueries(Domain domain, string storageNode)
    {
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.SelectStorageNode(storageNode);
        var invoice = session.Query.All<Model2.Invoice>().First();
        invoice.Items.Clear();

        foreach (var invoiceItem in session.Query.All<Model2.InvoiceItem>()) {
          var serviceItem = invoiceItem as Model2.ServiceItem;
          if (serviceItem!=null) {
            var service = serviceItem.Service;
            serviceItem.Service = null;
            service.Remove();
            serviceItem.Remove();
          }
          else {
            var productItem = invoiceItem as Model2.ProductItem;
            var product = productItem.Product;
            productItem.Product = null;
            productItem.Remove();
            product.Remove();
          }
        }

        foreach (var measure in session.Query.All<Model2.Measure>())
          measure.Remove();

        transaction.Complete();
      }
    }
  }
}
