using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.DisableValidationAndImplicitConstraintsTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  namespace DisableValidationAndImplicitConstraintsTestModel
  {
    [HierarchyRoot]
    public class MyEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Nullable = false)]
      public string Name { get; set; }
    }
  }

  public class DisableValidationAndImplicitConstraintsTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (MyEntity));
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new MyEntity {Name = "Hello"};
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = Query.All<MyEntity>().Single();
        entity.Name = null;
        entity.Name = "Bye";
        tx.Complete();
      }
    }
  }
}