// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.22

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0194_WrongResultWhenInClauseArgumentIsEmptyModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0194_WrongResultWhenInClauseArgumentIsEmptyModel
{
  [HierarchyRoot, KeyGenerator(KeyGeneratorKind.None)]
  public class Person : Entity
  {
    [Field, Key(0)]
    public string Name { get; private set; }

    [Field, Key(1)]
    public string Surname { get; private set; }

    public Person(string name, string surname)
      : base(name, surname)
    {
    }
  }

  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Person Owner { get; set; }

    public Document()
    {
      Name = Id.ToString();
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0194_WrongResultWhenInClauseArgumentIsEmpty : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Document).Assembly, typeof (Document).Namespace);
      return config;
    }

    [Test]
    public void ListOfPrimitiveTypeAsParameterTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var document = new Document();
          var forbiddenIds = new List<int>();
          var result = session.Query.All<Document>().Where(doc => !doc.Id.In(forbiddenIds)).Count();
          Assert.AreEqual(1, result);
          // Rollback
        }
      }
    }

    [Test]
    public void ListOfPrimitiveTypeAsExpressionTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var document = new Document();
          var result = session.Query.All<Document>().Where(doc => !doc.Id.In(new List<int>())).Count();
          Assert.AreEqual(1, result);
          // Rollback
        }
      }
    }

    [Test]
    public void ListOfReferenceTypeAsParameterTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var person = new Person("Vasily", "Petrov");
          var document = new Document {Owner = person};
          var forbiddenOwners = new List<Person>();
          var result = session.Query.All<Document>().Where(doc => !doc.Owner.In(forbiddenOwners)).Count();
          Assert.AreEqual(1, result);
          // Rollback
        }
      }
    }

    [Test]
    public void ListOfReferenceTypeAsExpressionTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var person = new Person("Vasily", "Petrov");
          var document = new Document {Owner = person};
          var result = session.Query.All<Document>().Where(doc => !doc.Owner.In(new List<Person>())).Count();
          Assert.AreEqual(1, result);
          // Rollback
        }
      }
    }
  }
}