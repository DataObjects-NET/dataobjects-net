// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2015.12.31

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0627_PocoClassPropertyRenitializationModel;

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0627_PocoClassPropertyRenitialization : AutoBuildTest
  {
    private int businessUnitCount;

    [Test]
    public void InitializationOnlyUsingConstructorParametersTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var buClasses = (
                    from businessUnit in session.Query.All<BusinessUnit>()
                    where businessUnit.Active && !string.IsNullOrEmpty(businessUnit.QuickbooksClass)
                    select new QboClassModel(businessUnit.Id, businessUnit.QuickbooksClass)).ToList();
        foreach (var qboClassModel in buClasses) {
          Assert.That(qboClassModel.Name.StartsWith(" ") || qboClassModel.Name.EndsWith(" "), Is.False);
          Assert.That(qboClassModel.SomeOtherField, Is.Null);
        }
      }
    }

    [Test]
    public void AnonymousTypeInitializationTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var buClasses = (from businessUnit in session.Query.All<BusinessUnit>()
          where businessUnit.Active && !string.IsNullOrEmpty(businessUnit.QuickbooksClass)
          select new {businessUnit.Id, Name = businessUnit.QuickbooksClass}).ToList();

        foreach (var qboClassModel in buClasses)
          Assert.That(qboClassModel.Name.StartsWith(" ") || qboClassModel.Name.EndsWith(" "), Is.True);
      }
    }

    [Test]
    public void MixedInitialization()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var buClasses = (from businessUnit in session.Query.All<BusinessUnit>()
          where businessUnit.Active && !string.IsNullOrEmpty(businessUnit.QuickbooksClass)
          select new QboClassModel(businessUnit.Id, businessUnit.QuickbooksClass) {SomeOtherField = businessUnit.QuickbooksClass}).ToList();
        foreach (var qboClassModel in buClasses) {
          Assert.That(qboClassModel.Name.StartsWith(" ") || qboClassModel.Name.EndsWith(" "), Is.False);
          Assert.That(qboClassModel.SomeOtherField.StartsWith(" ") || qboClassModel.SomeOtherField.EndsWith(" "), Is.True);
        }
      }
    }

    [Test]
    public void SimpleClassConstructorInitialization01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new Poco0(el.QuickbooksClass, el.QuickbooksClass)).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new Poco0(el.QuickbooksClass, el.QuickbooksClass)).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));
        foreach (var poco in query)
          Assert.That(expected.Any(el=>el.Key==poco.Key && el.Value==poco.Value), Is.True);
      }
    }

    [Test]
    public void SimpleClassConstructorInitialization02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>().Select(el => new Poco0()).ToList();
        var expected = session.Query.All<BusinessUnit>().AsEnumerable().Select(el => new Poco0()).ToList();
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));
        foreach (var poco in expected) {
          Assert.That(poco.Key, Is.EqualTo(default(string)));
          Assert.That(poco.Value, Is.EqualTo(default(string)));
        }
      }
    }

    [Test]
    public void SimpleClassConstructorInitialization03()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new Poco0(el.QuickbooksClass, el.QuickbooksClass))
          .Select(el=>new {Key = el.Key, Value = el.Value}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new Poco0(el.QuickbooksClass, el.QuickbooksClass))
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));
        foreach (var poco in query)
          Assert.That(expected.Any(el => el.Key==poco.Key && el.Value==poco.Value), Is.True);
      }
    }

    [Test]
    public void SimpleClassConstructorInitialization04()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new Poco0())
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable().Select(el => new Poco0())
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(expected.Count));
      }
    }

    [Test]
    public void SimpleClassConstructorInitialization05()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new Poco1(el.QuickbooksClass, el.QuickbooksClass)).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new Poco1(el.QuickbooksClass, el.QuickbooksClass)).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(expected.Count));
        foreach (var poco in query)
          Assert.That(expected.Any(el => el.Key==poco.Key && el.Value==poco.Value), Is.True);
      }
    }

    [Test]
    public void SimpleClassConstructorInitialization06()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>().Select(el => new Poco1()).ToList();
        var expected = session.Query.All<BusinessUnit>().AsEnumerable().Select(el => new Poco1()).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(expected.Count));
      }
    }

    [Test]
    public void SimpleClassConstructorInitialization07()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new Poco1(el.QuickbooksClass, el.QuickbooksClass))
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new Poco1(el.QuickbooksClass, el.QuickbooksClass))
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));
        foreach (var poco in query)
          Assert.That(expected.Any(el => el.Key==poco.Key && el.Value==poco.Value), Is.True);
      }
    }

    [Test]
    public void SimpleClassConstructorInitialization08()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new Poco1())
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable().Select(el => new Poco1())
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));
      }
    }

    [Test]
    public void SimpleClassObjectInitialization01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new Poco1 {Key = el.QuickbooksClass, Value = el.QuickbooksClass}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new Poco1 { Key = el.QuickbooksClass, Value = el.QuickbooksClass }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));
        foreach (var poco in query)
          Assert.That(expected.Any(el => el.Key==poco.Key && el.Value==poco.Value), Is.True);
      }
    }

    [Test]
    public void SimpleClassObjectInitialization02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new Poco1 { Key = el.QuickbooksClass }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new Poco1 { Key = el.QuickbooksClass }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));
        foreach (var poco in query) {
          var expectedPoco = expected.FirstOrDefault(el => el.Key==poco.Key);
          Assert.That(expectedPoco, Is.Not.Null);
          Assert.That(expectedPoco.Value, Is.EqualTo(default (string)));
          Assert.That(poco.Value, Is.EqualTo(default (string)));
        }
      }
    }

    [Test]
    public void SimpleClassObjectInitialization03()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new Poco1 {Key = el.QuickbooksClass, Value = el.QuickbooksClass})
          .Select(el => new {Key = el.Key, Value = el.Value}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new Poco1 {Key = el.QuickbooksClass, Value = el.QuickbooksClass})
          .Select(el => new {Key = el.Key, Value = el.Value}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));
        foreach (var poco in query)
          Assert.That(expected.Any(el => el.Key==poco.Key && el.Value==poco.Value), Is.True);
      }
    }

    [Test]
    public void SimpleClassObjectInitialization04()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new Poco1 {Key = el.QuickbooksClass})
          .Select(el => new {Key = el.Key, Value = el.Value}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new Poco1 {Key = el.QuickbooksClass})
          .Select(el => new {Key = el.Key, Value = el.Value}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var poco in query) {
          var expectedPoco = expected.FirstOrDefault(el => el.Key == poco.Key);
          Assert.That(expectedPoco, Is.Not.Null);
          Assert.That(expectedPoco.Value, Is.EqualTo(default(string)));
          Assert.That(poco.Value, Is.EqualTo(default(string)));
        }
      }
    }

    [Test]
    public void GenericClassConstructorInitialization01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco0<string, string>(el.QuickbooksClass, el.QuickbooksClass)).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco0<string, string>(el.QuickbooksClass, el.QuickbooksClass)).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco0 in query)
          Assert.That(expected.Any(el=>el.Key==genericPoco0.Key && el.Value==genericPoco0.Value));
      }
    }

    [Test]
    public void GenericClassConstructorInitialization02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco0<long, long>(el.Id, el.Id)).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco0<long, long>(el.Id, el.Id)).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco0 in query)
          Assert.That(expected.Any(el => el.Key==genericPoco0.Key && el.Value==genericPoco0.Value));
      }
    }

    [Test]
    public void GenericClassConstructorInitialization03()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco0<string, string>()).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco0<string, string>()).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in expected) {
          Assert.That(genericPoco.Key, Is.EqualTo(default(string)));
          Assert.That(genericPoco.Value, Is.EqualTo(default(string)));
        }

        foreach (var genericPoco in query) {
          Assert.That(genericPoco.Key, Is.EqualTo(default(string)));
          Assert.That(genericPoco.Value, Is.EqualTo(default(string)));
        }
      }
    }

    [Test]
    public void GenericClassConstructorInitialization04()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco0<long, long>()).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco0<long, long>()).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in expected) {
          Assert.That(genericPoco.Key, Is.EqualTo(default(long)));
          Assert.That(genericPoco.Value, Is.EqualTo(default(long)));
        }

        foreach (var genericPoco in query) {
          Assert.That(genericPoco.Key, Is.EqualTo(default(long)));
          Assert.That(genericPoco.Value, Is.EqualTo(default(long)));
        }
      }
    }

    [Test]
    public void GenericClassConstructorInitialization05()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco0<string, string>(el.QuickbooksClass, el.QuickbooksClass))
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco0<string, string>(el.QuickbooksClass, el.QuickbooksClass))
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco0 in query)
          Assert.That(expected.Any(el => el.Key==genericPoco0.Key && el.Value==genericPoco0.Value));
      }
    }

    [Test]
    public void GenericClassConstructorInitialization06()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco0<long, long>(el.Id, el.Id))
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco0<long, long>(el.Id, el.Id))
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query)
          Assert.That(expected.Any(el => el.Key==genericPoco.Key && el.Value==genericPoco.Value));
      }
    }

    [Test]
    public void GenericClassConstructorInitialization07()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco0<string, string>())
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco0<string, string>())
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in expected) {
          Assert.That(genericPoco.Key, Is.EqualTo(default(string)));
          Assert.That(genericPoco.Value, Is.EqualTo(default(string)));
        }

        foreach (var genericPoco in query) {
          Assert.That(genericPoco.Key, Is.EqualTo(default(string)));
          Assert.That(genericPoco.Value, Is.EqualTo(default(string)));
        }
      }
    }

    [Test]
    public void GenericClassConstructorInitialization08()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco0<long, long>())
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco0<long, long>())
          .Select(el => new {Key = el.Key, Value = el.Value}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in expected) {
          Assert.That(genericPoco.Key, Is.EqualTo(default(long)));
          Assert.That(genericPoco.Value, Is.EqualTo(default(long)));
        }

        foreach (var genericPoco in query) {
          Assert.That(genericPoco.Key, Is.EqualTo(default(long)));
          Assert.That(genericPoco.Value, Is.EqualTo(default(long)));
        }
      }
    }

    [Test]
    public void GenericClassObjectInitialization01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco1<string, string> {Key = el.QuickbooksClass, Value = el.QuickbooksClass}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<string, string> {Key = el.QuickbooksClass, Value = el.QuickbooksClass}).ToList();

        Assert.That(query.Count, Is.EqualTo(5));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query)
          Assert.That(expected.Any(el => el.Key==genericPoco.Key && el.Value==genericPoco.Value));
      }
    }

    [Test]
    public void GenericClassObjectInitialization02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco1<string, string> {Key = el.QuickbooksClass}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<string, string> {Key = el.QuickbooksClass}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query) {
          Assert.That(expected.Any(el => el.Key==genericPoco.Key && el.Value==genericPoco.Value));
          Assert.That(genericPoco.Value==default(string));
        }
      }
    }

    [Test]
    public void GenericClassObjectInitialization03()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco1<string, string> {Key = el.QuickbooksClass, Value = el.QuickbooksClass})
          .Select(el => new {Key = el.Key, Value = el.Value}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<string, string> {Key = el.QuickbooksClass, Value = el.QuickbooksClass})
          .Select(el => new {Key = el.Key, Value = el.Value}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query)
          Assert.That(expected.Any(el => el.Key==genericPoco.Key && el.Value==genericPoco.Value));
      }
    }

    [Test]
    public void GenericClassObjectInitialization04()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco1<string, string> {Key = el.QuickbooksClass})
          .Select(el => new {Key = el.Key, Value = el.Value}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<string, string> {Key = el.QuickbooksClass})
          .Select(el => new {Key = el.Key, Value = el.Value}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query) {
          Assert.That(expected.Any(el => el.Key==genericPoco.Key && el.Value==genericPoco.Value));
          Assert.That(genericPoco.Value==default(string));
        }
      }
    }

    [Test]
    public void GenericClassObjectInitialization05()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco1<long, long> {Key = el.Id, Value = el.Id}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<long, long> {Key = el.Id, Value = el.Id}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query)
          Assert.That(expected.Any(el => el.Key==genericPoco.Key && el.Value==genericPoco.Value));
      }
    }

    [Test]
    public void GenericClassObjectInitialization06()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco1<long, long> {Key = el.Id}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<long, long> {Key = el.Id}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query) {
          Assert.That(expected.Any(el => el.Key == genericPoco.Key && el.Value == genericPoco.Value));
          Assert.That(genericPoco.Value == default(long));
        }
      }
    }

    [Test]
    public void GenericClassObjectInitialization07()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco1<long, long> {Key = el.Id, Value = el.Id})
          .Select(el => new {Key = el.Key, Value = el.Value}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<long, long> {Key = el.Id, Value = el.Id})
          .Select(el => new {Key = el.Key, Value = el.Value}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query)
          Assert.That(expected.Any(el => el.Key==genericPoco.Key && el.Value==genericPoco.Value));
      }
    }

    [Test]
    public void GenericClassObjectInitialization08()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco1<long, long> {Key = el.Id})
          .Select(el => new {Key = el.Key, Value = el.Value}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<long, long> {Key = el.Id})
          .Select(el => new {Key = el.Key, Value = el.Value}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query) {
          Assert.That(expected.Any(el => el.Key==genericPoco.Key && el.Value==genericPoco.Value));
          Assert.That(genericPoco.Value == default(long));
        }
      }
    }

    [Test]
    public void DescendantClassConstructorInitializationTest01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new DescendantPoco0(el.QuickbooksClass)).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco0(el.QuickbooksClass)).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var descendantPoco in expected) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.True);
        }

        foreach (var descendantPoco in query) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.True);
        }
      }
    }

    [Test]
    public void DescendantClassConstructorInitializationTest02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new DescendantPoco0(el.QuickbooksClass, el.QuickbooksClass)).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco0(el.QuickbooksClass, el.QuickbooksClass)).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var descendantPoco in expected) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }

        foreach (var descendantPoco in query) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }
      }
    }

    [Test]
    public void DescendantClassConstructorInitializationTest03()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new DescendantPoco0(el.QuickbooksClass, el.QuickbooksClass, el.QuickbooksClass)).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco0(el.QuickbooksClass, el.QuickbooksClass, el.QuickbooksClass)).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var descendantPoco in expected) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }

        foreach (var descendantPoco in query) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }
      }
    }

    [Test]
    public void DescendantClassConstructorInitializationTest04()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new DescendantPoco0(el.QuickbooksClass))
          .Select(el => new {Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco0(el.QuickbooksClass))
          .Select(el => new {Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var descendantPoco in expected) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.True);
        }

        foreach (var descendantPoco in query) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.True);
        }
      }
    }

    [Test]
    public void DescendantClassConstructorInitializationTest05()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new DescendantPoco0(el.QuickbooksClass, el.QuickbooksClass))
          .Select(el => new {Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco0(el.QuickbooksClass, el.QuickbooksClass))
          .Select(el => new {Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var descendantPoco in expected) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }

        foreach (var descendantPoco in query) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }
      }
    }

    [Test]
    public void DescendantClassConstructorInitializationTest06()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new DescendantPoco0(el.QuickbooksClass, el.QuickbooksClass, el.QuickbooksClass))
          .Select(el => new {Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco0(el.QuickbooksClass, el.QuickbooksClass, el.QuickbooksClass))
          .Select(el => new { Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var descendantPoco in expected) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }

        foreach (var descendantPoco in query) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }
      }
    }

    [Test]
    public void DescendantClassObjectInitializationTest01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new DescendantPoco1 {AdditionalInfo = el.QuickbooksClass}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco1 {AdditionalInfo = el.QuickbooksClass}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));

        foreach (var descendantPoco in expected) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.True);
        }

        foreach (var descendantPoco in query) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.True);
        }
      }
    }

    [Test]
    public void DescendantClassObjectInitializationTest02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new DescendantPoco1 {Key = el.QuickbooksClass, Value = el.QuickbooksClass}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco1 {Key = el.QuickbooksClass, Value = el.QuickbooksClass}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));

        foreach (var descendantPoco in expected){
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }

        foreach (var descendantPoco in query) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }
      }
    }

    [Test]
    public void DescendantClassObjectInitializationTest03()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new DescendantPoco1 {AdditionalInfo = el.QuickbooksClass, Key = el.QuickbooksClass, Value = el.QuickbooksClass}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco1 {AdditionalInfo = el.QuickbooksClass, Key = el.QuickbooksClass, Value = el.QuickbooksClass}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));

        foreach (var descendantPoco in expected) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }

        foreach (var descendantPoco in query) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }
      }
    }

    [Test]
    public void DescendantClassObjectInitializationTest04()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new DescendantPoco1 { AdditionalInfo = el.QuickbooksClass })
          .Select(el => new {Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco1 {AdditionalInfo = el.QuickbooksClass})
          .Select(el => new {Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));

        foreach (var descendantPoco in expected) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.True);
        }

        foreach (var descendantPoco in query) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.True);
        }
      }
    }

    [Test]
    public void DescendantClassObjectInitializationTest05()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new DescendantPoco1 {Key = el.QuickbooksClass, Value = el.QuickbooksClass})
          .Select(el => new {Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco1 {Key = el.QuickbooksClass, Value = el.QuickbooksClass})
          .Select(el => new {Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));

        foreach (var descendantPoco in expected) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }

        foreach (var descendantPoco in query) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.True);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }
      }
    }

    [Test]
    public void DescendantClassObjectInitializationTest06()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new DescendantPoco1 {AdditionalInfo = el.QuickbooksClass, Key = el.QuickbooksClass, Value = el.QuickbooksClass})
          .Select(el => new {Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo}).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco1 {AdditionalInfo = el.QuickbooksClass, Key = el.QuickbooksClass, Value = el.QuickbooksClass})
          .Select(el => new {Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo}).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(query.Count, Is.EqualTo(businessUnitCount));

        foreach (var descendantPoco in expected) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }

        foreach (var descendantPoco in query) {
          Assert.That(string.IsNullOrEmpty(descendantPoco.AdditionalInfo), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Key), Is.False);
          Assert.That(string.IsNullOrEmpty(descendantPoco.Value), Is.False);
        }
      }
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new BusinessUnit {Active = true, QuickbooksClass = "jdfhgkjhfdgjkhjhjh     "};
        new BusinessUnit {Active = true, QuickbooksClass = "    jdfhgkgjkhjhjh     "};
        new BusinessUnit {Active = true, QuickbooksClass = " jdfhgkjhjdhfgfdgjkhjhjh"};
        new BusinessUnit {Active = true, QuickbooksClass = "dfhgkaaaaajkhjhjh "};
        new BusinessUnit {Active = true, QuickbooksClass = " jdfhgkjhfdgaaaaajh"};
        businessUnitCount = 5;
        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (BusinessUnit));
      return configuration;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0627_PocoClassPropertyRenitializationModel
{
  [HierarchyRoot]
  public class BusinessUnit : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public bool Active { get; set; }

    [Field]
    public string QuickbooksClass { get; set; }
  }

  public class Poco0
  {
    public string Key { get; private set; }

    public string Value { get; private set; }

    public Poco0()
    {
      Key = default (string);
      Value = default (string);
    }

    public Poco0(string key, string value)
    {
      Key = key;
      Value = value;
    }
  }

  public class Poco1
  {
    public string Key { get; set; }

    public string Value { get; set; }

    public Poco1()
    {
      Key = default (string);
      Value = default (string);
    }

    public Poco1(string key, string value)
    {
      Key = key;
      Value = value;
    }
  }

  public class GenericPoco0<TKey, TValue>
  {
    public TKey Key { get; private set; }

    public TValue Value { get; private set; }

    public GenericPoco0()
    {
      Key = default (TKey);
      Value = default (TValue);
    }

    public GenericPoco0(TKey key, TValue value)
    {
      Key = key;
      Value = value;
    }
  }

  public class GenericPoco1<TKey, TValue>
  {
    public TKey Key { get; set; }

    public TValue Value { get; set; }

    public GenericPoco1()
    {
      Key = default (TKey);
      Value = default (TValue);
    }

    public GenericPoco1(TKey key, TValue value)
    {
      Key = key;
      Value = value;
    }
  }

  public class BasePoco0
  {
    public string Key { get; private set; }

    public string Value { get; private set; }

    public BasePoco0()
    {
      Key = default(string);
      Value = default(string);
    }

    public BasePoco0(string key, string value)
    {
      Key = key;
      Value = value;
    }
  }

  public sealed class DescendantPoco0 : BasePoco0
  {
    public string AdditionalInfo { get; private set; }

    public DescendantPoco0()
    {
      AdditionalInfo = string.Empty;
    }

    public DescendantPoco0(string additionalInfo)
    {
      AdditionalInfo = additionalInfo;
    }

    public DescendantPoco0(string key, string value)
      : base(key, value)
    {
      AdditionalInfo = string.Empty;
    }

    public DescendantPoco0(string key, string value, string additionalInfo)
      : base(key, value)
    {
      AdditionalInfo = additionalInfo;
    }
  }

  public class BasePoco1
  {
    public string Key { get; set; }

    public string Value { get; set; }

    public BasePoco1()
    {
      Key = default(string);
      Value = default(string);
    }

    public BasePoco1(string key, string value)
    {
      Key = key;
      Value = value;
    }
  }

  public sealed class DescendantPoco1 : BasePoco1
  {
    public string AdditionalInfo { get; set; }

    public DescendantPoco1()
    {
      AdditionalInfo = default(string);
    }

    public DescendantPoco1(string additionalInfo)
    {
      AdditionalInfo = additionalInfo;
    }

    public DescendantPoco1(string key, string value)
      : base(key, value)
    {
      AdditionalInfo = default(string);
    }

    public DescendantPoco1(string key, string value, string additionalInfo)
      : base(key, value)
    {
      AdditionalInfo = additionalInfo;
    }
  }

  public class QboClassModel
  {
    public long Id { get; private set; }

    public string Name { get; private set; }

    public string SomeOtherField { get; internal set; }

    public QboClassModel(long initialId, string initialName)
    {
      Id = initialId;
      Name = initialName.Trim();
    }
  }
}
