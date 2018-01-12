using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xtensive.Orm.Tests.Storage.SchemaSharing.KeyGenerator.Model
{
  namespace Part1
  {
    [HierarchyRoot]
    public class TestEntity1 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity1(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }

    [HierarchyRoot]
    public class TestEntity2 : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity2(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }
  }

  namespace Part2
  {
    [HierarchyRoot]
    public class TestEntity3 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity3(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }

    [HierarchyRoot]
    public class TestEntity4 : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity4(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }
  }

  namespace Part3
  {
    [HierarchyRoot]
    public class TestEntity5 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity5(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }

    [HierarchyRoot]
    public class TestEntity6 : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity6(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }
  }

  namespace Part4
  {
    [HierarchyRoot]
    public class TestEntity7 : Entity
    {
      [Field, Key]
      public int Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity7(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }

    [HierarchyRoot]
    public class TestEntity8 : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      public string NodeId { get; private set; }

      public TestEntity8(Session session)
        : base(session)
      {
        NodeId = session.StorageNodeId;
      }
    }
  }
}
