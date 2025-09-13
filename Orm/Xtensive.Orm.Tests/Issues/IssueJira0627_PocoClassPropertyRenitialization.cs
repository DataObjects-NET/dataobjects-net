// Copyright (C) 2015-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2015.12.31

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Tests.Issues.IssueJira0627_PocoClassPropertyRenitializationModel;

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0627_PocoClassPropertyRenitialization : AutoBuildTest
  {
    private int businessUnitCount;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.RegisterCaching(typeof(BusinessUnit).Assembly, typeof(BusinessUnit).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new BusinessUnit { Active = true, QuickbooksClass = "jdfhgkjhfdgjkhjhjh     " };
        _ = new BusinessUnit { Active = true, QuickbooksClass = "    jdfhgkgjkhjhjh     " };
        _ = new BusinessUnit { Active = true, QuickbooksClass = " jdfhgkjhjdhfgfdgjkhjhjh" };
        _ = new BusinessUnit { Active = true, QuickbooksClass = "dfhgkaaaaajkhjhjh " };
        _ = new BusinessUnit { Active = true, QuickbooksClass = " jdfhgkjhfdgaaaaajh" };

        _ = new TestEntity { Name = Guid.NewGuid().ToString() };
        _ = new TestEntity { Name = Guid.NewGuid().ToString() };
        _ = new TestEntity { Name = Guid.NewGuid().ToString() };
        _ = new TestEntity { Name = Guid.NewGuid().ToString() };
        _ = new TestEntity { Name = Guid.NewGuid().ToString() };
        _ = new TestEntity { Name = Guid.NewGuid().ToString() };
        _ = new TestEntity { Name = Guid.NewGuid().ToString() };
        _ = new TestEntity { Name = Guid.NewGuid().ToString() };
        _ = new TestEntity { Name = Guid.NewGuid().ToString() };

        _ = new TestEntity { Name = "klegjkrlksl hdfhgh jhthkjhjth " };
        _ = new TestEntity { Name = " ohgoih oierigh oihreho hoiherigh oherg" };
        _ = new TestEntity { Name = "jshfhjhgjkherjghewogerogp reopertgo   " };
        _ = new TestEntity { Name = "hjwroiheorihi oerhoigho hohergoh " };
        _ = new TestEntity { Name = "wieoru ioritgierh oiheroihg hoidfhgdf" };
        _ = new TestEntity { Name = "joijie oersidfgo dhhri " };
        _ = new TestEntity { Name = "fdg lhwoih jngoj bhoihiwht e" };
        _ = new TestEntity { Name = "rh ihh4i3hi ohierth094t pjpigd" };
        _ = new TestEntity { Name = "i049 hi0 4th 0fgi08 h03gh " };
        businessUnitCount = 5;

        transaction.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var trasnaction = session.OpenTransaction()) {
        var localClasses = new List<QboClassModel>();
        var buClassesLocal = (
          from businessUnit in session.Query.All<BusinessUnit>()
          where businessUnit.Active && !string.IsNullOrEmpty(businessUnit.QuickbooksClass)
          select new {
            businessUnit.Id,
            businessUnit.QuickbooksClass
          }).ToList();

        localClasses.AddRange(
            buClassesLocal.Select(bu => new QboClassModel(bu.Id, bu.QuickbooksClass)));
        Assert.That(localClasses.Count, Is.Not.EqualTo(0));
        foreach (var qboClassModel in localClasses) {
          Assert.That(qboClassModel.Name.StartsWith(" ") || qboClassModel.Name.EndsWith(" "), Is.False);
        }

        localClasses.Clear();

        var buClasses = (
          from businessUnit in session.Query.All<BusinessUnit>()
          where businessUnit.Active && !string.IsNullOrEmpty(businessUnit.QuickbooksClass)
          select new {
            businessUnit.Id,
            businessUnit.QuickbooksClass
          });
        localClasses.AddRange(
            buClasses.Select(bu => new QboClassModel(bu.Id, bu.QuickbooksClass)));
        Assert.That(localClasses.Count, Is.Not.EqualTo(0));
        foreach (var qboClassModel in localClasses) {
          Assert.That(qboClassModel.Name.StartsWith(" ") || qboClassModel.Name.EndsWith(" "), Is.False);
        }
      }
    }

    [Test]
    public void InitializationOnlyUsingConstructorParametersTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var buClasses = (from businessUnit in session.Query.All<BusinessUnit>()
                         where businessUnit.Active && !string.IsNullOrEmpty(businessUnit.QuickbooksClass)
                         select new QboClassModel(businessUnit.Id, businessUnit.QuickbooksClass)).ToList();

        foreach (var qboClassModel in buClasses) {
          Assert.That(qboClassModel.Name.StartsWith(" ") || qboClassModel.Name.EndsWith(" "), Is.False);
          Assert.That(qboClassModel.SomeOtherField, Is.Null);
        }
      }
    }

    [Test]
    public void PocoClassConstructorInsideQuery()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var buClasses = (from dto in
          (
            from businessUnit in session.Query.All<BusinessUnit>()
            where businessUnit.Active && !string.IsNullOrEmpty(businessUnit.QuickbooksClass)
            select new QboClassModel(businessUnit.Id, businessUnit.QuickbooksClass))
                         where dto.Name.StartsWith(" ")
                         select dto.Name).ToList();
      }
    }

    [Test]
    public void PocoClassObjectInitializerInsideQuery()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var buClassess = (from dto in
          (
            from businessUnit in session.Query.All<BusinessUnit>()
            where businessUnit.Active && !string.IsNullOrEmpty(businessUnit.QuickbooksClass)
            select new Poco1() { Key = businessUnit.QuickbooksClass, Value = businessUnit.QuickbooksClass })
                          where dto.Key.StartsWith(" ")
                          select dto.Value).ToList();
      }
    }

    [Test]
    public void AnonymousTypeInitializationTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var buClasses = (from businessUnit in session.Query.All<BusinessUnit>()
                         where businessUnit.Active && !string.IsNullOrEmpty(businessUnit.QuickbooksClass)
                         select new { businessUnit.Id, Name = businessUnit.QuickbooksClass }).ToList();

        foreach (var qboClassModel in buClasses) {
          Assert.That(qboClassModel.Name.StartsWith(" ") || qboClassModel.Name.EndsWith(" "), Is.True);
        }
      }
    }

    [Test]
    public void MixedInitialization()
    {
      Require.ProviderIsNot(StorageProvider.Firebird);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var buClasses = (from businessUnit in session.Query.All<BusinessUnit>()
                         where businessUnit.Active && !string.IsNullOrEmpty(businessUnit.QuickbooksClass)
                         select new QboClassModel(businessUnit.Id, businessUnit.QuickbooksClass) { SomeOtherField = businessUnit.QuickbooksClass }).ToList();
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
        foreach (var poco in query) {
          Assert.That(expected.Any(el => el.Key == poco.Key && el.Value == poco.Value), Is.True);
        }
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
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new Poco0(el.QuickbooksClass, el.QuickbooksClass))
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));
        foreach (var poco in query) {
          Assert.That(expected.Any(el => el.Key == poco.Key && el.Value == poco.Value), Is.True);
        }
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
        foreach (var poco in query) {
          Assert.That(expected.Any(el => el.Key == poco.Key && el.Value == poco.Value), Is.True);
        }
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
        foreach (var poco in query) {
          Assert.That(expected.Any(el => el.Key == poco.Key && el.Value == poco.Value), Is.True);
        }
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
          .Select(el => new Poco1 { Key = el.QuickbooksClass, Value = el.QuickbooksClass }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new Poco1 { Key = el.QuickbooksClass, Value = el.QuickbooksClass }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));
        foreach (var poco in query) {
          Assert.That(expected.Any(el => el.Key == poco.Key && el.Value == poco.Value), Is.True);
        }
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
          var expectedPoco = expected.FirstOrDefault(el => el.Key == poco.Key);
          Assert.That(expectedPoco, Is.Not.Null);
          Assert.That(expectedPoco.Value, Is.EqualTo(default(string)));
          Assert.That(poco.Value, Is.EqualTo(default(string)));
        }
      }
    }

    [Test]
    public void SimpleClassObjectInitialization03()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new Poco1 { Key = el.QuickbooksClass, Value = el.QuickbooksClass })
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new Poco1 { Key = el.QuickbooksClass, Value = el.QuickbooksClass })
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));
        foreach (var poco in query) {
          Assert.That(expected.Any(el => el.Key == poco.Key && el.Value == poco.Value), Is.True);
        }
      }
    }

    [Test]
    public void SimpleClassObjectInitialization04()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new Poco1 { Key = el.QuickbooksClass })
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new Poco1 { Key = el.QuickbooksClass })
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

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

        foreach (var genericPoco0 in query) {
          Assert.That(expected.Any(el => el.Key == genericPoco0.Key && el.Value == genericPoco0.Value));
        }
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

        foreach (var genericPoco0 in query) {
          Assert.That(expected.Any(el => el.Key == genericPoco0.Key && el.Value == genericPoco0.Value));
        }
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

        foreach (var genericPoco0 in query) {
          Assert.That(expected.Any(el => el.Key == genericPoco0.Key && el.Value == genericPoco0.Value));
        }
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

        foreach (var genericPoco in query) {
          Assert.That(expected.Any(el => el.Key == genericPoco.Key && el.Value == genericPoco.Value));
        }
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
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

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
          .Select(el => new GenericPoco1<string, string> { Key = el.QuickbooksClass, Value = el.QuickbooksClass }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<string, string> { Key = el.QuickbooksClass, Value = el.QuickbooksClass }).ToList();

        Assert.That(query.Count, Is.EqualTo(5));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query) {
          Assert.That(expected.Any(el => el.Key == genericPoco.Key && el.Value == genericPoco.Value));
        }
      }
    }

    [Test]
    public void GenericClassObjectInitialization02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco1<string, string> { Key = el.QuickbooksClass }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<string, string> { Key = el.QuickbooksClass }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query) {
          Assert.That(expected.Any(el => el.Key == genericPoco.Key && el.Value == genericPoco.Value));
          Assert.That(genericPoco.Value == default(string));
        }
      }
    }

    [Test]
    public void GenericClassObjectInitialization03()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco1<string, string> { Key = el.QuickbooksClass, Value = el.QuickbooksClass })
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<string, string> { Key = el.QuickbooksClass, Value = el.QuickbooksClass })
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query) {
          Assert.That(expected.Any(el => el.Key == genericPoco.Key && el.Value == genericPoco.Value));
        }
      }
    }

    [Test]
    public void GenericClassObjectInitialization04()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco1<string, string> { Key = el.QuickbooksClass })
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<string, string> { Key = el.QuickbooksClass })
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query) {
          Assert.That(expected.Any(el => el.Key == genericPoco.Key && el.Value == genericPoco.Value));
          Assert.That(genericPoco.Value == default(string));
        }
      }
    }

    [Test]
    public void GenericClassObjectInitialization05()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco1<long, long> { Key = el.Id, Value = el.Id }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<long, long> { Key = el.Id, Value = el.Id }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query) {
          Assert.That(expected.Any(el => el.Key == genericPoco.Key && el.Value == genericPoco.Value));
        }
      }
    }

    [Test]
    public void GenericClassObjectInitialization06()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco1<long, long> { Key = el.Id }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<long, long> { Key = el.Id }).ToList();

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
          .Select(el => new GenericPoco1<long, long> { Key = el.Id, Value = el.Id })
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<long, long> { Key = el.Id, Value = el.Id })
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query) {
          Assert.That(expected.Any(el => el.Key == genericPoco.Key && el.Value == genericPoco.Value));
        }
      }
    }

    [Test]
    public void GenericClassObjectInitialization08()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new GenericPoco1<long, long> { Key = el.Id })
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new GenericPoco1<long, long> { Key = el.Id })
          .Select(el => new { Key = el.Key, Value = el.Value }).ToList();

        Assert.That(query.Count, Is.EqualTo(businessUnitCount));
        Assert.That(expected.Count, Is.EqualTo(businessUnitCount));

        foreach (var genericPoco in query) {
          Assert.That(expected.Any(el => el.Key == genericPoco.Key && el.Value == genericPoco.Value));
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
          .Select(el => new { Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco0(el.QuickbooksClass))
          .Select(el => new { Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo }).ToList();

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
          .Select(el => new { Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco0(el.QuickbooksClass, el.QuickbooksClass))
          .Select(el => new { Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo }).ToList();

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
          .Select(el => new { Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo }).ToList();
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
          .Select(el => new DescendantPoco1 { AdditionalInfo = el.QuickbooksClass }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco1 { AdditionalInfo = el.QuickbooksClass }).ToList();

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
          .Select(el => new DescendantPoco1 { Key = el.QuickbooksClass, Value = el.QuickbooksClass }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco1 { Key = el.QuickbooksClass, Value = el.QuickbooksClass }).ToList();

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
    public void DescendantClassObjectInitializationTest03()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<BusinessUnit>()
          .Select(el => new DescendantPoco1 { AdditionalInfo = el.QuickbooksClass, Key = el.QuickbooksClass, Value = el.QuickbooksClass }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco1 { AdditionalInfo = el.QuickbooksClass, Key = el.QuickbooksClass, Value = el.QuickbooksClass }).ToList();

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
          .Select(el => new { Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco1 { AdditionalInfo = el.QuickbooksClass })
          .Select(el => new { Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo }).ToList();

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
          .Select(el => new DescendantPoco1 { Key = el.QuickbooksClass, Value = el.QuickbooksClass })
          .Select(el => new { Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco1 { Key = el.QuickbooksClass, Value = el.QuickbooksClass })
          .Select(el => new { Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo }).ToList();

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
          .Select(el => new DescendantPoco1 { AdditionalInfo = el.QuickbooksClass, Key = el.QuickbooksClass, Value = el.QuickbooksClass })
          .Select(el => new { Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo }).ToList();
        var expected = session.Query.All<BusinessUnit>()
          .AsEnumerable()
          .Select(el => new DescendantPoco1 { AdditionalInfo = el.QuickbooksClass, Key = el.QuickbooksClass, Value = el.QuickbooksClass })
          .Select(el => new { Key = el.Key, Value = el.Value, AdditionalInfo = el.AdditionalInfo }).ToList();

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
    public void AllByFieldOfPoco01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .All(el => el.Name.IsNullOrEmpty());
      }
    }

    [Test]
    public void AllByFieldOfPoco02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .All(el => el.Name.IsNullOrEmpty());
      }
    }

    [Test]
    public void AllByFieldOfPoco03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .All(el => el.Name.IsNullOrEmpty());
      }
    }

    [Test]
    public void AllByFieldOfPoco04Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = Assert.Throws<QueryTranslationException>(() => session.Query.All<TestEntity>()
          .Select(e => new Poco { BaseName = e.Name })
          .All(el => el.Name.IsNullOrEmpty()));
      }
    }

    [Test]
    public void AllByFieldOfPoco05Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco())
          .All(el => el.Name.IsNullOrEmpty());
      }
    }


    [Test]
    public void AnyByFieldOfPoco01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .Any(el => el.Name.IsNullOrEmpty());
      }
    }

    [Test]
    public void AnyByFieldOfPoco02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Any(el => el.Name.IsNullOrEmpty());
      }
    }

    [Test]
    public void AnyByFieldOfPoco03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Any(el => el.BaseName.IsNullOrEmpty());
      }
    }

    [Test]
    public void AnyByFieldOfPoco04Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { BaseName = e.Name })
          .Any(el => el.BaseName.IsNullOrEmpty());
      }
    }

    [Test]
    public void AnyByFieldOfPoco05Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco())
          .Any(el => el.Name.IsNullOrEmpty());
      }
    }

    [Test]
    public void AverageByFieldOfPoco01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .Average(el => el.Name.Length);
      }
    }

    [Test]
    public void AverageByFieldOfPoco02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Average(el => el.Name.Length);
      }
    }

    [Test]
    public void AverageByFieldOfPoco03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Average(el => el.BaseName.Length);
      }
    }

    [Test]
    public void AverageByFieldOfPoco04Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { BaseName = e.Name })
          .Average(el => el.BaseName.Length);
      }
    }

    [Test]
    public void AverageByFieldOfPoco05Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = Assert.Throws<TargetInvocationException>(() => session.Query.All<TestEntity>()
          .Select(e => new Poco())
          .Average(el => el.Name.Length));
      }
    }

    [Test]
    public void ConcatOfPocos01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .Concat(session.Query.All<TestEntity>().Select(e => new Poco { Name = e.Name }))
          .Run();
      }
    }

    [Test]
    public void ConcatOfPocos02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Concat(session.Query.All<TestEntity>().Select(e => new Poco { Name = e.Name, BaseName = e.Name }))
          .Run();
      }
    }

    [Test]
    public void ConcatOfPocos03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { BaseName = e.Name })
          .Concat(session.Query.All<TestEntity>().Select(e => new Poco { BaseName = e.Name }))
          .Run();
      }
    }

    [Test]
    public void ConcatOfPocos04Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco())
          .Concat(session.Query.All<TestEntity>().Select(e => new Poco()))
          .Run();
      }
    }

    [Test]
    public void CountByFieldOfPoco01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .Count(el => el.Name.IsNullOrEmpty());
      }
    }

    [Test]
    public void CountByFieldOfPoco02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Count(el => el.Name.IsNullOrEmpty());
      }
    }

    [Test]
    public void CountByFieldOfPoco03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Count(el => el.BaseName.IsNullOrEmpty());
      }
    }

    [Test]
    public void CountByFieldOfPoco04Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { BaseName = e.Name })
          .Count(el => el.BaseName.IsNullOrEmpty());
      }
    }

    [Test]
    public void CountByFieldOfPoco05Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco())
          .Count(el => el.Name.IsNullOrEmpty());
      }
    }

    [Test]
    public void DistinctOfPocos01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .Distinct()
          .Run();
      }
    }

    [Test]
    public void ExceptOfPocos01Test()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.MySql);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .Except(session.Query.All<TestEntity>()
            .Select(e => new Poco { Name = e.Name }))
          .Run();
      }
    }

    [Test]
    public void ExceptOfPocos02Test()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.MySql);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Except(session.Query.All<TestEntity>()
            .Select(e => new Poco { Name = e.Name, BaseName = e.Name }))
          .Run();
      }
    }

    [Test]
    public void ExceptOfPocos03Test()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.MySql);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { BaseName = e.Name })
          .Except(session.Query.All<TestEntity>()
            .Select(e => new Poco { BaseName = e.Name }))
          .Run();
      }
    }

    [Test]
    public void ExceptOfPocos05Test()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.MySql);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco())
          .Except(session.Query.All<TestEntity>()
            .Select(e => new Poco()))
          .Run();
      }
    }

    [Test]
    public void GroupByFieldOfPoco01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .GroupBy(e => e.Name)
          .Run();
      }
    }

    [Test]
    public void GroupByFieldOfPoco02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .GroupBy(e => e.Name)
          .Run();
      }
    }

    [Test]
    public void GroupByFieldOfPoco03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .GroupBy(e => e.BaseName)
          .Run();
      }
    }

    [Test]
    public void GroupByFieldOfPoco04Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = Assert.Throws<QueryTranslationException>(() => session.Query.All<TestEntity>()
          .Select(e => new Poco { BaseName = e.Name })
          .GroupBy(e => e.Name)
          .Run());
      }
    }

    [Test]
    public void GroupByFieldOfPoco05Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco())
          .GroupBy(e => e.Name)
          .Run();
      }
    }

    [Test]
    public void GroupJoinOfPoco01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .GroupJoin(
            session.Query.All<TestEntity>().Select(e => new Poco { Name = e.Name }),
            (poco) => poco.Name,
            poco => poco.Name,
            (poco, pocos) => new { Key = poco.Name, Values = pocos })
          .Run();
      }
    }

    [Test]
    public void GroupJoinOfPoco02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .GroupJoin(
            session.Query.All<TestEntity>().Select(e => new Poco { Name = e.Name, BaseName = e.Name }),
            (poco) => poco.Name,
            poco => poco.Name,
            (poco, pocos) => new { Key = poco.Name, Values = pocos })
          .Run();
      }
    }

    [Test]
    public void GroupJoinOfPocos03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .GroupJoin(
            session.Query.All<TestEntity>().Select(e => new Poco { Name = e.Name, BaseName = e.Name }),
            (poco) => poco.Name,
            poco => poco.BaseName,
            (poco, pocos) => new { Key = poco.Name, Values = pocos })
          .Run();
      }
    }

    [Test]
    public void GroupJoinOfPocos04Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .GroupJoin(
            session.Query.All<TestEntity>().Select(e => new Poco { Name = e.Name, BaseName = e.Name }),
            (poco) => poco.BaseName,
            poco => poco.Name,
            (poco, pocos) => new { Key = poco.BaseName, Values = pocos })
          .Run();
      }
    }

    [Test]
    public void GroupJoinOfPocos05Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .GroupJoin(
            session.Query.All<TestEntity>().Select(e => new Poco { Name = e.Name, BaseName = e.Name }),
            (poco) => poco.BaseName,
            poco => poco.BaseName,
            (poco, pocos) => new { Key = poco.BaseName, Values = pocos })
          .Run();
      }
    }

    [Test]
    public void GroupJoinOfPocos06Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { BaseName = e.Name })
          .GroupJoin(
            session.Query.All<TestEntity>().Select(e => new Poco { BaseName = e.Name }),
            (poco) => poco.BaseName,
            poco => poco.BaseName,
            (poco, pocos) => new { Key = poco.BaseName, Values = pocos })
          .Run();
      }
    }

    [Test]
    public void IntersectOfPocos01Test()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.MySql);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .Except(session.Query.All<TestEntity>()
            .Select(e => new Poco { Name = e.Name }))
          .Run();
      }
    }

    [Test]
    public void IntersectOfPocos02Test()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.MySql);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Intersect(session.Query.All<TestEntity>()
            .Select(e => new Poco { Name = e.Name, BaseName = e.Name }))
          .Run();
      }
    }

    [Test]
    public void IntersectOfPocos03Test()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.MySql);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { BaseName = e.Name })
          .Intersect(session.Query.All<TestEntity>()
            .Select(e => new Poco { BaseName = e.Name }))
          .Run();
      }
    }

    [Test]
    public void IntersectOfPocos05Test()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.MySql);
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco())
          .Intersect(session.Query.All<TestEntity>()
            .Select(e => new Poco()))
          .Run();
      }
    }


    [Test]
    public void JoinOfPocos01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .Join(
            session.Query.All<TestEntity>().Select(e => new Poco { Name = e.Name }),
            poco => poco.Name,
            poco => poco.Name,
            (poco, poco1) => new { poco, poco1 })
          .Run();
      }
    }

    [Test]
    public void JoinOfPoco02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Join(
            session.Query.All<TestEntity>().Select(e => new Poco { Name = e.Name, BaseName = e.Name }),
            poco => poco.Name,
            poco => poco.Name,
            (poco, poco1) => new { poco, poco1 })
          .Run();
      }
    }

    [Test]
    public void JoinOfPocos03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Join(
            session.Query.All<TestEntity>().Select(e => new Poco { Name = e.Name, BaseName = e.Name }),
            poco => poco.BaseName,
            poco => poco.Name,
            (poco, poco1) => new { poco, poco1 })
          .Run();
      }
    }

    [Test]
    public void JoinOfPocos04Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Join(
            session.Query.All<TestEntity>().Select(e => new Poco { Name = e.Name, BaseName = e.Name }),
            poco => poco.Name,
            poco => poco.BaseName,
            (poco, poco1) => new { poco, poco1 })
          .Run();
      }
    }

    [Test]
    public void JoinOfPocos05Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Join(
            session.Query.All<TestEntity>().Select(e => new Poco { Name = e.Name, BaseName = e.Name }),
            poco => poco.BaseName,
            poco => poco.BaseName,
            (poco, poco1) => new { poco, poco1 })
          .Run();
      }
    }

    [Test]
    public void JoinOfPocos06Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { BaseName = e.Name })
          .Join(session.Query.All<TestEntity>()
            .Select(e => new Poco { BaseName = e.Name }), poco => poco.BaseName, poco => poco.BaseName, (poco, poco1) => new { poco, poco1 })
          .Run();
      }
    }

    [Test]
    public void MaxByFieldOfPoco01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .Max(el => el.Name.Length);
      }
    }

    [Test]
    public void MaxByFieldOfPoco02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Max(el => el.Name.Length + el.BaseName.Length);
      }
    }

    [Test]
    public void MaxByFieldOfPoco03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { BaseName = e.Name })
          .Max(el => el.BaseName.Length);
      }
    }

    [Test]
    public void MaxByFieldOfPoco04Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = Assert.Throws<TargetInvocationException>(() => session.Query.All<TestEntity>()
          .Select(e => new Poco())
          .Max(el => el.Name.Length));
      }
    }

    [Test]
    public void MinByFieldOfPoco01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .Min(el => el.Name.Length);
      }
    }

    [Test]
    public void MinByFieldOfPoco02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Min(el => el.Name.Length + el.BaseName.Length);
      }
    }

    [Test]
    public void MinByFieldOfPoco03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { BaseName = e.Name })
          .Min(el => el.BaseName.Length);
      }
    }

    [Test]
    public void MinByFieldOfPoco04Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = Assert.Throws<TargetInvocationException>(() => session.Query.All<TestEntity>()
          .Select(e => new Poco())
          .Min(el => el.Name.Length));
      }
    }

    [Test]
    public void OrderByFieldOfPoco01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .OrderBy(e => e.Name)
          .Run();
      }
    }

    [Test]
    public void OrderByFieldOfPoco02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .OrderBy(e => e.Name)
          .Run();
      }
    }

    [Test]
    public void OrderByFieldOfPoco03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .OrderBy(e => e.BaseName)
          .Run();
      }
    }

    [Test]
    public void OrderByFieldOfPoco04Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = Assert.Throws<QueryTranslationException>(() =>
          session.Query.All<TestEntity>()
            .Select(e => new Poco { BaseName = e.Name })
            .OrderBy(e => e.Name)
            .Run());
      }
    }

    [Test]
    public void OrderByFieldOfPoco05Test()
    {
      RequireProviderDeniesOrderByNull();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = Assert.Throws<StorageException>(() =>
          session.Query.All<TestEntity>()
            .Select(e => new Poco())
            .OrderBy(e => e.Name)
            .Run());
      }
    }

    [Test]
    public void OrderByFieldOfPoco06Test()
    {
      RequireProviderAllowsOrderByNull();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(() =>
          session.Query.All<TestEntity>()
            .Select(e => new Poco())
            .OrderBy(e => e.Name)
            .Run());
      }
    }

    [Test]
    public void OrderByDescendingByFieldOfPoco01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .OrderByDescending(e => e.Name)
          .Run();
      }
    }

    [Test]
    public void OrderByDescendingByFieldOfPoco02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .OrderByDescending(e => e.Name)
          .Run();
      }
    }

    [Test]
    public void OrderByDescendingByFieldOfPoco03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .OrderByDescending(e => e.BaseName)
          .Run();
      }
    }

    [Test]
    public void OrderByDescendingByFieldOfPoco04Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = Assert.Throws<QueryTranslationException>(() =>
          session.Query.All<TestEntity>()
            .Select(e => new Poco { BaseName = e.Name })
            .OrderByDescending(e => e.Name)
            .Run());
      }
    }

    [Test]
    public void OrderByDescendingByFieldOfPoco05Test()
    {
      RequireProviderDeniesOrderByNull();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = Assert.Throws<StorageException>(() =>
          session.Query.All<TestEntity>()
            .Select(e => new Poco())
            .OrderByDescending(e => e.Name)
            .Run());
      }
    }

    [Test]
    public void OrderByDescendingByFieldOfPoco06Test()
    {
      RequireProviderAllowsOrderByNull();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(() =>
          session.Query.All<TestEntity>()
            .Select(e => new Poco())
            .OrderByDescending(e => e.Name)
            .Run());
      }
    }

    [Test]
    public void SumByFieldOfPoco01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name }).Sum(el => el.Name.Length);
      }
    }

    [Test]
    public void SumByFieldOfPoco02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Sum(el => el.Name.Length + el.BaseName.Length);
      }
    }

    [Test]
    public void SumByFieldOfPoco03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = session.Query.All<TestEntity>()
          .Select(e => new Poco { BaseName = e.Name }).Sum(el => el.BaseName.Length);
      }
    }

    [Test]
    public void SumByFieldOfPoco04Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = Assert.Throws<TargetInvocationException>(() =>
          session.Query.All<TestEntity>()
            .Select(e => new Poco()).Sum(el => el.Name.Length));
      }
    }

    [Test]
    public void UnionOfPocos01Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name })
          .Union(session.Query.All<TestEntity>()
            .Select(e => new Poco { Name = e.Name }))
          .Run();
      }
    }

    [Test]
    public void UnionOfPocos02Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { Name = e.Name, BaseName = e.Name })
          .Union(session.Query.All<TestEntity>()
            .Select(e => new Poco { Name = e.Name, BaseName = e.Name }))
          .Run();
      }
    }

    [Test]
    public void UnionOfPocos03Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco { BaseName = e.Name })
          .Union(session.Query.All<TestEntity>()
            .Select(e => new Poco { BaseName = e.Name }))
          .Run();
      }
    }

    [Test]
    public void UnionOfPocos05Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.Query.All<TestEntity>()
          .Select(e => new Poco())
          .Union(session.Query.All<TestEntity>()
            .Select(e => new Poco()))
          .Run();
      }
    }

    private void RequireProviderDeniesOrderByNull()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite | StorageProvider.PostgreSql |
        StorageProvider.MySql | StorageProvider.Firebird | StorageProvider.Oracle);
    }

    private void RequireProviderAllowsOrderByNull()
    {
      Require.ProviderIs(StorageProvider.Sqlite | StorageProvider.PostgreSql |
        StorageProvider.MySql | StorageProvider.Firebird | StorageProvider.Oracle);
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

  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public Guid Id { get; set; }

    [Field]
    public string Name { get; set; }
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

  public class Poco : PocoBase
  {
    public string Name { get; set; }
  }

  public class PocoBase
  {
    public string BaseName { get; set; }
  }

  public class QboClassModel
  {
    public long Id { get; private set; }

    public string Name { get; private set; }

    public string SomeOtherField { get; internal set; }

    public QboClassModel(long id, string name)
    {
      Id = id;
      Name = name.Trim();
    }
  }
}
