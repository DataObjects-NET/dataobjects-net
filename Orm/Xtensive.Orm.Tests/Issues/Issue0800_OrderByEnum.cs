using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues_Issue0800_OrderByEnum;

namespace Xtensive.Orm.Tests.Issues_Issue0800_OrderByEnum
{
  public enum Status
  {
    Married,
    Single
  }

  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int? IntField { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0800_OrderByEnum: AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var person1 = new Person(){IntField = 1};
          var person2 = new Person(){IntField = 2};
          var personNull = new Person(){IntField = null};
          var query = from person in session.Query.All<Person>()
                      select new {Status = person.IntField == null ? Status.Married : Status.Single};
          var result = query.ToList();
          Assert.AreEqual(3, result.Count);
          // Rollback
        }
      }
    }
  }
}
