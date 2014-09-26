// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.12.30

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.IssueJira0556_ArithmeticExceptionWhenSubstractionOfDatesModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0556_ArithmeticExceptionWhenSubstractionOfDatesModel
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public DateTime CreatedOn { get; set; }

    [Field]
    public DateTime InvoicedOn { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0556_ArithmeticExceptionWhenSubstractionOfDates : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.Types.Register(typeof(Invoice).Assembly, typeof(Invoice).Namespace);
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      return domainConfiguration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var tx = session.OpenTransaction()) {
        new Invoice {CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(10)};
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(100) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(1000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(10000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(100000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(106751) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(120000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(130000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(140000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(150000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(160000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(170000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(180000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(190000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(200000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(300000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(400000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(500000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(600000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(700000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(800000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(900000) };
        new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(1000000) };
        //new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MaxValue };
        //new Invoice { CreatedOn = DateTime.MaxValue, InvoicedOn = DateTime.MinValue.AddYears(100) };
        tx.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<Invoice>().Where(invoice => (invoice.CreatedOn - invoice.InvoicedOn) > TimeSpan.FromHours(1)).ToList();
      }
    }
  }
}
