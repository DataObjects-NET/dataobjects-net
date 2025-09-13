// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.07.23

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0765_LinqTranslationError_Model;
using System.Linq;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0765_LinqTranslationError_Model
  {
    [HierarchyRoot]
    public class Document : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field(Length = Int32.MaxValue)]
      public string FullText { get; set; }
    }

    public class Invoice : Document
    {
      [Field(Length = 80)]
      public String DocumentId { get; set; }

      [Field(Length = 80)]
      public String Supplier { get; set; }
    }

    [HierarchyRoot]
    public class InvoiceExtract : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public Invoice Owner { get; set; }

      [Field]
      public string Name { get; set; }

      [Field(Length = Int32.MaxValue)]
      public string FullText { get; set; }
    }
  }

  [Serializable]
  public class Issue0765_LinqTranslationError : AutoBuildTest
  {

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof(Document).Assembly, typeof(Document).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var invoice = new Invoice {
          DocumentId = "test",
          Supplier = "supplier"
        };
        var extract = new InvoiceExtract { Owner = invoice };

        // Committing transaction
        t.Complete();
      }

      // Reading all persisted objects from another Session
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var groupByQuery = session.Query.All<Invoice>()
            .Join(session.Query.All<InvoiceExtract>(), i => i, e => e.Owner, (i, e) => new { Invoice = i, Extract = e })
            .GroupBy(c => c.Invoice.DocumentId);
        var result = groupByQuery.ToList();
      }
    }
  }
}