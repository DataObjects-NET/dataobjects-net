using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues_Issue0847_IsOperator;

namespace Xtensive.Orm.Tests.Issues_Issue0847_IsOperator
{
  public enum Activity
  {
    Blogging,
    Reading,
    Emailing,
    Buzzing,
    Chatting,
    Unknown
  }

  [HierarchyRoot]
  public class Developer : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  public interface IAmSoloProfessional : IEntity 
  {
    [Field]
    Activity CurrentActivity { get; set; }
  }

  public sealed class Alex : Developer, IAmSoloProfessional
  {
    public Activity CurrentActivity { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0847_IsOperator : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Alex).Assembly, typeof (Alex).Namespace);
      return config;
    }

    [Test]
    public void IsTest()
    {
      using (var session = Session.Open(Domain)) {
        using (var transactionn = Transaction.Open()) {
          var alex = new Alex();
          session.SaveChanges();
          var query = Query.All<Developer>().Select(developer => new {IsSoloProfessional = developer is IAmSoloProfessional}).ToArray();
          Assert.AreEqual(1, query.Length);
          Assert.AreEqual(true, query[0].IsSoloProfessional);
        }
      }
    }

    [Test]
    public void AsTest()
    {
      using (var session = Session.Open(Domain)) {
        using (var transactionn = Transaction.Open()) {
          var alex = new Alex();
          session.SaveChanges();
          var query = Query.All<Developer>().Select(developer => new {SoloProfessional = developer as IAmSoloProfessional}).ToArray();
          Assert.AreEqual(1, query.Length);
          Assert.AreSame(alex, query[0].SoloProfessional);
        }
      }
    }
  }
}