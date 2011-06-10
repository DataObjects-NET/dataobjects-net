// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.11

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0102_NoForeignKeyOnInterfaceField_Model;
using Xtensive.Storage.Providers;
using DomainHandler = Xtensive.Storage.Providers.Sql.DomainHandler;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Issues.Issue0102_NoForeignKeyOnInterfaceField_Model
{
  public interface IHierarchy : IEntity
  {
    [Field]
    MyEntity Parent { get; set; }
  }

  [HierarchyRoot]
  public class MyEntity : Entity, IHierarchy
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 100)]
    public string Text { get; set; }

    [Field]
    public MyEntity Parent { get; set; }

    public MyEntity()
    {}

    public MyEntity(Session session)
      : base(session)
    {}
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0102_NoForeignKeyOnInterfaceField : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      var dh = Domain.Handler as DomainHandler;
      var ti = Domain.Model.Types[typeof (MyEntity)];
      string tableName = Domain.Services.Get<NameBuilder>().ApplyNamingRules(ti.MappingName);
      Assert.IsTrue(dh.Schema.Tables[tableName].TableConstraints.OfType<ForeignKey>().Any());
    }
  }
}