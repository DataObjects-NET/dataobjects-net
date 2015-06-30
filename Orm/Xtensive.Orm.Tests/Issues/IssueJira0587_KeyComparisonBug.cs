// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.06.29

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Issues.IssueJira0587_KeyComparisonBugModel;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Issues.IssueJira0587_KeyComparisonBugModel
{
  public interface IPerson : IEntity
  {
    [Field]
    string FirstName { get; set; }

    [Field]
    string LastName { get; set; }
  }

  [HierarchyRoot]
  public class Person : Entity, IPerson
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    public Person(Session session, string firstName, string lastName)
      : base(session)
    {
      FirstName = firstName;
      LastName = lastName;
    }
  }

  [HierarchyRoot]
  public class PassportOffice : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Title { get; set; }

    public PassportOffice(Session session, string titile)
      : base(session)
    {
      Title = titile;
    }
  }

  [HierarchyRoot]
  public class Passport: Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public IPerson Person { get; set; }

    public Passport(Session session, IPerson person)
      : base(session)
    {
      Person = person;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0587_KeyComparisonBug: AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var person = session.Query.All<Person>().First(el => el.Id==1);
        Assert.That(typeof (IPerson).IsAssignableFrom(person.Key.TypeReference.Type.UnderlyingType), Is.True);
        var passportOffice = session.Query.All<PassportOffice>().First(el => el.Id==1);

        var passport = new Passport(session, person);
        var referencedPerson = passport.Person;
        Assert.That(referencedPerson, Is.Not.Null);
        Assert.That(referencedPerson.GetType(), Is.EqualTo(typeof (Person)));
      }
    }

    protected override void PopulateData()
    {
      Catalog catalog;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var connection = ((SqlSessionHandler) session.Handler).Connection;
        var extractionResult = Domain.Handlers.StorageDriver.Extract(connection, new[] { new SqlExtractionTask(connection.UnderlyingConnection.Database) });
        catalog = extractionResult.Catalogs[0];
        transaction.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var connection = ((SqlSessionHandler) session.Handler).Connection;
        var queryBuilder = session.Services.Get<QueryBuilder>();
        var type = Domain.Model.Types[typeof (PassportOffice)];
        var insert1 = SqlDml.Insert(SqlDml.TableRef(catalog.DefaultSchema.Tables[type.MappingName]));
        insert1.Values.Add(insert1.Into.Columns[type.Fields["Id"].MappingName], 1);
        insert1.Values.Add(insert1.Into.Columns[type.Fields["Title"].MappingName], "Department #1");
        var compiledQuery = queryBuilder.CompileQuery(insert1);
        using (var command = connection.CreateCommand(insert1)) {
          command.ExecuteNonQuery();
        }

        type = Domain.Model.Types[typeof (Person)];
        var insert2 = SqlDml.Insert(SqlDml.TableRef(catalog.DefaultSchema.Tables[type.MappingName]));
        insert2.Values.Add(insert2.Into.Columns[type.Fields["Id"].MappingName], 1);
        insert2.Values.Add(insert2.Into.Columns[type.Fields["FirstName"].MappingName], "John");
        insert2.Values.Add(insert2.Into.Columns[type.Fields["LastName"].MappingName], "Smith");
        using (var command = connection.CreateCommand(insert2)) {
          command.ExecuteNonQuery();
        }
        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (IPerson).Assembly, typeof (IPerson).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
