// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueGitHub0402_UnusedINCausesWrongMappingsModel;

namespace Xtensive.Orm.Tests.Issues.IssueGitHub0402_UnusedINCausesWrongMappingsModel
{
  [HierarchyRoot]
  public class Invoice : Entity
  {
    [Field, Key]
    public long InvoiceId { get; private set; }

    [Field]
    public long PseudoId { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public DateTime CreatedOn { get; private set; }

    [Field]
    public decimal Subtotal { get; set; }


    public Invoice(Session session)
      : base(session)
    {
      CreatedOn = DateTime.UtcNow.Date.AddDays(-100 + InvoiceId);
      PseudoId = InvoiceId;
    }

    public Invoice(Session session, string name)
      : base(session)
    {
      CreatedOn = DateTime.UtcNow.Date.AddDays(-100 + InvoiceId);
      PseudoId = InvoiceId;
      Name = name;

    }
  }

  public static class FakeExtensions
  {
    public static bool InSome<T>(this T source, params T[] values) =>
      InSome(source, (IEnumerable<T>) values);

    public static bool InSome<T>(this T source, IEnumerable<T> values) =>
      values == null ? false : values.Contains(source);

    public static bool InSome<T>(this T source, IncludeAlgorithm algorithm, params T[] values) =>
      InSome(source, algorithm, (IEnumerable<T>) values);

#pragma warning disable IDE0060 // Remove unused parameter
    public static bool InSome<T>(this T source, IncludeAlgorithm algorithm, IEnumerable<T> values) =>
      values == null ? false : values.Contains(source);
#pragma warning restore IDE0060 // Remove unused parameter

  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueGitHub0402_UnusedINCausesWrongMappings : AutoBuildTest
  {
    private readonly long[] nonExistingIds = new long[3];
    private readonly long[] existingIds = new long[4];

    private readonly string[] nonExistingNames = new string[3];
    private readonly string[] existingNames = new string[4];

    private readonly DateTime[] nonExistingDates = new DateTime[3];
    private readonly DateTime[] existingDates = new DateTime[4];

    protected override DomainConfiguration BuildConfiguration()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.Types.RegisterCaching(typeof(Invoice).Assembly, typeof(Invoice).Namespace);
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;

      return domainConfiguration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var i1 = new Invoice(session, "A");
        var i2 = new Invoice(session, "B");
        var i3 = new Invoice(session, "C");
        var i4 = new Invoice(session, "D");

        nonExistingIds[0] = -(existingIds[0] = i1.InvoiceId);
        nonExistingIds[1] = -(existingIds[1] = i2.InvoiceId);
        nonExistingIds[2] = -(existingIds[2] = i3.InvoiceId);
        existingIds[3] = i4.InvoiceId;

        existingNames[0] = i1.Name;
        existingNames[1] = i2.Name;
        existingNames[2] = i3.Name;
        existingNames[3] = i4.Name;

        nonExistingNames[0] = "E";
        nonExistingNames[1] = "F";
        nonExistingNames[2] = "G";

        existingDates[0] = i1.CreatedOn;
        existingDates[1] = i2.CreatedOn;
        existingDates[2] = i3.CreatedOn;
        existingDates[3] = i4.CreatedOn;

        nonExistingDates[0] = DateTime.UtcNow.AddDays(1);
        nonExistingDates[1] = DateTime.UtcNow.AddDays(2);
        nonExistingDates[2] = DateTime.UtcNow.AddDays(3);

        tx.Complete();
      }
    }

    [Test]
    public void OriginalLinqQueryCaseTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result = (from invoice in session.Query.All<Invoice>()
            let inNonExisting = invoice.InvoiceId.In(nonExistingIds) // This line incorrectly works as additional filter.
                                                                     // Must not have any impact
            where invoice.InvoiceId.In(existingIds)
            select invoice)
          .Count();

        Assert.That(result, Is.EqualTo(existingIds.Length));
      }
    }

    [Test]
    public void MethodsVersionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(nonExistingIds)
          })
          .Where(a => a.invoice.InvoiceId.In(existingIds))
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(nonExistingIds)
          })
          .Where(a => a.invoice.InvoiceId.In(existingIds))
          .Select(filtered => new { filtered.invoice, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingIds.Length));

        Assert.That(result2.Length, Is.EqualTo(existingIds.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.invoice.InvoiceId.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void ExplicitIncludeAlgorithmTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, nonExistingIds)
          })
          .Where(a => a.invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, existingIds))
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, nonExistingIds)
          })
          .Where(a => a.invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, existingIds))
          .Select(filtered => new { filtered.invoice, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingIds.Length));

        Assert.That(result2.Length, Is.EqualTo(existingIds.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.invoice.InvoiceId.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void TwoINsWithinSelectOrder2Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inExisting = invoice.InvoiceId.In(existingIds),
            inNonExisting = invoice.InvoiceId.In(nonExistingIds)
          })
          .Where(a => a.inNonExisting)
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inExisting = invoice.InvoiceId.In(existingIds),
            inNonExisting = invoice.InvoiceId.In(nonExistingIds)
          })
          .Where(a => a.inNonExisting)
          .Select(filtered => new { filtered.invoice, filtered.inExisting, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(0));
        Assert.That(result2.Length, Is.EqualTo(0));
      }
    }

    [Test]
    public void TwoINsWithExplicitAlgorithmWithinSelectOrder2Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inExisting = invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, existingIds),
            inNonExisting = invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, nonExistingIds)
          })
          .Where(a => a.inNonExisting)
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inExisting = invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, existingIds),
            inNonExisting = invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, nonExistingIds)
          })
          .Where(a => a.inNonExisting)
          .Select(filtered => new { filtered.invoice, filtered.inExisting, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(0));
        Assert.That(result2.Length, Is.EqualTo(0));
      }
    }

    [Test]
    public void TwoINsWithinSelectOrder3Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(nonExistingIds),
            inExisting = invoice.InvoiceId.In(existingIds),
          })
          .Where(a => a.inExisting)
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(nonExistingIds),
            inExisting = invoice.InvoiceId.In(existingIds),
          })
          .Where(a => a.inExisting)
          .Select(filtered => new { filtered.invoice, filtered.inExisting, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingIds.Length));

        Assert.That(result2.Length, Is.EqualTo(existingIds.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.inExisting == true), Is.True);
        Assert.That(result2.All(r => r.invoice.InvoiceId.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void TwoINsWithExplicitAlgorithmWithinSelectOrder3Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, nonExistingIds),
            inExisting = invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, existingIds),
          })
          .Where(a => a.inExisting)
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, nonExistingIds),
            inExisting = invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, existingIds),
          })
          .Where(a => a.inExisting)
          .Select(filtered => new { filtered.invoice, filtered.inExisting, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingIds.Length));

        Assert.That(result2.Length, Is.EqualTo(existingIds.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.inExisting == true), Is.True);
        Assert.That(result2.All(r => r.invoice.InvoiceId.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void PseudoIdWithinINTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.PseudoId.In(nonExistingIds)
          })
          .Where(a => a.invoice.InvoiceId.In(existingIds))
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.PseudoId.In(nonExistingIds)
          })
          .Where(a => a.invoice.InvoiceId.In(existingIds))
          .Select(filtered => new { filtered.invoice, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingIds.Length));

        Assert.That(result2.Length, Is.EqualTo(existingIds.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.invoice.InvoiceId.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void PseudoIdWithinINWithExplicitAlgorithmTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.PseudoId.In(IncludeAlgorithm.ComplexCondition, nonExistingIds)
          })
          .Where(a => a.invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, existingIds))
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.PseudoId.In(IncludeAlgorithm.ComplexCondition, nonExistingIds)
          })
          .Where(a => a.invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, existingIds))
          .Select(filtered => new { filtered.invoice, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingIds.Length));

        Assert.That(result2.Length, Is.EqualTo(existingIds.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.invoice.InvoiceId.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void INsByPseudoIdAndSelectedFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice1 = invoice,
            invoice2Id = invoice.InvoiceId,
            inNonExisting = invoice.PseudoId.In(nonExistingIds)
          })
          .Where(a => a.invoice2Id.In(existingIds))
          .Select(filtered => filtered.invoice1)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice1 = invoice,
            invoice2Id = invoice.InvoiceId,
            inNonExisting = invoice.PseudoId.In(nonExistingIds)
          })
          .Where(a => a.invoice2Id.In(existingIds))
          .Select(filtered => new { filtered.invoice1, filtered.invoice2Id, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingIds.Length));

        Assert.That(result2.Length, Is.EqualTo(existingIds.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.invoice1.InvoiceId.In(existingIds)), Is.True);
        Assert.That(result2.All(r => r.invoice2Id.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void INsWithExplicitAlgorithmByPseudoIdAndSelectedFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice1 = invoice,
            invoice2Id = invoice.InvoiceId,
            inNonExisting = invoice.PseudoId.In(IncludeAlgorithm.ComplexCondition, nonExistingIds)
          })
          .Where(a => a.invoice2Id.In(IncludeAlgorithm.ComplexCondition, existingIds))
          .Select(filtered => filtered.invoice1)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice1 = invoice,
            invoice2Id = invoice.InvoiceId,
            inNonExisting = invoice.PseudoId.In(IncludeAlgorithm.ComplexCondition, nonExistingIds)
          })
          .Where(a => a.invoice2Id.In(IncludeAlgorithm.ComplexCondition, existingIds))
          .Select(filtered => new { filtered.invoice1, filtered.invoice2Id, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingIds.Length));

        Assert.That(result2.Length, Is.EqualTo(existingIds.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.invoice1.InvoiceId.In(existingIds)), Is.True);
        Assert.That(result2.All(r => r.invoice2Id.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void INsOfDifferentTypesTest1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.Name.In(IncludeAlgorithm.ComplexCondition, nonExistingNames)
          })
          .Where(a => a.invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, existingIds))
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.Name.In(IncludeAlgorithm.ComplexCondition, nonExistingNames)
          })
          .Where(a => a.invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, existingIds))
          .Select(filtered => new { filtered.invoice, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingIds.Length));

        Assert.That(result2.Length, Is.EqualTo(existingIds.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.invoice.InvoiceId.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void INsOfDifferentTypesWithExplicitAlgorithmTest1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.Name.In(IncludeAlgorithm.ComplexCondition, nonExistingNames)
          })
          .Where(a => a.invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, existingIds))
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.Name.In(IncludeAlgorithm.ComplexCondition, nonExistingNames)
          })
          .Where(a => a.invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, existingIds))
          .Select(filtered => new { filtered.invoice, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingIds.Length));

        Assert.That(result2.Length, Is.EqualTo(existingIds.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.invoice.InvoiceId.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void INsOfDifferentTypesTest2()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.CreatedOn.In(nonExistingDates)
          })
          .Where(a => a.invoice.InvoiceId.In(existingIds))
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.CreatedOn.In(nonExistingDates)
          })
          .Where(a => a.invoice.InvoiceId.In(existingIds))
          .Select(filtered => new { filtered.invoice, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingIds.Length));

        Assert.That(result2.Length, Is.EqualTo(existingIds.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.invoice.InvoiceId.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void INsOfDifferentTypesWithExplicitAlgorithmTest2()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.CreatedOn.In(IncludeAlgorithm.ComplexCondition, nonExistingDates)
          })
          .Where(a => a.invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, existingIds))
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.CreatedOn.In(IncludeAlgorithm.ComplexCondition, nonExistingDates)
          })
          .Where(a => a.invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, existingIds))
          .Select(filtered => new { filtered.invoice, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingIds.Length));

        Assert.That(result2.Length, Is.EqualTo(existingIds.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.invoice.InvoiceId.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void INsOfDifferentTypesTest3()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(nonExistingIds)
          })
          .Where(a => a.invoice.Name.In(existingNames))
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(nonExistingIds)
          })
          .Where(a => a.invoice.Name.In(existingNames))
          .Select(filtered => new { filtered.invoice, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingNames.Length));

        Assert.That(result2.Length, Is.EqualTo(existingNames.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.invoice.InvoiceId.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void INsOfDifferentTypesWithExplicitAlgorithmTest3()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, nonExistingIds)
          })
          .Where(a => a.invoice.Name.In(IncludeAlgorithm.ComplexCondition, existingNames))
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, nonExistingIds)
          })
          .Where(a => a.invoice.Name.In(IncludeAlgorithm.ComplexCondition, existingNames))
          .Select(filtered => new { filtered.invoice, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingNames.Length));

        Assert.That(result2.Length, Is.EqualTo(existingNames.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.invoice.InvoiceId.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void INsOfDifferentTypesTest4()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(nonExistingIds)
          })
          .Where(a => a.invoice.CreatedOn.In(existingDates))
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(nonExistingIds)
          })
          .Where(a => a.invoice.CreatedOn.In(existingDates))
          .Select(filtered => new { filtered.invoice, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingDates.Length));

        Assert.That(result2.Length, Is.EqualTo(existingDates.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.invoice.InvoiceId.In(existingIds)), Is.True);
      }
    }

    [Test]
    public void INsOfDifferentTypesWithExplicitAlgorithmTest4()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var result1 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, nonExistingIds)
          })
          .Where(a => a.invoice.CreatedOn.In(IncludeAlgorithm.ComplexCondition, existingDates))
          .Select(filtered => filtered.invoice)
          .ToArray();

        var result2 = session.Query.All<Invoice>()
          .Select(invoice => new {
            invoice = invoice,
            inNonExisting = invoice.InvoiceId.In(IncludeAlgorithm.ComplexCondition, nonExistingIds)
          })
          .Where(a => a.invoice.CreatedOn.In(IncludeAlgorithm.ComplexCondition, existingDates))
          .Select(filtered => new { filtered.invoice, filtered.inNonExisting })
          .ToArray();

        Assert.That(result1.Length, Is.EqualTo(existingDates.Length));

        Assert.That(result2.Length, Is.EqualTo(existingDates.Length));
        Assert.That(result2.All(r => r.inNonExisting == false), Is.True);
        Assert.That(result2.All(r => r.invoice.InvoiceId.In(existingIds)), Is.True);
      }
    }
  }
}
