// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Orm.Tests.Storage.DataLossOnEventsPrevention.EntityChangeDuringPersistTestModel
{
  public abstract class TestBoundEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string TestIdentifier { get; private set; }

    public TestBoundEntity(Session session, string testIdentifier)
      : base(session)
    {
      TestIdentifier = testIdentifier;
    }
  }

  [HierarchyRoot]
  public class SimpleEntity : TestBoundEntity
  {
    [Field]
    public int Value { get; set; }

    public SimpleEntity(Session session, string testIdentifier)
      : base(session, testIdentifier)
    {
    }
  }

  [HierarchyRoot]
  public class NonPairedEntitySetContainer : TestBoundEntity
  {
    [Field]
    public EntitySet<RefEntity> Refs { get; private set; }

    public NonPairedEntitySetContainer(Session session, string testIdentifier)
      : base(session, testIdentifier)
    {
    }
  }

  [HierarchyRoot]
  public class RefEntity : TestBoundEntity
  {
    [Field]
    public int Value { get; set; }

    public RefEntity(Session session, string testIdentifier)
      : base(session, testIdentifier)
    {
    }
  }

  [HierarchyRoot]
  public class PairedEntitySetContainer : TestBoundEntity
  {
    [Field]
    [Association(PairTo = nameof(PairedRefEntity.Container))]
    public EntitySet<PairedRefEntity> Refs { get; private set; }

    public PairedEntitySetContainer(Session session, string testIdentifier)
      : base(session, testIdentifier)
    {
    }
  }

  [HierarchyRoot]
  public class PairedRefEntity : TestBoundEntity
  {
    [Field]
    public PairedEntitySetContainer Container { get; set; }

    [Field]
    public int Value { get; set; }

    public PairedRefEntity(Session session, string testIdentifier)
      : base(session, testIdentifier)
    {
    }
  }

  [HierarchyRoot]
  public class NonPairedReferencingEntity : TestBoundEntity
  {
    [Field]
    public NonPairedReferencedEntity Ref { get; set; }

    public NonPairedReferencingEntity(Session session, string testIdentifier)
      : base(session, testIdentifier)
    {
    }
  }

  [HierarchyRoot]
  public class NonPairedReferencedEntity : TestBoundEntity
  {
    [Field]
    public int Value { get; set; }

    public NonPairedReferencedEntity(Session session, string testIdentifier)
      : base(session, testIdentifier)
    {
    }
  }
}
