using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0493_BadImageFormatTest_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0493_BadImageFormatTest_Model
{
  [Serializable]
  public abstract class Info<T> : Entity where T:Entity
  {
    [Field, Key]
    public T Target { get; private set; }

    public Info(T target)
      : base(target)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class PersonInfo : Info<Person>
  {
    public PersonInfo(Person target)
      : base(target)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0493_BadImageFormatTest : AutoBuildTest
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
          var p = new Person();
          var pi = new PersonInfo(p);
          t.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Assert.IsNotNull(session.Query.All<PersonInfo>().First());
        }
      }
    }
  }
}