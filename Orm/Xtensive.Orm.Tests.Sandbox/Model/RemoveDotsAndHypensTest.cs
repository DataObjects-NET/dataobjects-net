using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.RemoveDotsAndHypensTestModel;

namespace Xtensive.Orm.Tests.Model
{
  namespace RemoveDotsAndHypensTestModel
  {
    [HierarchyRoot]
    public class DotObject : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public DotObject Parent { get; set; }

      [Field]
      public EntitySet<DotObject> Siblings { get; set; }

      [Field]
      public ExtraInfo Info { get; set; }

      [Field]
      public string Value { get; set; }
    }

    public class ExtraInfo : Structure
    {
      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    public class MetaInfo<T> : Entity
      where T : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Info { get; set; }
    }
  }

  public class RemoveDotsAndHypensTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (DotObject).Assembly, typeof (DotObject).Namespace);
      configuration.NamingConvention.NamingRules = NamingRules.RemoveHyphens | NamingRules.RemoveDots;
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      foreach (var type in Domain.Model.Types) {
        Assert.That(type.MappingName.Contains("_"), Is.Not.True);
        Assert.That(type.MappingName.Contains("-"), Is.Not.True);

        foreach (var column in type.Columns) {
          Assert.That(column.Name.Contains("_"), Is.Not.True);
          Assert.That(column.Name.Contains("-"), Is.Not.True);
        }
      }
    }
  }
}