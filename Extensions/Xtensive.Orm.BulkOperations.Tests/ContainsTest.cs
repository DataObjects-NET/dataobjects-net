// Copyright (C) 2020-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using NUnit.Framework;
using Xtensive.Orm.BulkOperations.ContainsTestModel;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.BulkOperations.ContainsTestModel
{
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class TagType : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public int ProjectedValueAdjustment { get; set; }

    public TagType(Session session, long id)
      :base(session, id)
    {
    }
  }
}

namespace Xtensive.Orm.BulkOperations.Tests
{
  public class ContainsTest : BulkOperationBaseTest
  {
    private long[] tagIds;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(TagType).Assembly, typeof(TagType).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      tagIds = Enumerable.Range(0, 100).Select(i => (long) i).ToArray();
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        foreach (var id in tagIds.Concat(Enumerable.Repeat(1000, 1).Select(i => (long) i))) {
          _ = new TagType(session, id) { ProjectedValueAdjustment = -1 };
        }

        transaction.Complete();
      }
    }

    [Test]
    public void Test1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var updatedRows = session.Query.All<TagType>()
          .Where(t => t.Id.In(tagIds))
          .Set(t => t.ProjectedValueAdjustment, 2)
          .Update();
        Assert.That(updatedRows, Is.EqualTo(100));
        Assert.That(session.Query.All<TagType>().Count(t => t.ProjectedValueAdjustment == 2 && t.Id <= 200), Is.EqualTo(100));
        Assert.That(session.Query.All<TagType>().Count(t => t.ProjectedValueAdjustment == -1 && t.Id > 700), Is.EqualTo(1));
      }
    }

    [Test]
    public void Test2()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var updatedRows = session.Query.All<TagType>()
          .Where(t => t.Id.In(IncludeAlgorithm.ComplexCondition, tagIds))
          .Set(t => t.ProjectedValueAdjustment, 2)
          .Update();
        Assert.That(updatedRows, Is.EqualTo(100));
        Assert.That(session.Query.All<TagType>().Count(t => t.ProjectedValueAdjustment == 2 && t.Id <= 200), Is.EqualTo(100));
        Assert.That(session.Query.All<TagType>().Count(t => t.ProjectedValueAdjustment == -1 && t.Id > 700), Is.EqualTo(1));
      }
    }

    [Test]
    public void Test3()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = Assert.Throws<NotSupportedException>(() => session.Query.All<TagType>()
          .Where(t => t.Id.In(IncludeAlgorithm.TemporaryTable, tagIds))
          .Set(t => t.ProjectedValueAdjustment, 2)
          .Update());
      }
    }

    [Test]
    public void Test4()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var updatedRows = session.Query.All<TagType>()
          .Where(t => tagIds.Contains(t.Id))
          .Set(t => t.ProjectedValueAdjustment, 2)
          .Update();
        Assert.That(updatedRows, Is.EqualTo(100));
        Assert.That(session.Query.All<TagType>().Count(t => t.ProjectedValueAdjustment == 2 && t.Id <= 200), Is.EqualTo(100));
        Assert.That(session.Query.All<TagType>().Count(t => t.ProjectedValueAdjustment == -1 && t.Id > 700), Is.EqualTo(1));
      }
    }
  }
}
