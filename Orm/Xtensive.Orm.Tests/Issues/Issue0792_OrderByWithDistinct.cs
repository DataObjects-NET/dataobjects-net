using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;

using Xtensive.Orm.Tests.Issues_Issue0792_OrderByWithDistinct;

namespace Xtensive.Orm.Tests.Issues_Issue0792_OrderByWithDistinct
{
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  class Issue0792_OrderByWithDistinct: AutoBuildTest
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
          var person3 = new Person {Name = "Person3"};
          var person2 = new Person {Name = "Person2"};
          var person1 = new Person {Name = "Person1"};
          session.SaveChanges();
          var query = session.Query.All<Person>().OrderBy(p => p.Name).Distinct().ToList();
          Assert.IsTrue(query.SequenceEqual(new []{person1, person2, person3}));
          // Rollback
        }
      }
    }
  }
}
