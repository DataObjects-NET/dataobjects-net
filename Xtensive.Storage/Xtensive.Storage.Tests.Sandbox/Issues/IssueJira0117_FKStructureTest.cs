// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.05.19

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Sql.Model;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.MultipleFKViaStructureTestModel;
using DomainHandler = Xtensive.Storage.Providers.Sql.DomainHandler;

namespace Xtensive.Storage.Tests.Storage.MultipleFKViaStructureTestModel
{
  [HierarchyRoot]
  public class Target1 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [HierarchyRoot]
  public class Target2 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  public class TargetSet : Structure
  {
    [Field]
    public Target1 T1 { get; set; }

    [Field]
    public Target2 T2 { get; set; }
  }

  [HierarchyRoot]
  public class Owner0 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Target1 T1 { get; set; }

    [Field]
    public Target2 T2 { get; set; }
  }

  [HierarchyRoot]
  public class Owner1 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public TargetSet Targets { get; set; }
  }

  [HierarchyRoot]
  public class Owner2 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public TargetSet Targets { get; set; }
  }

  [HierarchyRoot]
  public class Owner3 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public TargetSet Targets { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class IssueJira0117_FKStructureTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sql);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Target1).Assembly, typeof (Target1).Namespace);
      return config;
    }

    [Test]
    public void AssociationThroughStructureTest()
    {
      var type = Domain.Model.Types[typeof (Owner1)];
      Assert.AreEqual(2, type.GetOwnerAssociations().Count);
      Assert.AreEqual(8, Domain.Model.Associations.Count);
    }

    [Test]
    public void ForeignKeysCountTest()
    {
      var domainHandler = typeof(Domain)
        .GetProperty("Handler", BindingFlags.Instance | BindingFlags.NonPublic)
        .GetValue(Domain, null) as DomainHandler;

      var schema = domainHandler.Schema;

      var count = GetForeignKeysCount(schema, typeof(Owner1));
      Assert.AreEqual(count, GetForeignKeysCount(schema, typeof(Owner2)));
      Assert.AreEqual(count, GetForeignKeysCount(schema, typeof(Owner3)));
    }

    [Test]
    public void IndexesCountTest()
    {
      var count = GetIndexesCount(typeof (Owner0));
      Assert.AreEqual(count, GetIndexesCount(typeof(Owner1)));
      Assert.AreEqual(count, GetIndexesCount(typeof(Owner2)));
      Assert.AreEqual(count, GetIndexesCount(typeof(Owner3)));
    }

    private int GetIndexesCount(Type type)
    {
      return Domain.Model.Types[type].Indexes.Count;
    }

    private int GetForeignKeysCount(Schema schema, Type type)
    {
      string tableName = Domain.NameBuilder.ApplyNamingRules(Domain.Model.Types[type].MappingName);
      var result = schema.Tables[tableName].TableConstraints.OfType<ForeignKey>().Count();
      return result;
    }
  }
}
