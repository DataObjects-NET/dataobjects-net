// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.13

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class WhereTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.ObjectModel.NorthwindDO");
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Xtensive.Storage.Domain result = base.BuildDomain(configuration);
      return result;
    }

    [TestFixtureSetUp]
    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          for (int i = 0; i < 100; i++) {
            var supplier = new Supplier();
            supplier.Address = new Address();
            supplier.Address.City = "City" + i;
            supplier.Address.Country = "Country" + i;
            supplier.Address.PostalCode = "Code" + i;
            supplier.Address.StreetAddress = "Address" + i;
            supplier.Address.Region = "Region" + i;
            supplier.CompanyName = "Company" + i;
            supplier.ContactName = "Contact" + i;
            supplier.ContactTitle = "Title" + i;
            supplier.Phone = "Phone" + i;
            supplier.Fax = "Fax" + i;
            supplier.HomePage = "www.homepage.com" + i;
          }
          t.Complete();
        }
      }
    }

    [Test]
    public void ColumnTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var suppliers = Session.Current.All<Supplier>();
          var supplier = suppliers.Where(s => s.CompanyName=="Country20").First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Country20", supplier.CompanyName);
          t.Complete();
        }
      }
    }

    [Test]
    public void StructureTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var suppliers = Session.Current.All<Supplier>();
          var supplier = suppliers.Where(s => s.Address.Region == "Region30").First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Region30", supplier.Address.Region);
          t.Complete();
        }
      }
    }
  }
}