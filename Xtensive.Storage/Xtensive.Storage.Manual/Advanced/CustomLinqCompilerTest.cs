// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.16

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Manual.Advanced.CustomLinqCompiler
{
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field]
    [Key]
    public int Id { get; private set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public DateTime BirthDay { get; set; }

    public string Fullname
    {
      get { return string.Format("{0} {1}", FirstName, LastName); }
    }

    public string AddPrefix(string prefix)
    {
      return string.Format("{0}{1}", prefix, LastName);
    }
  }

  [TestFixture]
  public class CustomLinqCompilerTest
  {
    [Test]
    public void PropertyTest()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (Person).Assembly, typeof(Person).Namespace);
      config.CompilerContainers.Register(typeof (CustomLinqCompilerContainer));
      var domain = Domain.Build(config);
      using (var session = Session.Open(domain)) {
        using (Transaction.Open(session)) {
          Fill();
          var expectedFullNames = Query<Person>.All.AsEnumerable().OrderBy(p => p.Id).Select(p => p.Fullname);
          Assert.Greater(expectedFullNames.Count(), 0);
          var fullNames = Query<Person>.All.OrderBy(p => p.Id).Select(p => p.Fullname);
          Assert.IsTrue(expectedFullNames.SequenceEqual(fullNames));
        }
      }
    }

    [Test]
    public void MethodTest()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (Person).Assembly, typeof(Person).Namespace);
      config.CompilerContainers.Register(typeof (CustomLinqCompilerContainer));
      var domain = Domain.Build(config);
      using (var session = Session.Open(domain)) {
        using (Transaction.Open(session)) {
          Fill();
          var expectedStrings = Query<Person>.All.AsEnumerable().OrderBy(p => p.Id).Select(p => p.AddPrefix("Mr. "));
          var resultStrings = Query<Person>.All.OrderBy(p => p.Id).Select(p => p.AddPrefix("Mr. "));
          Assert.IsTrue(expectedStrings.SequenceEqual(resultStrings));
        }
      }
    }

    private void Fill()
    {
      new Person {FirstName = "Ivan", LastName = "Semenov"};
      new Person {FirstName = "John", LastName = "Smith"};
      new Person {FirstName = "Andrew", LastName = "Politkovsky"};
    }
  }
}