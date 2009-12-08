// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.29

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Issues.Issue0276_DuplicateIndex_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0276_DuplicateIndex_Model
{
  [HierarchyRoot]
  [Index("Id", Unique = true, Name = "IX_Id")]
  [Index("Left", Unique = true, Name = "IX_L")]
  [Index("Right", Unique = true, Name = "IX_R")]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public MyEntity Left { get; set; }

    [Field]
    public Key Right { get; set; }
  }

  [KeyGenerator(null)]
  [HierarchyRoot]
  [Index("Target", Unique = true)]
  public class MyEntityInfo<TEntity> : Entity where TEntity : Entity
  {
    [Field, Key]
    public TEntity Target { get; private set; }

    public MyEntityInfo (TEntity target)
      : base(target)
    {
    }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0276_DuplicateIndex : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      var type = typeof (MyEntity).GetTypeInfo(Domain);
      Assert.AreEqual(6, type.Indexes.Count);

      type = typeof (MyEntityInfo<MyEntity>).GetTypeInfo(Domain);
      Assert.AreEqual(2, type.Indexes.Count);
    }
  }
}