using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.TypeIdAsParameterModel;

namespace Xtensive.Orm.Tests.Storage.TypeIdAsParameterModel
{
  [HierarchyRoot]
  public class RegularType : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 20)]
    public string Name { get; set; }

    public RegularType(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot(IncludeTypeId = true)]
  public class TypeIdIncludedType : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 20)]
    public string Name { get; set; }
    
    public TypeIdIncludedType(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
  public class Root : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 20)]
    public string Name { get; set; }
  }

  public class Child : Root
  {
    [Field]
    public string ExtendedName { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class TypeIdAsParameter : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(RegularType));
      config.Types.Register(typeof(TypeIdIncludedType));
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    [Test]
    public void MainTest()
    {
      using(var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var item = "abc";
        //var noFiltersQuery = session.Query.All<RegularType>().Where(t => t.Name == item).ToArray();
        var constantInFilter = session.Query.Execute("Key", q => q.All<RegularType>().Where(t => t.Name == item)).ToArray();
        item = "def";
        constantInFilter = session.Query.Execute("Key", q => q.All<RegularType>().Where(t => t.Name == item)).ToArray();
      }
    }

    [Test]
    public void IncludedIdTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        //var noFiltersQuery = session.Query.All<TestEntity>().ToArray();
        var item = "abc";
        var constantInFilter = session.Query.Execute(q => q.All<TypeIdIncludedType>().Where(t => t.Name == item)).ToArray();
      }
    }
  }
}
