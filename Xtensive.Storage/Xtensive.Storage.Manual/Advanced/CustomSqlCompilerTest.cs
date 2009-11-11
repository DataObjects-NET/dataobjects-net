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
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Name { get; set; }

  }

  [TestFixture]
  public class CustomSqlCompilerTest
  {
    [Test]
    public void MainTest()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (Person));
      config.CompilerContainers.Register(typeof (CustomStringCompilerContainer));
      var domain = Domain.Build(config);
      using (var session = Session.Open(domain)) {
        using (Transaction.Open(session)) {
          var person1 = new Person {Name = "John"};
          var person2 = new Person {Name = "Ivan"};
          var names = Query<Person>.All.Select(p => p.Name).ToList();
          var substrings = Query<Person>.All.Select(p => p.Name.Substring(2, 3));
          var thirdChars = Query<Person>.All.Select(p => p.Name.GetThirdChar()).OrderBy(c=>c).ToList();
          Assert.IsTrue(thirdChars.SequenceEqual(new[]{'a','h'}));
        }
      }
    }
  }
}