using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Building;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.AutoGenericsOverrideTestModel;

namespace Xtensive.Orm.Tests.Model
{
  namespace AutoGenericsOverrideTestModel
  {
    public class Builder : Module
    {
      public override void OnAutoGenericsBuilt(BuildingContext context, ICollection<System.Type> autoGenerics)
      {
        autoGenerics.Remove(typeof (MyAutoGeneric<MyBaseType>));
        autoGenerics.Add(typeof (MyAutoGeneric<MyChildType>));
      }
    }

    [HierarchyRoot]
    public class MyAutoGeneric<T> : Entity
      where T : IEntity
    {
      [Key, Field]
      public long Id { get; private set; }
    }

    [HierarchyRoot]
    public class MyBaseType : Entity
    {
      [Key, Field]
      public long Id { get; private set; }
    }

    public class MyChildType : MyBaseType
    {
    }
  }

  [TestFixture]
  public class AutoGenericsOverrideTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Builder).Assembly, typeof (Builder).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      var baseTypeGeneric = typeof (MyAutoGeneric<MyBaseType>);
      var childTypeGeneric = typeof (MyAutoGeneric<MyChildType>);

      Assert.That(!Domain.Model.Types.Contains(baseTypeGeneric));
      Assert.That(Domain.Model.Types.Contains(childTypeGeneric));
    }
  }
}