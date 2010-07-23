// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.07.12

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0765_LinqTranslationError_Model;
using System.Linq;

namespace Xtensive.Storage.Tests.Issues
{
  namespace Issue0765_LinqTranslationError_Model
  {
    public interface IHasFullTextIndex : IEntity
    {
      [Field]
      bool IsFtEnabled { get; set; }

      [Field]
      bool? IsFtIndexUpToDate { get; set; }

      [Field(Length = Int32.MaxValue)]
      [FullText("French")]
      string FullText { get; set; }
    }

    [HierarchyRoot]
    public partial class Document : Entity, IHasFullTextIndex
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public bool IsFtEnabled { get; set; }

      [Field]
      public bool? IsFtIndexUpToDate { get; set; }

      [Field(Length = Int32.MaxValue)]
      public string FullText { get; set; }
    }

    public class Invoice : Document, IHasFullTextIndex
    {
      [Field(Length = 80)]
      public String DocumentId { get; set; }

      [Field(Length = 80)]
      public String Supplier { get; set; }

      public override string ToString()
      {
        string s = String.Format("{0}/{1} ", Supplier, DocumentId);
        return s + base.ToString();
      }
    }

    [HierarchyRoot]
    public class InvoiceExtract : Entity, IHasFullTextIndex
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public Invoice Owner { get; set; }

      [Field]
      public string Name { get; set; }

      #region Full-Text
      [Field]
      public bool IsFtEnabled { get; set; }

      [Field]
      public bool? IsFtIndexUpToDate { get; set; }

      [Field(Length = Int32.MaxValue)]
      public string FullText { get; set; }
      #endregion
    }
  }

  [Serializable]
  public class Issue0765_LinqTranslationError : AutoBuildTest
  {

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof(Document).Assembly, typeof(Document).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var invoice = new Invoice {
          DocumentId = "test",
          Supplier = "supplier"
        };
        var extract = new InvoiceExtract { Owner = invoice };

        // Committing transaction
        t.Complete();
      }

      // Reading all persisted objects from another Session
      using (Session.Open(Domain))
      using (var transactionScope = Transaction.Open()) {
        var groupByQuery = Query.All<Invoice>()
            .Join(Query.All<InvoiceExtract>(), i => i, e => e.Owner, (i, e) => new { Invoice = i, Extract = e })
            .GroupBy(c => c.Invoice.DocumentId);
        var result = groupByQuery.ToList();
      }
    }
  }
}