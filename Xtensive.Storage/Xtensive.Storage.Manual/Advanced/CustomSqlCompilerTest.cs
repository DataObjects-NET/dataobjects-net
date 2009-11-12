// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.11

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using System.Linq;

namespace Xtensive.Storage.Manual.Advanced
{
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
  }

  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Name { get; set; }

    [Field]
    public Address Address { get; set; }
  }

  [TestFixture]
  public class CustomSqlCompilerTest
  {
    [Test]
    public void GetThirdCharTest()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (Person).Assembly, typeof(Person).Namespace);
      config.CompilerContainers.Register(typeof (CustomStringCompilerContainer));
      var domain = Domain.Build(config);
      using (var session = Session.Open(domain)) {
        using (Transaction.Open(session)) {
          AddPersons();
          var thirdChars = Query<Person>.All.Select(p => p.Name.GetThirdChar());
          var orderedThirdChars = thirdChars.OrderBy(c=>c).ToList();
          Assert.IsTrue(orderedThirdChars.SequenceEqual(new[]{'a','r'}));
        }
      }
    }

    [Test]
    public void BuildAddress()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (Person).Assembly, typeof(Person).Namespace);
      config.CompilerContainers.Register(typeof (CustomStringCompilerContainer));
      var domain = Domain.Build(config);
      using (var session = Session.Open(domain)) {
        using (Transaction.Open(session)) {
          AddPersons();
          var addresses = Query<Person>.All.Select(p => CustomCompilerStringExtensions.BuildAddressString(p.Address.Country, p.Address.City, p.Address.Building));
          var orderedAddresses = addresses.OrderBy(c=>c).ToList();
          var addressesExpected = Query<Person>.All.AsEnumerable().Select(p => CustomCompilerStringExtensions.BuildAddressString(p.Address.Country, p.Address.City, p.Address.Building)).OrderBy(a=>a);
          Assert.IsTrue(orderedAddresses.SequenceEqual(addressesExpected));
        }
      }
    }

    private void AddPersons()
    {
      new Person {Name = "Tereza", Address = new Address {Country="Czech Republic", City="Prague", Street="Vinohradska", Building="34"}};
      new Person {Name = "Ivan", Address = new Address {Country="Russia", City="Ekaterinburg", Street="Lenina", Building="11/2"}};
    }
  }
}