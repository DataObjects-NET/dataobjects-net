// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.11

using System;
using NUnit.Framework;
using Xtensive.Linq;
using Xtensive.Orm.Tests;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using System.Linq;

namespace Xtensive.Orm.Manual.Advanced.CustomSqlCompiler
{
  #region Model

  [Serializable]
  public class Address : Structure
  {
    [Field(Length = 200)]
    public string Country { get; set; }

    [Field(Length = 200)]
    public string City { get; set; }

    [Field(Length = 200)]
    public string Street { get; set; }

    [Field(Length = 200)]
    public string Building { get; set; }

    public Address(Session session)
      : base(session)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Name { get; set; }

    [Field]
    public Address Address { get; set; }

    public Person(Session session)
      : base(session)
    {}
  }

  #endregion

  #region Extension methods to compile

  public static class CustomSqlCompilerStringExtensions
  {
    public static char GetThirdChar(this string source)
    {
      return source[2];
    }

    public static string BuildAddressString(string country, string city, string building)
    {
      return $"{country}, {city}-{building}";
    }
  }

  #endregion

  #region Compiler container

  [CompilerContainer(typeof(SqlExpression))]
  public static class CustomSqlCompilerContainer
  {
    [Compiler(typeof(CustomSqlCompilerStringExtensions), "GetThirdChar", TargetKind.Method | TargetKind.Static)]
    public static SqlExpression GetThirdChar(SqlExpression _this)
    {
      return SqlDml.Substring(_this, 2, 1);
    }

    [Compiler(typeof(CustomSqlCompilerStringExtensions), "BuildAddressString", TargetKind.Method | TargetKind.Static)]
    public static SqlExpression BuildAddressString(SqlExpression countryExpression, SqlExpression streetExpression, SqlExpression buildingExpression)
    {
      return SqlDml.Concat(countryExpression, SqlDml.Literal(", "), streetExpression, SqlDml.Literal("-"), buildingExpression);
    }
  }

  #endregion
  
  [TestFixture]
  public class CustomSqlCompilerTest
  {
    private Domain existingDomain;

    [Test]
    public void GetThirdCharTest()
    {
      var domain = GetDomain();

      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var thirdChars = session.Query.All<Person>()
            .Select(p => p.Name.GetThirdChar())
            .OrderBy(thirdChar => thirdChar)
            .ToList();
          Assert.IsTrue(thirdChars.SequenceEqual(new[] {'a','r'}));
        }
      }
    }

    [Test]
    public void BuildAddressStringTest()
    {
      var domain = GetDomain();

      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var addresses = session.Query.All<Person>()
            .Select(p => CustomSqlCompilerStringExtensions.BuildAddressString(
              p.Address.Country, p.Address.City, p.Address.Building))
            .OrderBy(a => a)
            .ToList();
          var expectedAddresses = session.Query.All<Person>().AsEnumerable()
            .Select(p => CustomSqlCompilerStringExtensions.BuildAddressString(
              p.Address.Country, p.Address.City, p.Address.Building))
            .OrderBy(a=>a);
          Assert.IsTrue(addresses.SequenceEqual(expectedAddresses));
        }
      }
    }

    private Domain GetDomain()
    {
      if (existingDomain==null) {
        var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
        config.UpgradeMode = DomainUpgradeMode.Recreate;
        config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
        config.Types.Register(typeof (CustomSqlCompilerContainer));
        var domain = Domain.Build(config);
        using (var session = domain.OpenSession()) {
          using (var transactionScope = session.OpenTransaction()) {
            // Creating initial content
            new Person(session) {
              Name = "Tereza", Address = new Address (session) {
                Country = "Czech Republic", City = "Prague", Street = "Vinohradska", Building = "34"
              }
            };
            new Person(session) {
              Name = "Ivan", Address = new Address (session) {
                Country = "Russia", City = "Ekaterinburg", Street = "Lenina", Building = "11/2"
              }
            };

            transactionScope.Complete();
          }
        }
        existingDomain = domain;
      }
      return existingDomain;
    }
  }
}