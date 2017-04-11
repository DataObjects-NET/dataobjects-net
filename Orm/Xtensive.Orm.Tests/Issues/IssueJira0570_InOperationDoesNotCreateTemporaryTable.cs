// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.08.24

using System;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0570_InOperationDoesNotCreateTemporaryTableModel;
using Xtensive.Orm.Tests.Storage;

namespace Xtensive.Orm.Tests.Issues.IssueJira0570_InOperationDoesNotCreateTemporaryTableModel
{
  [HierarchyRoot]
  public class Payment : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public bool Active { get; set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public PaymentStatus Status { get; set; }

    [Field]
    public decimal Amount { get; set; }
  }

  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public bool Active { get; set; }

    [Field]
    public InvoiceStatus Status { get; set; }

    [Field]
    public DateTime? InvoicedOn { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Entity
  {
    [Field, Key]
    public int Id { get; set; }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public BusinessUnit BusinessUnit { get; set; }
  }

  [HierarchyRoot]
  public class BusinessUnit : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public bool Active { get; set; }

    [Field]
    public string QuickbooksClass { get; set; }
  }


  public enum InvoiceStatus
  {
    Pending,
    Queued,
    Authorized,
    Paid,
    Processing,
    Shipped,
    Cancelled,
    Refunded,
  }

  public enum PaymentStatus
  {
    Scheduled,
    Canceled,
    Processed,
    Failed,
    Paid,
    Retuned,
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0570_InOperationDoesNotCreateTemporaryTable : AutoBuildTest
  {
    private int[] customerIds;
    private PaymentStatus[] queryablePaymentStatuses;
    private InvoiceStatus[] queryableInvoiceStatuses;

    [Test]
    public void OnlyTemporaryTableTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var customers = new int[customerIds.Length];
        customerIds.CopyTo(customers, 0);

        var invoiceStatuses = new InvoiceStatus[queryableInvoiceStatuses.Length];
        queryableInvoiceStatuses.CopyTo(invoiceStatuses, 0);

        var paymentStatuses = new PaymentStatus[queryablePaymentStatuses.Length];
        queryablePaymentStatuses.CopyTo(paymentStatuses, 0);

        var paymentsQuery =
          from payment in session.Query.All<Payment>()
          join invoice in session.Query.All<Invoice>() on payment.Invoice equals invoice
          join customer in session.Query.All<Customer>() on invoice.Customer equals customer
          where invoice.Active &&
                invoice.Status.In(IncludeAlgorithm.TemporaryTable, invoiceStatuses) &&
                invoice.InvoicedOn.HasValue &&
                customer.Id.In(IncludeAlgorithm.TemporaryTable, customers) &&
                payment.Active &&
                payment.Status.In(IncludeAlgorithm.TemporaryTable, paymentStatuses)
          group payment.Amount by invoice.Id
            into amounts
            select new { Id = amounts.Key, Amount = amounts.Sum(e => e) };
        Assert.DoesNotThrow(paymentsQuery.Run);
      }
    }

    [Test]
    public void OnlyComplexConditionTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var customers = new int[customerIds.Length];
        customerIds.CopyTo(customers, 0);

        var invoiceStatuses = new InvoiceStatus[queryableInvoiceStatuses.Length];
        queryableInvoiceStatuses.CopyTo(invoiceStatuses, 0);

        var paymentStatuses = new PaymentStatus[queryablePaymentStatuses.Length];
        queryablePaymentStatuses.CopyTo(paymentStatuses, 0);

        var paymentsQuery =
          from payment in session.Query.All<Payment>()
          join invoice in session.Query.All<Invoice>() on payment.Invoice equals invoice
          join customer in session.Query.All<Customer>() on invoice.Customer equals customer
          where invoice.Active &&
                invoice.Status.In(IncludeAlgorithm.ComplexCondition, invoiceStatuses) &&
                invoice.InvoicedOn.HasValue &&
                customer.Id.In(IncludeAlgorithm.ComplexCondition, customers) &&
                payment.Active &&
                payment.Status.In(IncludeAlgorithm.ComplexCondition, paymentStatuses)
          group payment.Amount by invoice.Id
            into amounts
            select new { Id = amounts.Key, Amount = amounts.Sum(e => e) };
        Assert.DoesNotThrow(paymentsQuery.Run);
      }
    }

    [Test]
    public void TemporaryTableInTheBeginning()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var customers = new int[customerIds.Length];
        customerIds.CopyTo(customers, 0);

        var invoiceStatuses = new InvoiceStatus[queryableInvoiceStatuses.Length];
        queryableInvoiceStatuses.CopyTo(invoiceStatuses, 0);

        var paymentStatuses = new PaymentStatus[queryablePaymentStatuses.Length];
        queryablePaymentStatuses.CopyTo(paymentStatuses, 0);

        var paymentsQuery =
          from payment in session.Query.All<Payment>()
          join invoice in session.Query.All<Invoice>() on payment.Invoice equals invoice
          join customer in session.Query.All<Customer>() on invoice.Customer equals customer
          where invoice.Active &&
                invoice.Status.In(IncludeAlgorithm.TemporaryTable, invoiceStatuses) &&
                invoice.InvoicedOn.HasValue &&
                customer.Id.In(IncludeAlgorithm.ComplexCondition, customers) &&
                payment.Active &&
                payment.Status.In(IncludeAlgorithm.ComplexCondition, paymentStatuses)
          group payment.Amount by invoice.Id
            into amounts
            select new { Id = amounts.Key, Amount = amounts.Sum(e => e) };
        Assert.DoesNotThrow(paymentsQuery.Run);
      }
    }

    [Test]
    public void TemporaryTableInTheMiddle()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var customers = new int[customerIds.Length];
        customerIds.CopyTo(customers, 0);

        var invoiceStatuses = new InvoiceStatus[queryableInvoiceStatuses.Length];
        queryableInvoiceStatuses.CopyTo(invoiceStatuses, 0);

        var paymentStatuses = new PaymentStatus[queryablePaymentStatuses.Length];
        queryablePaymentStatuses.CopyTo(paymentStatuses, 0);

        var paymentsQuery =
          from payment in session.Query.All<Payment>()
          join invoice in session.Query.All<Invoice>() on payment.Invoice equals invoice
          join customer in session.Query.All<Customer>() on invoice.Customer equals customer
          where invoice.Active &&
                invoice.Status.In(IncludeAlgorithm.ComplexCondition, invoiceStatuses) &&
                invoice.InvoicedOn.HasValue &&
                customer.Id.In(IncludeAlgorithm.TemporaryTable, customers) &&
                payment.Active &&
                payment.Status.In(IncludeAlgorithm.ComplexCondition, paymentStatuses)
          group payment.Amount by invoice.Id
            into amounts
            select new { Id = amounts.Key, Amount = amounts.Sum(e => e) };
        Assert.DoesNotThrow(paymentsQuery.Run);
      }
    }

    [Test]
    public void TemporaryTableInTheEnd()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var customers = new int[customerIds.Length];
        customerIds.CopyTo(customers, 0);

        var invoiceStatuses = new InvoiceStatus[queryableInvoiceStatuses.Length];
        queryableInvoiceStatuses.CopyTo(invoiceStatuses, 0);

        var paymentStatuses = new PaymentStatus[queryablePaymentStatuses.Length];
        queryablePaymentStatuses.CopyTo(paymentStatuses, 0);

        var paymentsQuery =
          from payment in session.Query.All<Payment>()
          join invoice in session.Query.All<Invoice>() on payment.Invoice equals invoice
          join customer in session.Query.All<Customer>() on invoice.Customer equals customer
          where invoice.Active &&
                invoice.Status.In(IncludeAlgorithm.ComplexCondition, invoiceStatuses) &&
                invoice.InvoicedOn.HasValue &&
                customer.Id.In(IncludeAlgorithm.ComplexCondition, customers) &&
                payment.Active &&
                payment.Status.In(IncludeAlgorithm.TemporaryTable, paymentStatuses)
          group payment.Amount by invoice.Id
            into amounts
            select new { Id = amounts.Key, Amount = amounts.Sum(e => e) };
        Assert.DoesNotThrow(paymentsQuery.Run);
      }
    }

    [Test]
    public void ComplexConditionInTheBeginning()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var customers = new int[customerIds.Length];
        customerIds.CopyTo(customers, 0);

        var invoiceStatuses = new InvoiceStatus[queryableInvoiceStatuses.Length];
        queryableInvoiceStatuses.CopyTo(invoiceStatuses, 0);

        var paymentStatuses = new PaymentStatus[queryablePaymentStatuses.Length];
        queryablePaymentStatuses.CopyTo(paymentStatuses, 0);

        var paymentsQuery =
          from payment in session.Query.All<Payment>()
          join invoice in session.Query.All<Invoice>() on payment.Invoice equals invoice
          join customer in session.Query.All<Customer>() on invoice.Customer equals customer
          where invoice.Active &&
                invoice.Status.In(IncludeAlgorithm.ComplexCondition, invoiceStatuses) &&
                invoice.InvoicedOn.HasValue &&
                customer.Id.In(IncludeAlgorithm.TemporaryTable, customers) &&
                payment.Active &&
                payment.Status.In(IncludeAlgorithm.TemporaryTable, paymentStatuses)
          group payment.Amount by invoice.Id
            into amounts
            select new { Id = amounts.Key, Amount = amounts.Sum(e => e) };
        Assert.DoesNotThrow(paymentsQuery.Run);
      }
    }

    [Test]
    public void ComplexConditionInTheMiddle()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var customers = new int[customerIds.Length];
        customerIds.CopyTo(customers, 0);

        var invoiceStatuses = new InvoiceStatus[queryableInvoiceStatuses.Length];
        queryableInvoiceStatuses.CopyTo(invoiceStatuses, 0);

        var paymentStatuses = new PaymentStatus[queryablePaymentStatuses.Length];
        queryablePaymentStatuses.CopyTo(paymentStatuses, 0);

        var paymentsQuery =
          from payment in session.Query.All<Payment>()
          join invoice in session.Query.All<Invoice>() on payment.Invoice equals invoice
          join customer in session.Query.All<Customer>() on invoice.Customer equals customer
          where invoice.Active &&
                invoice.Status.In(IncludeAlgorithm.TemporaryTable, invoiceStatuses) &&
                invoice.InvoicedOn.HasValue &&
                customer.Id.In(IncludeAlgorithm.ComplexCondition, customers) &&
                payment.Active &&
                payment.Status.In(IncludeAlgorithm.TemporaryTable, paymentStatuses)
          group payment.Amount by invoice.Id
            into amounts
            select new { Id = amounts.Key, Amount = amounts.Sum(e => e) };
        Assert.DoesNotThrow(paymentsQuery.Run);
      }
    }

    [Test]
    public void ComplexConditionInTheEnd()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var customers = new int[customerIds.Length];
        customerIds.CopyTo(customers, 0);

        var invoiceStatuses = new InvoiceStatus[queryableInvoiceStatuses.Length];
        queryableInvoiceStatuses.CopyTo(invoiceStatuses, 0);

        var paymentStatuses = new PaymentStatus[queryablePaymentStatuses.Length];
        queryablePaymentStatuses.CopyTo(paymentStatuses, 0);

        var paymentsQuery =
          from payment in session.Query.All<Payment>()
          join invoice in session.Query.All<Invoice>() on payment.Invoice equals invoice
          join customer in session.Query.All<Customer>() on invoice.Customer equals customer
          where invoice.Active &&
                invoice.Status.In(IncludeAlgorithm.TemporaryTable, invoiceStatuses) &&
                invoice.InvoicedOn.HasValue &&
                customer.Id.In(IncludeAlgorithm.TemporaryTable, customers) &&
                payment.Active &&
                payment.Status.In(IncludeAlgorithm.ComplexCondition, paymentStatuses)
          group payment.Amount by invoice.Id
            into amounts
            select new { Id = amounts.Key, Amount = amounts.Sum(e => e) };
        Assert.DoesNotThrow(paymentsQuery.Run);
      }
    }

    [Test]
    public void StoreThenIncludeTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var businessUnitIds = session.Query.All<BusinessUnit>().Select(bu=>bu.Id).ToList();

        var bounds = new Tuple<DateTime, DateTime, string>[26];
        for (int i = 0; i < bounds.Length; i++) {
          bounds[i] = new Tuple<DateTime, DateTime, string>(DateTime.UtcNow, DateTime.UtcNow, "");
        }
        var list = session.Query.All<Job>()
          .Select(job => new {BusinessUnitId = job.BusinessUnit.Id, Month = bounds.FirstOrDefault().Item3})
          .Where(job => job.BusinessUnitId.In(businessUnitIds))
          .ToList();
      }
    }

    [Test]
    public void IncludeThenStoreTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var businessUnitIds = session.Query.All<BusinessUnit>().Select(bu => bu.Id).ToList();

        var bounds = new Tuple<DateTime, DateTime, string>[26];
        for (int i = 0; i < bounds.Length; i++) {
          bounds[i] = new Tuple<DateTime, DateTime, string>(DateTime.UtcNow, DateTime.UtcNow, "");
        }
        var list = session.Query.All<Job>()
          .Where(job => job.BusinessUnit.Id.In(businessUnitIds))
          .Select(job => new { BusinessUnitId = job.BusinessUnit.Id, Month = bounds.FirstOrDefault().Item3 })
        .ToList();
      }
    }

    [Test]
    public void IncludeWithoutStoreTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var businessUnitIds = session.Query.All<BusinessUnit>().Select(bu => bu.Id).ToList();

        var bounds = new Tuple<DateTime, DateTime, string>[26];
        for (int i = 0; i < bounds.Length; i++) {
          bounds[i] = new Tuple<DateTime, DateTime, string>(DateTime.UtcNow, DateTime.UtcNow, "");
        }
        var list = session.Query.All<Job>()
          .Where(job => job.BusinessUnit.Id.In(businessUnitIds))
          .ToList();
      }
    }

    [Test]
    public void StoreWithoutIncludeTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var businessUnitIds = session.Query.All<BusinessUnit>().Select(bu => bu.Id).ToList();

        var bounds = new Tuple<DateTime, DateTime, string>[26];
        for (int i = 0; i < bounds.Length; i++) {
          bounds[i] = new Tuple<DateTime, DateTime, string>(DateTime.UtcNow, DateTime.UtcNow, "");
        }
        var list = session.Query.All<Job>()
          .Select(job => new { BusinessUnitId = job.BusinessUnit.Id, Month = bounds.FirstOrDefault().Item3 })
          .ToList();
      }
    }

    protected override void PopulateData()
    {
      PopulateEnums();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        PopulateCustomers();
        PopulateInvoices();
        PopulatePayments();
        PopulateBusinessUnits();
        PopulateJobs();
        transaction.Complete();
      }
    }

    private void PopulateEnums()
    {
      queryableInvoiceStatuses = new[] {
        InvoiceStatus.Authorized,
        InvoiceStatus.Processing,
        InvoiceStatus.Paid,
        InvoiceStatus.Cancelled,
        InvoiceStatus.Refunded,
      };

      queryablePaymentStatuses = new[] {
        PaymentStatus.Scheduled,
        PaymentStatus.Paid,
        PaymentStatus.Retuned,
        PaymentStatus.Canceled
      };
    }

    private void PopulateCustomers()
    {
      customerIds = new int[5];
      for (int i = 0; i < 5; i++)
        customerIds[i] = new Customer().Id;
    }

    private void PopulateInvoices()
    {
      foreach (var customer in Query.All<Customer>()) {
        foreach (var status in Enum.GetValues(typeof(InvoiceStatus))) {
          new Invoice {
            Active = true,
            Customer = customer,
            InvoicedOn = DateTime.Now,
            Status = (InvoiceStatus)status,
          };
        }
      }
    }

    private void PopulatePayments()
    {
      var generator = new Random();
      foreach (var invoice in Query.All<Invoice>().Where(el => el.Status.In(queryableInvoiceStatuses))) {
        new Payment {
          Active = true,
          Amount = new decimal(generator.Next(10000000, 200000000) / 100000),
          Invoice = invoice,
          Status = GetStatus(invoice.Status)
        };
      }
    }

    private void PopulateBusinessUnits()
    {
      new BusinessUnit() {
        Active = true,
        QuickbooksClass = ""
      };

      new BusinessUnit() {
        Active = true,
        QuickbooksClass = ""
      };

      new BusinessUnit() {
        Active = true,
        QuickbooksClass = ""
      };
    }

    private void PopulateJobs()
    {
      foreach (var bu in Query.All<BusinessUnit>()) {
        new Job() {
          BusinessUnit = bu
        };
      }
    }

    private PaymentStatus GetStatus(InvoiceStatus invoiceStatus)
    {
      switch (invoiceStatus) {
        case InvoiceStatus.Authorized:
          return PaymentStatus.Scheduled;
        case InvoiceStatus.Paid:
          return PaymentStatus.Paid;
        case InvoiceStatus.Processing:
          return PaymentStatus.Scheduled;
        case InvoiceStatus.Refunded:
          return PaymentStatus.Retuned;
        case InvoiceStatus.Cancelled:
          return PaymentStatus.Canceled;
        default:
          throw new NotSupportedException();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (Payment).Assembly, typeof (Payment).Namespace);
      return configuration;
    }
  }
}
