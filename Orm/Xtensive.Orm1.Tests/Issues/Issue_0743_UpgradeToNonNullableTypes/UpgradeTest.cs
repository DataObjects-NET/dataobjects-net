// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Tests.Issues.Issue_0743_UpgradeToNonNullableTypes.Model.Version2;
using M1 = Xtensive.Orm.Tests.Issues.Issue_0743_UpgradeToNonNullableTypes.Model.Version1;
using M2 = Xtensive.Orm.Tests.Issues.Issue_0743_UpgradeToNonNullableTypes.Model.Version2;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Issues.Issue_0743_UpgradeToNonNullableTypes
{
  [TestFixture]
  public class UpgradeTest
  {
    private Domain domain;

    [SetUp]
    public void SetUp()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate);
      using (var session = domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var person1 = new Model.Version1.Person {
            Name = "Person",
            Age = 30,
            Bytes = new byte[] {1,2,3}
          };
          var person2 = new Model.Version1.Person();
          person1.Friend = person2;
          person2.Friend = person1;
          tx.Complete();
        }
      }
    }
    
    [Test]
    public void UpgradeToVersion2Test()
    {
      BuildDomain("2", DomainUpgradeMode.Perform);
      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var persons = (
            from p in session.Query.All<Person>()
            orderby p.Name
            select p
            ).ToList();
          Assert.AreEqual(3, persons.Count);
          
          var none = persons[1];
          var person1 = persons[2];
          var person2 = persons[0];
          var person3 = new Person() {Friend = person1};
          
          Assert.IsNull(person3.Name);
          person3.Name = string.Empty;

          persons.Add(person3);
          Session.Demand().SaveChanges();

          Assert.AreEqual("Person", person1.Name);
          Assert.AreEqual(30, person1.Age);
          Assert.AreEqual(person2, person1.Friend);
          AssertEx.HasSameElements(new byte[] {1,2,3}, person1.Bytes);

          Assert.AreEqual(string.Empty, person2.Name);
          Assert.AreEqual(-1, person2.Age);
          Assert.AreEqual(person1, person2.Friend);

          Assert.AreEqual(string.Empty, person3.Name);
          Assert.AreEqual(-1, person3.Age);
          Assert.AreEqual(person1, person3.Friend);

          Assert.AreEqual("None", none.Name);
          Assert.AreEqual(-1, none.Age);
          Assert.AreEqual(none, none.Friend);

          foreach (var person in persons) {
            Assert.AreEqual("A", person.DefaultTest1);
            Assert.AreEqual(1, person.DefaultTest2);
            Assert.AreEqual(new DateTime(1900,01,01), person.DefaultTest3);
            if (person.Bytes.Length!=3)
              AssertEx.HasSameElements(new byte[] {0}, person.Bytes);
            // Structure defaults test
            Assert.AreEqual("A", person.DefaultTest4.DefaultTest1);
            Assert.AreEqual(1, person.DefaultTest4.DefaultTest2);
            Assert.AreEqual(new DateTime(1900,01,01), person.DefaultTest4.DefaultTest3);
          }
        }
      }
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();

      string ns = typeof(Model.Version1.Person).Namespace;
      string nsPrefix = ns.Substring(0, ns.Length - 1);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nsPrefix + version);
      configuration.Types.Register(typeof(Upgrader));

      using (Upgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }
  }
}