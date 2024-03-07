// Copyright (C) 2014 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2013.09.26

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;
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

    protected override void CheckRequirements() => Require.ProviderIsNot(StorageProvider.Firebird, "No type that would allow to fix it");

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var tx = session.OpenTransaction()) {
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(10) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(100) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(1000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(10000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(100000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(106751) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(120000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(130000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(140000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(150000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(160000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(170000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(180000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(190000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(200000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(300000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(400000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(500000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(600000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(700000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(800000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(900000) };
        _ = new Invoice { CreatedOn = DateTime.MinValue.AddYears(100), InvoicedOn = DateTime.MinValue.AddYears(100).AddDays(1000000) };
        tx.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<Invoice>().Where(invoice => (invoice.CreatedOn - invoice.InvoicedOn) > TimeSpan.FromHours(1)).ToList();
      }
    }
  }
}
