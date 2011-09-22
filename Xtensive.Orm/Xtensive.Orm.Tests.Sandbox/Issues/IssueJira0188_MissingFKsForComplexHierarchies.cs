// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.20

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Sql.Model;
using Xtensive.Orm.Tests.Issues.IssueJira0188_MissingFKsForComplexHierarchiesModel;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Tests.Issues.IssueJira0188_MissingFKsForComplexHierarchiesModel
{
  public interface IDocumentElement : IEntity
  {
    [Field]
    Document Document { get; }
  }

  public interface IDocumentCollection : IEntity
  {
    [Field]
    EntitySet<Document> Documents { get; }
  }

  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class Picture : Entity, IDocumentElement
  {
    [Field, Key]
    public int Id { get; private set; }

    public Document Document { get; set; }
  }

  [HierarchyRoot]
  public class Paragraph : Entity, IDocumentElement
  {
    [Field, Key]
    public int Id { get; private set; }

    public Document Document { get; set; }
  }

  [HierarchyRoot]
  public class Person : Entity, IDocumentCollection
  {
    [Field, Key]
    public int Id { get; private set; }

    public EntitySet<Document> Documents { get; private set; }
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
  public abstract class Animal : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Person Owner { get; set; }
  }

  public class Dog : Animal
  {
  }

  public class Cat : Animal
  {
  }

  public class Tiger : Cat
  {
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0188_MissingFKsForComplexHierarchies : AutoBuildTest
  {
    private Schema schema;

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sql);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Document).Assembly, typeof (Document).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      schema = ((DomainHandler) domain.Handler).Schema;
      return domain;
    }

    [Test]
    public void AnimalsTest()
    {
      Assert.AreEqual(1, GetForeignKeysCount(typeof (Dog)));
      Assert.AreEqual(1, GetForeignKeysCount(typeof (Cat)));
      Assert.AreEqual(1, GetForeignKeysCount(typeof (Tiger)));
    }

    [Test]
    public void DocumentsTest()
    {
      Assert.AreEqual(1, GetForeignKeysCount(typeof (Picture)));
      Assert.AreEqual(1, GetForeignKeysCount(typeof (Paragraph)));
    }

    private int GetForeignKeysCount(TypeInfo typeInfo)
    {
      var tableName = Domain.Handlers.NameBuilder.ApplyNamingRules(typeInfo.MappingName);
      var result = schema.Tables[tableName].TableConstraints.OfType<ForeignKey>().Count();
      return result;
    }

    private int GetForeignKeysCount(Type type)
    {
      return GetForeignKeysCount(Domain.Model.Types[type]);
    }
  }
}