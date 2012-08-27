// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.08.27

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests.Linq.SubqueryAndComputedColumnOptimizationTestModel;

namespace Xtensive.Orm.Tests.Linq
{
  namespace SubqueryAndComputedColumnOptimizationTestModel
  {
    [HierarchyRoot]
    public class Product : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public string Description { get; set; }
    }

    [HierarchyRoot]
    public class ProductLocalization : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public string Description { get; set; }

      [Field]
      public string Culture { get; set; }

      [Field]
      public Product Product { get; set; }
    }
  }

  [TestFixture]
  public class SubqueryAndComputedColumnOptimizationTest : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Product).Assembly, typeof (Product).Namespace);
      return configuration;
    }

    private List<Pair<Product, string>> GetLocalizedProducts(Session session, string culture)
    {
      var query = session.Query.All<Product>()
        .Select(p => new {
          Product = p,
          Name = session.Query.All<ProductLocalization>()
            .Where(l => l.Product==p && l.Culture==culture)
            .Select(l => l.Name)
            .FirstOrDefault() ?? p.Name,
          Description = session.Query.All<ProductLocalization>()
            .Where(l => l.Product==p && l.Culture==culture)
            .Select(l => l.Description)
            .FirstOrDefault() ?? p.Description
        })
        .OrderBy(i => i.Name).ThenBy(i => i.Description);

      return query.AsEnumerable().Select(i => new Pair<Product, string>(i.Product, i.Name)).ToList();
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var product = new Product {
          Name = "DataObjects.Net",
          Description = "UberORM"
        };
        var localization = new ProductLocalization {
          Product = product,
          Name = "ДанныеОбъекты.Сеть",
          Description = "УберОРМ",
          Culture = "ru",
        };

        var russianProducts = GetLocalizedProducts(session, "ru");
        tx.Complete();
      }
    }
  }
}