// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueGithub0281_InnerJoinForNullableEntityReferenceModel;

namespace Xtensive.Orm.Tests.Issues.IssueGithub0281_InnerJoinForNullableEntityReferenceModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Nullable = false)]
    public string Name { get; set; }

    public TestEntity(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class TestEntity2 : Entity
  {
    [Field, Key] 
    public int Id { get; private set; }

    [Field(Nullable = false)] 
    public string Name { get; set; }

    [Field(Nullable = false)]
    public TestEntity Owner { get; set; }

    [Field]
    public TestEntity3 NullableLink { get; set; }

    [Field(Nullable = false)]
    public TestEntity3 Link { get; set; }

    public TestEntity2(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class TestEntity3 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Nullable = false)] 
    public string Name { get; set; }

    public TestEntity3(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueGithub0281_InnerJoinForNullableEntityReference : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var domainConfiguration = base.BuildConfiguration();

      domainConfiguration.Types.Register(typeof(TestEntity));
      domainConfiguration.Types.Register(typeof(TestEntity2));
      domainConfiguration.Types.Register(typeof(TestEntity3));

      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;

      return domainConfiguration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        _ = new TestEntity(session) { Name = "1" };
        _ = new TestEntity(session) { Name = "2" };

        t.Complete();
      }
    }

    [Test]
    public void SimpleFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var result = session.Query.All<TestEntity>()
          .Select(e => new {
            e.Id,
            Name = session.Query.All<TestEntity2>()
              .FirstOrDefault(it => it.Owner == e).Name
            })
            .ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(x => x.Name == null));
      }
    }

    [Test]
    public void NullableReferenceFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var result = session.Query.All<TestEntity>()
          .Select(e => new {
            e.Id,
            NullableLink = session.Query.All<TestEntity2>()
              .FirstOrDefault(it => it.Owner == e).NullableLink
          })
          .ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(x => x.NullableLink == null));
      }
    }

    [Test]
    public void NonNullableReferenceFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var result = session.Query.All<TestEntity>()
          .Select(e => new {
            e.Id,
            Link = session.Query.All<TestEntity2>()
              .FirstOrDefault(it => it.Owner == e).Link
          })
          .ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(x => x.Link == null));
      }
    }
  }
}
