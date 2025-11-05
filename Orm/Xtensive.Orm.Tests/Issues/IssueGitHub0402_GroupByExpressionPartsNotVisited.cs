// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueGitHub0402_GroupByExpressionPartsNotVisitedModel;

namespace Xtensive.Orm.Tests.Issues.IssueGitHub0402_GroupByExpressionPartsNotVisitedModel
{
  public static class GlAccountTypes
  {
    public const string Income = "Income";
    public const string IncomeIncome = "IncomeIncome";
    public const string IncomeOtherIncome = "OtherIncome";

    public const string Loans = "Loans";
    public const string LoansPayable = "LoansPayable";
    public const string LoansDeferredRevenue = "LoansDeferredRevenue";

    public const string Assets = "Assets";
    public const string AssetsCash = "AssetsCash";
    public const string AssetsPrepaidExpenses = "AssetsPrepaidExpenses";
  }

  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public long InvoiceId { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public DateTime CreatedOn { get; private set; }

    [Field, Association(PairTo = nameof(InvoiceItem.Invoice))]
    public EntitySet<InvoiceItem> Items { get; set; }

    [Field]
    public decimal Subtotal { get; set; }

    public void CacheCalculateSubtotal()
    {
      this.Session.SaveChanges();
      Subtotal = Items.Select(i => i.Total).Sum();
    }

    public Invoice(Session session)
      : base(session)
    {
      CreatedOn = DateTime.UtcNow.Date.AddDays(-100 + InvoiceId);
    }

    public Invoice(Session session, string name)
      : base(session)
    {
      CreatedOn = DateTime.UtcNow.Date.AddDays(-100 + InvoiceId);
      Name = name;

    }
  }

  [HierarchyRoot]
  public class InvoiceItem : Entity
  {
    [Field, Key]
    public long InvoiceItemId { get; private set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public GeneralLedgerAccount GeneralLedgerAccount { get; set; }

    [Field]
    public BusinessUnit BusinessUnit { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public decimal Total { get; set; }

    public InvoiceItem(Session session, Invoice invoice)
      : base(session)
    {
      Invoice = invoice;
      Name = invoice.Name + "item #" + InvoiceItemId;
    }
  }

  [HierarchyRoot]
  public class GeneralLedgerAccount : Entity
  {
    [Field, Key]
    public long AccountId { get; private set; }

    [Field]
    public string TypeName { get; set; }

    [Field]
    public string Name { get; set; }

    public GeneralLedgerAccount(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class Job : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public Invoice Invoice { get; set; }

    [Field]
    public BusinessUnit BusinessUnit { get; set; }

    [Field]
    public string Name { get; set; }

    public Job(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class BusinessUnit : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string Name { get; set; }

    public BusinessUnit(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueGitHub0402_GroupByExpressionPartsNotVisited : AutoBuildTest
  {
    private readonly List<long> accessibleBusinessUnits = new List<long>();

    protected override DomainConfiguration BuildConfiguration()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.Types.RegisterCaching(typeof(Invoice).Assembly, typeof(Invoice).Namespace);
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;

      return domainConfiguration;
    }

    protected override void PopulateData()
    {
      var glAccountsContainerType = typeof(GlAccountTypes);
      var allTypes = glAccountsContainerType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
        .Where(fi => fi.IsLiteral && !fi.IsInitOnly);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        PopulateEntities(session, allTypes);

        tx.Complete();
      }
    }

    [Test]
    public void Case01Test()
    {
      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var businessUnitId = 10;
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var groupByResults = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId)
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            filteredB = b,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.anonB.anonInst.job.Id,
            gg => (gg.filteredB.anonB.anonInst.item != null)
              ? gg.itemRevenue
              : gg.filteredB.anonB.anonInst.job.Invoice.Subtotal)
          .ToList();

        var groupByResultsLocal = session.Query.All<Job>().AsEnumerable()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().AsEnumerable()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId)
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            filteredB = b,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.anonB.anonInst.job.Id,
            gg => (gg.filteredB.anonB.anonInst.item != null)
              ? gg.itemRevenue
              : gg.filteredB.anonB.anonInst.job.Invoice.Subtotal)
          .ToList();

        Assert.That(groupByResultsLocal.Count, Is.EqualTo(18));
        Assert.That(groupByResults.Count, Is.EqualTo(groupByResultsLocal.Count));

        groupByResults.Sort((a, b) => a.Key.CompareTo(b.Key));
        groupByResultsLocal.Sort((a, b) => a.Key.CompareTo(b.Key));

        for (int i = 0, count = groupByResults.Count; i < count; i++) {
          var server = groupByResults[i];
          var local = groupByResultsLocal[i];
          Assert.That(server.Key, Is.EqualTo(local.Key));

          var serverGroupContent = server.ToList();
          var localGroupContent = local.ToList();

          Assert.That(serverGroupContent.Count, Is.EqualTo(localGroupContent.Count));
          serverGroupContent.Sort();
          localGroupContent.Sort();

          for (int j = 0, gCount = serverGroupContent.Count; j < gCount; j++) {
            Assert.That(serverGroupContent[j], Is.EqualTo(localGroupContent[j]));
          }
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;
      }
    }

    [Test]
    public void Case02Test()
    {
      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var businessUnitId = 10;
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var groupByResults = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId)
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            filteredB = b.anonB.anonInst,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.job.Id,
            gg => (gg.filteredB.item != null)
              ? gg.itemRevenue
              : gg.filteredB.job.Invoice.Subtotal)
          .ToList();

        var groupByResultsLocal = session.Query.All<Job>().AsEnumerable()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().AsEnumerable()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId)
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            filteredB = b.anonB.anonInst,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.job.Id,
            gg => (gg.filteredB.item != null)
              ? gg.itemRevenue
              : gg.filteredB.job.Invoice.Subtotal)
          .ToList();

        Assert.That(groupByResultsLocal.Count, Is.EqualTo(18));
        Assert.That(groupByResults.Count, Is.EqualTo(groupByResultsLocal.Count));

        groupByResults.Sort((a, b) => a.Key.CompareTo(b.Key));
        groupByResultsLocal.Sort((a, b) => a.Key.CompareTo(b.Key));

        for (int i = 0, count = groupByResults.Count; i < count; i++) {
          var server = groupByResults[i];
          var local = groupByResultsLocal[i];
          Assert.That(server.Key, Is.EqualTo(local.Key));

          var serverGroupContent = server.ToList();
          var localGroupContent = local.ToList();

          Assert.That(serverGroupContent.Count, Is.EqualTo(localGroupContent.Count));
          serverGroupContent.Sort();
          localGroupContent.Sort();

          for (int j = 0, gCount = serverGroupContent.Count; j < gCount; j++) {
            Assert.That(serverGroupContent[j], Is.EqualTo(localGroupContent[j]));
          }
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;
      }
    }

    [Test]
    public void Case03Test()
    {
      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var groupByResults = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            filteredB = b.anonB.anonInst,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.job.Id,
            gg => (gg.filteredB.item != null)
              ? gg.itemRevenue
              : gg.filteredB.job.Invoice.Subtotal)
          .ToList();

        var groupByResultsLocal = session.Query.All<Job>().AsEnumerable()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().AsEnumerable()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            filteredB = b.anonB.anonInst,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.job.Id,
            gg => (gg.filteredB.item != null)
              ? gg.itemRevenue
              : gg.filteredB.job.Invoice.Subtotal)
          .ToList();

        Assert.That(groupByResultsLocal.Count, Is.EqualTo(20));
        Assert.That(groupByResults.Count, Is.EqualTo(groupByResultsLocal.Count));

        groupByResults.Sort((a, b) => a.Key.CompareTo(b.Key));
        groupByResultsLocal.Sort((a, b) => a.Key.CompareTo(b.Key));

        for (int i = 0, count = groupByResults.Count; i < count; i++) {
          var server = groupByResults[i];
          var local = groupByResultsLocal[i];
          Assert.That(server.Key, Is.EqualTo(local.Key));

          var serverGroupContent = server.ToList();
          var localGroupContent = local.ToList();

          Assert.That(serverGroupContent.Count, Is.EqualTo(localGroupContent.Count));
          serverGroupContent.Sort();
          localGroupContent.Sort();

          for (int j = 0, gCount = serverGroupContent.Count; j < gCount; j++) {
            Assert.That(serverGroupContent[j], Is.EqualTo(localGroupContent[j]));
          }
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;
      }
    }

    [Test]
    public void Case04Test()
    {
      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var businessUnitId = 10;
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var groupByResults = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId)
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            job = b.anonB.anonInst.job,
            item = b.anonB.anonInst.item,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.job.Id,
            gg => (gg.item != null)
              ? gg.itemRevenue
              : gg.job.Invoice.Subtotal)
          .ToList();

        var groupByResultsLocal = session.Query.All<Job>().AsEnumerable()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().AsEnumerable()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId)
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            job = b.anonB.anonInst.job,
            item = b.anonB.anonInst.item,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.job.Id,
            gg => (gg.item != null)
              ? gg.itemRevenue
              : gg.job.Invoice.Subtotal)
          .ToList();

        Assert.That(groupByResultsLocal.Count, Is.EqualTo(18));
        Assert.That(groupByResults.Count, Is.EqualTo(groupByResultsLocal.Count));

        groupByResults.Sort((a, b) => a.Key.CompareTo(b.Key));
        groupByResultsLocal.Sort((a, b) => a.Key.CompareTo(b.Key));

        for (int i = 0, count = groupByResults.Count; i < count; i++) {
          var server = groupByResults[i];
          var local = groupByResultsLocal[i];
          Assert.That(server.Key, Is.EqualTo(local.Key));

          var serverGroupContent = server.ToList();
          var localGroupContent = local.ToList();

          Assert.That(serverGroupContent.Count, Is.EqualTo(localGroupContent.Count));
          serverGroupContent.Sort();
          localGroupContent.Sort();

          for (int j = 0, gCount = serverGroupContent.Count; j < gCount; j++) {
            Assert.That(serverGroupContent[j], Is.EqualTo(localGroupContent[j]));
          }
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;
      }
    }

    [Test]
    public void Case05Test()
    {
      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var groupByResults = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            job = b.anonB.anonInst.job,
            item = b.anonB.anonInst.item,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.job.Id,
            gg => (gg.item != null)
              ? gg.itemRevenue
              : gg.job.Invoice.Subtotal)
          .ToList();

        var groupByResultsLocal = session.Query.All<Job>().AsEnumerable()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().AsEnumerable()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            job = b.anonB.anonInst.job,
            item = b.anonB.anonInst.item,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.job.Id,
            gg => (gg.item != null)
              ? gg.itemRevenue
              : gg.job.Invoice.Subtotal)
          .ToList();

        Assert.That(groupByResultsLocal.Count, Is.EqualTo(20));
        Assert.That(groupByResults.Count, Is.EqualTo(groupByResultsLocal.Count));

        groupByResults.Sort((a, b) => a.Key.CompareTo(b.Key));
        groupByResultsLocal.Sort((a, b) => a.Key.CompareTo(b.Key));

        for (int i = 0, count = groupByResults.Count; i < count; i++) {
          var server = groupByResults[i];
          var local = groupByResultsLocal[i];
          Assert.That(server.Key, Is.EqualTo(local.Key));

          var serverGroupContent = server.ToList();
          var localGroupContent = local.ToList();

          Assert.That(serverGroupContent.Count, Is.EqualTo(localGroupContent.Count));
          serverGroupContent.Sort();
          localGroupContent.Sort();

          for (int j = 0, gCount = serverGroupContent.Count; j < gCount; j++) {
            Assert.That(serverGroupContent[j], Is.EqualTo(localGroupContent[j]));
          }
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;
      }
    }

    [Test]
    public void Case06Test()
    {
      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var businessUnitId = 10;
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var groupByResults = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            job = b.anonInst.job,
            item = b.anonInst.item,
            isIncomeItem = b.isIncomeItem,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId)
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            job = b.job,
            item = b.item,
            itemRevenue = b.isIncomeItem
              ? b.item.Total
              : 0
          })
          .GroupBy(g => g.job.Id,
            gg => (gg.item != null)
              ? gg.itemRevenue
              : gg.job.Invoice.Subtotal)
          .ToList();

        var groupByResultsLocal = session.Query.All<Job>().AsEnumerable()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().AsEnumerable()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            job = b.anonInst.job,
            item = b.anonInst.item,
            isIncomeItem = b.isIncomeItem,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId)
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            job = b.job,
            item = b.item,
            itemRevenue = b.isIncomeItem
              ? b.item.Total
              : 0
          })
          .GroupBy(g => g.job.Id,
            gg => (gg.item != null)
              ? gg.itemRevenue
              : gg.job.Invoice.Subtotal)
          .ToList();

        Assert.That(groupByResultsLocal.Count, Is.EqualTo(18));
        Assert.That(groupByResults.Count, Is.EqualTo(groupByResultsLocal.Count));

        groupByResults.Sort((a, b) => a.Key.CompareTo(b.Key));
        groupByResultsLocal.Sort((a, b) => a.Key.CompareTo(b.Key));

        for (int i = 0, count = groupByResults.Count; i < count; i++) {
          var server = groupByResults[i];
          var local = groupByResultsLocal[i];
          Assert.That(server.Key, Is.EqualTo(local.Key));

          var serverGroupContent = server.ToList();
          var localGroupContent = local.ToList();

          Assert.That(serverGroupContent.Count, Is.EqualTo(localGroupContent.Count));
          serverGroupContent.Sort();
          localGroupContent.Sort();

          for (int j = 0, gCount = serverGroupContent.Count; j < gCount; j++) {
            Assert.That(serverGroupContent[j], Is.EqualTo(localGroupContent[j]));
          }
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;
      }
    }

    [Test]
    public void Case07Test()
    {
      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var groupByResults = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            job = b.anonInst.job,
            item = b.anonInst.item,
            isIncomeItem = b.isIncomeItem,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            job = b.job,
            item = b.item,
            itemRevenue = b.isIncomeItem
              ? b.item.Total
              : 0
          })
          .GroupBy(g => g.job.Id,
            gg => (gg.item != null)
              ? gg.itemRevenue
              : gg.job.Invoice.Subtotal)
          .ToList();

        var groupByResultsLocal = session.Query.All<Job>().AsEnumerable()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().AsEnumerable()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            job = b.anonInst.job,
            item = b.anonInst.item,
            isIncomeItem = b.isIncomeItem,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            job = b.job,
            item = b.item,
            itemRevenue = b.isIncomeItem
              ? b.item.Total
              : 0
          })
          .GroupBy(g => g.job.Id,
            gg => (gg.item != null)
              ? gg.itemRevenue
              : gg.job.Invoice.Subtotal)
          .ToList();

        Assert.That(groupByResultsLocal.Count, Is.EqualTo(20));
        Assert.That(groupByResults.Count, Is.EqualTo(groupByResultsLocal.Count));

        groupByResults.Sort((a, b) => a.Key.CompareTo(b.Key));
        groupByResultsLocal.Sort((a, b) => a.Key.CompareTo(b.Key));

        for (int i = 0, count = groupByResults.Count; i < count; i++) {
          var server = groupByResults[i];
          var local = groupByResultsLocal[i];
          Assert.That(server.Key, Is.EqualTo(local.Key));

          var serverGroupContent = server.ToList();
          var localGroupContent = local.ToList();

          Assert.That(serverGroupContent.Count, Is.EqualTo(localGroupContent.Count));
          serverGroupContent.Sort();
          localGroupContent.Sort();

          for (int j = 0, gCount = serverGroupContent.Count; j < gCount; j++) {
            Assert.That(serverGroupContent[j], Is.EqualTo(localGroupContent[j]));
          }
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;
      }
    }

    [Test]
    public void Case08Test()
    {
      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var businessUnitId = 10;
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var groupByResults = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            job = a.job,
            item = a.item,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            job = b.job,
            item = b.item,
            isIncomeItem = b.isIncomeItem,
            itemBusinessUnitId = ((b.item != null) && (b.item.BusinessUnit != null))
              ? b.item.BusinessUnit.Id
              : b.job.BusinessUnit.Id
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId)
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            job = b.job,
            item = b.item,
            itemRevenue = b.isIncomeItem
              ? b.item.Total
              : 0
          })
          .GroupBy(g => g.job.Id,
            gg => (gg.item != null)
              ? gg.itemRevenue
              : gg.job.Invoice.Subtotal)
          .ToList();

        var groupByResultsLocal = session.Query.All<Job>().AsEnumerable()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().AsEnumerable()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            job = a.job,
            item = a.item,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            job = b.job,
            item = b.item,
            isIncomeItem = b.isIncomeItem,
            itemBusinessUnitId = ((b.item != null) && (b.item.BusinessUnit != null))
              ? b.item.BusinessUnit.Id
              : b.job.BusinessUnit.Id
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId)
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            job = b.job,
            item = b.item,
            itemRevenue = b.isIncomeItem
              ? b.item.Total
              : 0
          })
          .GroupBy(g => g.job.Id,
            gg => (gg.item != null)
              ? gg.itemRevenue
              : gg.job.Invoice.Subtotal)
          .ToList();

        Assert.That(groupByResultsLocal.Count, Is.EqualTo(18));
        Assert.That(groupByResults.Count, Is.EqualTo(groupByResultsLocal.Count));

        groupByResults.Sort((a, b) => a.Key.CompareTo(b.Key));
        groupByResultsLocal.Sort((a, b) => a.Key.CompareTo(b.Key));

        for (int i = 0, count = groupByResults.Count; i < count; i++) {
          var server = groupByResults[i];
          var local = groupByResultsLocal[i];
          Assert.That(server.Key, Is.EqualTo(local.Key));

          var serverGroupContent = server.ToList();
          var localGroupContent = local.ToList();

          Assert.That(serverGroupContent.Count, Is.EqualTo(localGroupContent.Count));
          serverGroupContent.Sort();
          localGroupContent.Sort();

          for (int j = 0, gCount = serverGroupContent.Count; j < gCount; j++) {
            Assert.That(serverGroupContent[j], Is.EqualTo(localGroupContent[j]));
          }
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;
      }
    }

    [Test]
    public void Case09Test()
    {
      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var groupByResults = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            job = a.job,
            item = a.item,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            job = b.job,
            item = b.item,
            isIncomeItem = b.isIncomeItem,
            itemBusinessUnitId = ((b.item != null) && (b.item.BusinessUnit != null))
              ? b.item.BusinessUnit.Id
              : b.job.BusinessUnit.Id
          })
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            job = b.job,
            item = b.item,
            itemRevenue = b.isIncomeItem
              ? b.item.Total
              : 0
          })
          .GroupBy(g => g.job.Id,
            gg => (gg.item != null)
              ? gg.itemRevenue
              : gg.job.Invoice.Subtotal)
          .ToList();

        var groupByResultsLocal = session.Query.All<Job>().AsEnumerable()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().AsEnumerable()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            job = a.job,
            item = a.item,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            job = b.job,
            item = b.item,
            isIncomeItem = b.isIncomeItem,
            itemBusinessUnitId = ((b.item != null) && (b.item.BusinessUnit != null))
              ? b.item.BusinessUnit.Id
              : b.job.BusinessUnit.Id
          })
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            job = b.job,
            item = b.item,
            itemRevenue = b.isIncomeItem
              ? b.item.Total
              : 0
          })
          .GroupBy(g => g.job.Id,
            gg => (gg.item != null)
              ? gg.itemRevenue
              : gg.job.Invoice.Subtotal)
          .ToList();

        Assert.That(groupByResultsLocal.Count, Is.EqualTo(20));
        Assert.That(groupByResults.Count, Is.EqualTo(groupByResultsLocal.Count));

        groupByResults.Sort((a, b) => a.Key.CompareTo(b.Key));
        groupByResultsLocal.Sort((a, b) => a.Key.CompareTo(b.Key));

        for (int i = 0, count = groupByResults.Count; i < count; i++) {
          var server = groupByResults[i];
          var local = groupByResultsLocal[i];
          Assert.That(server.Key, Is.EqualTo(local.Key));

          var serverGroupContent = server.ToList();
          var localGroupContent = local.ToList();

          Assert.That(serverGroupContent.Count, Is.EqualTo(localGroupContent.Count));
          serverGroupContent.Sort();
          localGroupContent.Sort();

          for (int j = 0, gCount = serverGroupContent.Count; j < gCount; j++) {
            Assert.That(serverGroupContent[j], Is.EqualTo(localGroupContent[j]));
          }
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;
      }
    }

    [Test]
    public void Case10Test()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.Apply);

      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var businessUnitId = 10;
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var otherRevenues = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId)
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            filteredB = b,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.anonB.anonInst.job.Id,
            gg => (gg.filteredB.anonB.anonInst.item != null)
              ? gg.itemRevenue
              : gg.filteredB.anonB.anonInst.job.Invoice.Subtotal)
          .Select(items => new { JobId = items.Key, Revenue = items.Sum() })
          .ToList();

        var otherRevenuesLocal = session.Query.All<Job>().AsEnumerable()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().AsEnumerable()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId)
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            filteredB = b,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.anonB.anonInst.job.Id,
            gg => (gg.filteredB.anonB.anonInst.item != null)
              ? gg.itemRevenue
              : gg.filteredB.anonB.anonInst.job.Invoice.Subtotal)
          .Select(items => new { JobId = items.Key, Revenue = items.Sum() })
          .ToList();

        Assert.That(otherRevenuesLocal.Count, Is.EqualTo(18));
        Assert.That(otherRevenues.Count, Is.EqualTo(otherRevenuesLocal.Count));

        otherRevenues.Sort((a, b) => a.JobId.CompareTo(b.JobId));
        otherRevenuesLocal.Sort((a, b) => a.JobId.CompareTo(b.JobId));

        for (int i = 0, count = otherRevenues.Count; i < count; i++) {
          var server = otherRevenues[i];
          var local = otherRevenuesLocal[i];
          Assert.That(server.JobId, Is.EqualTo(local.JobId));
          Assert.That(server.Revenue, Is.EqualTo(local.Revenue));
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;

        Assert.That(otherRevenues.Count, Is.GreaterThan(0));
      }
    }

    [Test]
    public void Case11Test()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.Apply);

      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var otherRevenues = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            filteredB = b,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.anonB.anonInst.job.Id,
            gg => (gg.filteredB.anonB.anonInst.item != null)
              ? gg.itemRevenue
              : gg.filteredB.anonB.anonInst.job.Invoice.Subtotal)
          .Select(items => new { JobId = items.Key, Revenue = items.Sum() })
          .ToList();

        var otherRevenuesLocal = session.Query.All<Job>().AsEnumerable()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().AsEnumerable()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            filteredB = b,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.anonB.anonInst.job.Id,
            gg => (gg.filteredB.anonB.anonInst.item != null)
              ? gg.itemRevenue
              : gg.filteredB.anonB.anonInst.job.Invoice.Subtotal)
          .Select(items => new { JobId = items.Key, Revenue = items.Sum() })
          .ToList();

        Assert.That(otherRevenuesLocal.Count, Is.EqualTo(20));
        Assert.That(otherRevenues.Count, Is.EqualTo(otherRevenuesLocal.Count));

        otherRevenues.Sort((a, b) => a.JobId.CompareTo(b.JobId));
        otherRevenuesLocal.Sort((a, b) => a.JobId.CompareTo(b.JobId));

        for (int i = 0, count = otherRevenues.Count; i < count; i++) {
          var server = otherRevenues[i];
          var local = otherRevenuesLocal[i];
          Assert.That(server.JobId, Is.EqualTo(local.JobId));
          Assert.That(server.Revenue, Is.EqualTo(local.Revenue));
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;

        Assert.That(otherRevenues.Count, Is.GreaterThan(0));
      }
    }

    [Test]
    public void Case12Test()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.Apply);

      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var businessUnitId = 10;
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var otherRevenues = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId)
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            filteredB = b.anonB,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.anonInst.job.Id,
            gg => (gg.filteredB.anonInst.item != null)
              ? gg.itemRevenue
              : gg.filteredB.anonInst.job.Invoice.Subtotal)
          .Select(items => new { JobId = items.Key, Revenue = items.Sum() })
          .ToList();

        var otherRevenuesLocal = session.Query.All<Job>().AsEnumerable()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().AsEnumerable()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId)
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            filteredB = b.anonB,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.anonInst.job.Id,
            gg => (gg.filteredB.anonInst.item != null)
              ? gg.itemRevenue
              : gg.filteredB.anonInst.job.Invoice.Subtotal)
          .Select(items => new { JobId = items.Key, Revenue = items.Sum() })
          .ToList();

        Assert.That(otherRevenuesLocal.Count, Is.EqualTo(18));
        Assert.That(otherRevenues.Count, Is.EqualTo(otherRevenuesLocal.Count));

        otherRevenues.Sort((a, b) => a.JobId.CompareTo(b.JobId));
        otherRevenuesLocal.Sort((a, b) => a.JobId.CompareTo(b.JobId));

        for (int i = 0, count = otherRevenues.Count; i < count; i++) {
          var server = otherRevenues[i];
          var local = otherRevenuesLocal[i];
          Assert.That(server.JobId, Is.EqualTo(local.JobId));
          Assert.That(server.Revenue, Is.EqualTo(local.Revenue));
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;

        Assert.That(otherRevenues.Count, Is.GreaterThan(0));
      }
    }

    [Test]
    public void Case13Test()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.Apply);

      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var otherRevenues = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            filteredB = b,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.anonB.anonInst.job.Id,
            gg => (gg.filteredB.anonB.anonInst.item != null)
              ? gg.itemRevenue
              : gg.filteredB.anonB.anonInst.job.Invoice.Subtotal)
          .Select(items => new { JobId = items.Key, Revenue = items.Sum() })
          .ToList();

        var otherRevenuesLocal = session.Query.All<Job>().AsEnumerable()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().AsEnumerable()
                      .Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome })
          })
          .Select(b => new {
            anonB = b,
            itemBusinessUnitId = ((b.anonInst.item != null) && (b.anonInst.item.BusinessUnit != null))
              ? b.anonInst.item.BusinessUnit.Id
              : b.anonInst.job.BusinessUnit.Id
          })
          .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
          .Select(b => new {
            filteredB = b,
            itemRevenue = b.anonB.isIncomeItem
              ? b.anonB.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.anonB.anonInst.job.Id,
            gg => (gg.filteredB.anonB.anonInst.item != null)
              ? gg.itemRevenue
              : gg.filteredB.anonB.anonInst.job.Invoice.Subtotal)
          .Select(items => new { JobId = items.Key, Revenue = items.Sum() })
          .ToList();

        Assert.That(otherRevenuesLocal.Count, Is.EqualTo(20));
        Assert.That(otherRevenues.Count, Is.EqualTo(otherRevenuesLocal.Count));

        otherRevenues.Sort((a, b) => a.JobId.CompareTo(b.JobId));
        otherRevenuesLocal.Sort((a, b) => a.JobId.CompareTo(b.JobId));

        for (int i = 0, count = otherRevenues.Count; i < count; i++) {
          var server = otherRevenues[i];
          var local = otherRevenuesLocal[i];
          Assert.That(server.JobId, Is.EqualTo(local.JobId));
          Assert.That(server.Revenue, Is.EqualTo(local.Revenue));
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;

        Assert.That(otherRevenues.Count, Is.GreaterThan(0));
      }
    }

    [Test]
    public void Case14Test()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.Apply);

      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var businessUnitId = 10;
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var otherRevenues = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            anonInst = a,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
              .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome }),
            itemBusinessUnitId = ((a.item != null) && (a.item.BusinessUnit != null))
              ? a.item.BusinessUnit.Id
              : a.job.BusinessUnit.Id,
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId && (allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId)))
          .Select(b => new {
            filteredB = b,
            itemRevenue = b.isIncomeItem
              ? b.anonInst.item.Total
              : 0
          })
          .GroupBy(g => g.filteredB.anonInst.job.Id,
            gg => (gg.filteredB.anonInst.item != null)
              ? gg.itemRevenue
              : gg.filteredB.anonInst.job.Invoice.Subtotal)
          .Select(items => new { JobId = items.Key, Revenue = items.Sum() })
          .ToList();


        var otherRevenuesLocal = session.Query.All<Job>().AsEnumerable()
         .SelectMany(
           job => session.Query.All<InvoiceItem>().AsEnumerable().Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
           (job, item) => new { job = job, item = item })
         .Select(a => new {
           anonInst = a,
           isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
             .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome }),
           itemBusinessUnitId = ((a.item != null) && (a.item.BusinessUnit != null))
             ? a.item.BusinessUnit.Id
             : a.job.BusinessUnit.Id,
         })
         .Where(b => b.itemBusinessUnitId != businessUnitId)
         .Where(b => allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId))
         .Select(b => new {
           filteredB = b,
           itemRevenue = b.isIncomeItem
             ? b.anonInst.item.Total
             : 0
         })
         .GroupBy(g => g.filteredB.anonInst.job.Id,
           gg => (gg.filteredB.anonInst.item != null)
             ? gg.itemRevenue
             : gg.filteredB.anonInst.job.Invoice.Subtotal)
         .Select(items => new { JobId = items.Key, Revenue = items.Sum() })
         .ToList();

        Assert.That(otherRevenuesLocal.Count, Is.EqualTo(18));
        Assert.That(otherRevenues.Count, Is.EqualTo(otherRevenuesLocal.Count));

        otherRevenues.Sort((a, b) => a.JobId.CompareTo(b.JobId));
        otherRevenuesLocal.Sort((a, b) => a.JobId.CompareTo(b.JobId));

        for (int i = 0, count = otherRevenues.Count; i < count; i++) {
          var server = otherRevenues[i];
          var local = otherRevenuesLocal[i];
          Assert.That(server.JobId, Is.EqualTo(local.JobId));
          Assert.That(server.Revenue, Is.EqualTo(local.Revenue));
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;
      }
    }

    [Test]
    public void Case15Test()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.Apply);

      var accessibleBusinessUnitIds = accessibleBusinessUnits.ToList();
      var businessUnitId = 10;
      var allBusinessUnitsAccessible = !accessibleBusinessUnitIds.Any(id => id != 0);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += Events_DbCommandExecuting;
        session.Events.QueryExecuting += Events_QueryExecuting;

        var otherRevenues = session.Query.All<Job>()
          .SelectMany(
            job => session.Query.All<InvoiceItem>().Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
            (job, item) => new { job = job, item = item })
          .Select(a => new {
            job = a.job,
            item = a.item,
            isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
              .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome }),
            itemBusinessUnitId = ((a.item != null) && (a.item.BusinessUnit != null))
              ? a.item.BusinessUnit.Id
              : a.job.BusinessUnit.Id,
          })
          .Where(b => b.itemBusinessUnitId != businessUnitId && (allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId)))
          .Select(b => new {
            job = b.job,
            item = b.item,
            itemRevenue = b.isIncomeItem
              ? b.item.Total
              : 0
          })
          .GroupBy(g => g.job.Id,
            gg => (gg.item != null)
              ? gg.itemRevenue
              : gg.job.Invoice.Subtotal)
          .Select(items => new { JobId = items.Key, Revenue = items.Sum() })
          .ToList();


        var otherRevenuesLocal = session.Query.All<Job>().AsEnumerable()
         .SelectMany(
           job => session.Query.All<InvoiceItem>().AsEnumerable().Where(item => item.Invoice == job.Invoice).DefaultIfEmpty(),
           (job, item) => new { job = job, item = item })
         .Select(a => new {
           job = a.job,
           item = a.item,
           isIncomeItem = (a.item.GeneralLedgerAccount == null) || a.item.GeneralLedgerAccount.TypeName
              .In(new[] { GlAccountTypes.Income, GlAccountTypes.IncomeIncome, GlAccountTypes.IncomeOtherIncome }),
           itemBusinessUnitId = ((a.item != null) && (a.item.BusinessUnit != null))
              ? a.item.BusinessUnit.Id
              : a.job.BusinessUnit.Id,
         })
          .Where(b => b.itemBusinessUnitId != businessUnitId
             && (allBusinessUnitsAccessible || accessibleBusinessUnitIds.Contains(b.itemBusinessUnitId)))
          .Select(b => new {
            job = b.job,
            item = b.item,
            itemRevenue = b.isIncomeItem
              ? b.item.Total
              : 0
          })
          .GroupBy(g => g.job.Id,
            gg => (gg.item != null)
              ? gg.itemRevenue
              : gg.job.Invoice.Subtotal)
          .Select(items => new { JobId = items.Key, Revenue = items.Sum() })
         .ToList();

        Assert.That(otherRevenuesLocal.Count, Is.EqualTo(18));
        Assert.That(otherRevenues.Count, Is.EqualTo(otherRevenuesLocal.Count));

        otherRevenues.Sort((a, b) => a.JobId.CompareTo(b.JobId));
        otherRevenuesLocal.Sort((a, b) => a.JobId.CompareTo(b.JobId));

        for (int i = 0, count = otherRevenues.Count; i < count; i++) {
          var server = otherRevenues[i];
          var local = otherRevenuesLocal[i];
          Assert.That(server.JobId, Is.EqualTo(local.JobId));
          Assert.That(server.Revenue, Is.EqualTo(local.Revenue));
        }

        session.Events.QueryExecuting -= Events_QueryExecuting;
        session.Events.DbCommandExecuting -= Events_DbCommandExecuting;
      }
    }

    private void PopulateEntities(Session session, IEnumerable<System.Reflection.FieldInfo> allAccountTypes)
    {
      var glAccounts = new List<GeneralLedgerAccount>(15);
      foreach (var constantField in allAccountTypes) {
        var glAccount = new GeneralLedgerAccount(session) { TypeName = (string) constantField.GetValue(null) };
        glAccounts.Add(glAccount);
      }

      while (glAccounts.Count < 15) {
        glAccounts.Add(null);
      }

      var businessUnits = new List<BusinessUnit>(7);

      for (var i = 0; i < 7; i++) {
        var bUnit = new BusinessUnit(session) { Name = $"Unit #{i + 1}" };
        businessUnits.Add(bUnit);
        if (i < 6) {
          accessibleBusinessUnits.Add(bUnit.Id);
        }
      }

      session.SaveChanges();

      var indexForName = 1;

      // invoices entirely for one business unit each
      foreach (var bU in businessUnits) {
        var invoice1 = new Invoice(session) { Subtotal = 0, Name = $"invoice #{indexForName}" };
        var job1 = new Job(session) { Invoice = invoice1, BusinessUnit = bU, Name = $"job #{indexForName}" };

        var invoice2 = new Invoice(session) { Subtotal = 0, Name = $"invoice #{indexForName + 1}" };
        var job2 = new Job(session) { Invoice = invoice1, BusinessUnit = bU, Name = $"job #{indexForName + 1}" };

        //these gets business unit from job
        foreach (var account in glAccounts) {
          var invoiceItem1 = new InvoiceItem(session, invoice1) {
            GeneralLedgerAccount = account,
            BusinessUnit = null,
            Total = ((account != null ? account.AccountId : 0) + invoice1.InvoiceId + job1.Id) / indexForName
          };
          var invoiceItem2 = new InvoiceItem(session, invoice2) {
            GeneralLedgerAccount = account,
            BusinessUnit = null,
            Total = ((account != null ? account.AccountId : 5) + invoice2.InvoiceId + job1.Id) / indexForName
          };
        }
        indexForName += 2;
      }

      session.SaveChanges();

      //invoices with all invoice items of different business units
      var sharedInvoice = new Invoice(session) { Subtotal = 0, Name = $"shared invoice #0" };
      var sharedInvoceJob = new Job(session) { Invoice = sharedInvoice, BusinessUnit = businessUnits[0], Name = $"shared job #0" };

      indexForName = 1;
      foreach (var account in glAccounts.Where(b => b != null)) {
        foreach (var bU in businessUnits) {
          var invoiceItem = new InvoiceItem(session, sharedInvoice) {
            GeneralLedgerAccount = account,
            BusinessUnit = bU,
            Total = (account.AccountId + sharedInvoice.InvoiceId + sharedInvoceJob.Id) / indexForName
          };
          indexForName++;
        }
      }

      session.SaveChanges();

      // invoices with default business unit in job and defined in invoiceItem
      for (var i = 0; i < businessUnits.Count; i++) {
        var bU = businessUnits[i];
        var mixedInvoice = new Invoice(session) { Subtotal = 0, Name = $"mixed invoice #{i + 1}" };
        var mixedInvoceJob = new Job(session) { Invoice = sharedInvoice, BusinessUnit = bU, Name = $"mixed job #{i + 1}" };

        indexForName = 1;
        foreach (var account in glAccounts) {
          foreach (var bbU in businessUnits) {
            var invoiceItem = new InvoiceItem(session, sharedInvoice) {
              GeneralLedgerAccount = account,
              BusinessUnit = bU == bbU ? null : bbU,
              Total = ((account != null ? account.AccountId : 10) + sharedInvoice.InvoiceId + mixedInvoceJob.Id) / indexForName
            };
            indexForName++;
          }
        }
      }
      session.SaveChanges();
    }


    private void Events_QueryExecuting(object sender, QueryEventArgs e)
    {
      Console.WriteLine("The Query executing");
      Console.WriteLine(e.Expression.ToString());
    }

    private void Events_DbCommandExecuting(object sender, DbCommandEventArgs e)
    {
      Console.WriteLine("Command Text:");
      Console.WriteLine(e.Command.CommandText);
      foreach (System.Data.Common.DbParameter param in e.Command.Parameters) {
        Console.WriteLine(param.ParameterName + " = " + param.Value.ToString());
      }
    }
  }
}
