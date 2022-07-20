// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.04.05

using System;

namespace Xtensive.Orm.Tests.Storage.SchemaSharing.EntityManipulation.Model
{
  namespace Part1
  {
    [HierarchyRoot]
    public class TestEntity1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }

#if DO_DATEONLY
      [Field]
      public DateOnly DateOnly { get; set; }

      [Field]
      public TimeOnly TimeOnly { get; set; }
#endif

      [Field]
      public string DatabaseName { get; set; }

      [Field]
      public string SchemaName { get; set; }

      public TestEntity1(Session session)
        : base(session)
      {
      }
    }
  }

  namespace Part2
  {
    [HierarchyRoot]
    public class TestEntity2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }

      [Field]
      public string DatabaseName { get; set; }

      [Field]
      public string SchemaName { get; set; }

      public TestEntity2(Session session)
        : base(session)
      {
      }
    }
  }

  namespace Part3
  {
    [HierarchyRoot]
    public class TestEntity3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }

      [Field]
      public string DatabaseName { get; set; }

      [Field]
      public string SchemaName { get; set; }

      public TestEntity3(Session session)
        : base(session)
      {
      }
    }
  }

  namespace Part4
  {
    [HierarchyRoot]
    public class TestEntity4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }

      [Field]
      public string DatabaseName { get; set; }

      [Field]
      public string SchemaName { get; set; }

      public TestEntity4(Session session)
        : base(session)
      {
      }
    }
  }

  public class TestEntityDTO
  {
    public int Id { get; set; }

    public int TypeId { get; set; }

    public string Text { get; set; }

    public string DatabaseName { get; set; }

    public string SchemaName { get; set; }
  }
}
