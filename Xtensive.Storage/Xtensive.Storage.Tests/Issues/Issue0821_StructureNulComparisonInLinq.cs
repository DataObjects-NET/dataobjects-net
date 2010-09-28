using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues_Issue0821_StructureNulComparisonInLinq;

namespace Xtensive.Storage.Tests.Issues_Issue0821_StructureNulComparisonInLinq
{
  [HierarchyRoot]
  public class User : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public Address Address { get; set; }
  }

  public class Address : Structure
  {
    [Field]
    public string City { get; set; }

    [Field]
    public string Street { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0821_StructureNulComparisonInLinq : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (User).Assembly, typeof (User).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var address = new Address {City = "Moscow", Street = "Lomonosova"};
          var nullStrucutre = new User();
          var notNullStructure = new User {Address = address};
          session.SaveChanges();
          var nullQuery = Query.All<User>().Where(u => u.Address==null).ToList();
          var notNullQuery = Query.All<User>().Where(u => u.Address!=null).ToList();
          Assert.AreEqual(0, nullQuery.Count);
          Assert.AreEqual(2, notNullQuery.Count);
        }
      }
    }
  }
}
