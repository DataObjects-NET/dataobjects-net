// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.11

using System;
using System.Linq;
using System.Threading;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Aspects;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Storage.Rse;
using Xtensive.Reflection;

namespace Xtensive.Orm.Manual.Concurrency.Versions
{
  #region Model

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Name { get; set; }

    [Field(Length = 200)]
    public string SecondName { get; set; }

    [Field]
    [Association(PairTo = "Friends")]
    public EntitySet<Person> Friends { get; private set; }

    [Field]
    public Company Company { get; set; }

    public string FullName {
      get {
        return "{Name} {SecondName}".FormatWith(this);
      }
    }

    public override string ToString()
    {
      return ToString(false);
    }

    public string ToString(bool withFriends)
    {
      if (withFriends)
        return "Person('{0}', Friends={{1}})".FormatWith(FullName, Friends.ToCommaDelimitedString());
      else
        return "Person('{0}')".FormatWith(FullName);
    }

    public Person(string fullName)
    {
      var pair = fullName.RevertibleSplitFirstAndTail('\\', ',');
      SecondName = pair.First.Trim();
      Name = pair.Second.Trim();
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Company : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Name { get; set; }

    [Field, Version] 
    // If HandleVersionInfoUpdate() isn't implemented, default implementation
    // (relying on VersionGenerator.GenerateNextVersion(...) method) is used.
    public int Version { get; private set; }

    [Field]
    [Association(PairTo = "Company")]
    // By default any EntitySet change also leads to Version change
    public EntitySet<Person> Employees { get; private set; }

    public override string ToString()
    {
      return ToString(true);
    }

    public string ToString(bool withPersons)
    {
      if (withPersons)
        return "Company('{0}', Employees: {1})".FormatWith(Name, Employees.ToCommaDelimitedString());
      else
        return "Company('{0}')".FormatWith(Name);
    }

    public Company(string name)
    {
      Name = name;
    }
  }

  #endregion
  
  [TestFixture]
  public class VersionsTest
  {
    private Domain existingDomain;

    [Test]
    public void CombinedTest()
    {
      var domain = GetDomain();

      using (var session = domain.OpenSession()) {
        Person alex;
        VersionInfo alexVersion;
        Person dmitri;
        VersionInfo dmitriVersion;
        Company xtensive;
        VersionInfo xtensiveVersion;
        using (var tx = session.OpenTransaction()) {
          alex = new Person("Yakunin, Alex");
          alexVersion = alex.VersionInfo;
          Dump(alex);
          dmitri = new Person("Maximov, Dmitri");
          dmitriVersion = dmitri.VersionInfo;
          Dump(dmitri);

          alex.Friends.Add(dmitri);
          // Automatically provided versions won't change because of change in EntitySet!
          Assert.AreEqual(alexVersion, alex.VersionInfo);
          Assert.AreEqual(dmitriVersion, dmitri.VersionInfo);

          xtensive = new Company("X-tensive.com");
          xtensiveVersion = xtensive.VersionInfo;
          Dump(xtensive);
          tx.Complete();
        }

        using (var tx = session.OpenTransaction()) {
          string newName = "Xtensive";
          Console.WriteLine("Changing {0} name to {1}", xtensive.Name, newName);
          xtensive.Name = newName; 
          Dump(xtensive);
          Assert.AreNotEqual(xtensiveVersion, xtensive.VersionInfo); // company version changed
          xtensiveVersion = xtensive.VersionInfo;

          Console.WriteLine("Xtensive.Employees.Add(Alex)");
          xtensive.Employees.Add(alex);
          Dump(xtensive);
          Assert.AreEqual(xtensiveVersion, xtensive.VersionInfo); // company version already updated in current transaction
          Assert.AreNotEqual(alexVersion, alex.VersionInfo);
          xtensiveVersion = xtensive.VersionInfo;
          alexVersion = alex.VersionInfo;
          tx.Complete();
        }

        int xtensiveVersionFieldValue;
        using (var tx = session.OpenTransaction()) {
        Console.WriteLine("Dmitri.Company = Xtensive");
          dmitri.Company = xtensive;
          Dump(xtensive);
          Assert.AreNotEqual(xtensiveVersion, xtensive.VersionInfo);
          Assert.AreNotEqual(dmitriVersion, dmitri.VersionInfo);
          xtensiveVersion = xtensive.VersionInfo;
          dmitriVersion = dmitri.VersionInfo;

          Console.WriteLine("Transaction rollback test, before:");
          Dump(xtensive);
          xtensiveVersionFieldValue = xtensive.Version;
          tx.Complete();
        }

        using (var tx = session.OpenTransaction()) {
          xtensive.Employees.Remove(alex);
          // Xtensive version is changed
          var newXtensiveVersionInsideTransaction = xtensive.VersionInfo;
          Assert.AreNotEqual(xtensiveVersion, newXtensiveVersionInsideTransaction);
          Assert.AreEqual(xtensiveVersionFieldValue, xtensive.Version - 1); // Incremented
          // Alex version is changed
          Assert.AreNotEqual(alexVersion, alex.VersionInfo);

          xtensive.Employees.Remove(dmitri);
          // Xtensive version is NOT changed, since we try to update each version
          // just once per transaction
          Assert.AreEqual(newXtensiveVersionInsideTransaction, xtensive.VersionInfo);
          Assert.AreEqual(xtensiveVersionFieldValue, xtensive.Version - 1); // No increment now
          // Dmitri's version is changed
          Assert.AreNotEqual(dmitriVersion, dmitri.VersionInfo);

          Console.WriteLine("Transaction rollback test, inside:");
          Dump(xtensive);
          // tx.Complete(); // Rollback!
        }

        Console.WriteLine("Transaction rollback test, after:");

        using (var tx = session.OpenTransaction()) {
          Dump(xtensive);

          // Let's check if everything is rolled back
          Assert.AreEqual(xtensiveVersion, xtensive.VersionInfo);
          Assert.AreEqual(xtensiveVersionFieldValue, xtensive.Version);
          Assert.AreEqual(xtensiveVersion, xtensive.VersionInfo);
          Assert.AreEqual(dmitriVersion, dmitri.VersionInfo);
        }
      }
    }

    [Test]
    public void VersionValidatorTest()
    {
      var domain = GetDomain();

      using (var session = domain.OpenSession()) {
        var versions = new VersionSet();

        Person alex;
        Person dmitri;
        using (var tx = session.OpenTransaction()) {
          alex = new Person("Yakunin, Alex");
          dmitri = new Person("Maximov, Dmitri");
          tx.Complete();
        }

        using (VersionCapturer.Attach(versions))
        using (var tx = session.OpenTransaction()) {
          // Simulating entity displaying @ web page
          // By default this leads to their addition to VersionSet
          Dump(alex);
          Dump(dmitri);
          tx.Complete();
        }

        // Let's clone VersionSet (actually - serialize & deserialize)
        versions = Cloner.Default.Clone(versions);
        // And dump it
        Dump(versions);

        using (VersionValidator.Attach(versions))
        using (var tx = session.OpenTransaction()) {
          alex.Name = "Edited Alex"; // Passes
          alex.Name = "Edited again"; // Passes, because this is not the very first modification
          tx.Complete();
        }

        AssertEx.Throws<VersionConflictException>(() => {
          using (VersionValidator.Attach(versions))
          using (var tx = session.OpenTransaction()) {
            alex.Name = "And again"; 
            tx.Complete(); 
          } // Version check fails on Session.Persist() here
        });

        using (var tx = session.OpenTransaction())
          versions.Add(alex, true); // Overwriting versions
        Dump(versions);

        using (VersionValidator.Attach(versions))
        using (var tx = session.OpenTransaction()) {
          alex.Name = "Edited again"; // Passes now
          tx.Complete();
        }
      }
    }

    private void Dump(Entity entity)
    {
      Console.WriteLine("Entity: {0}", entity);
      Console.WriteLine("          Key: {0}", entity.Key);
      Console.WriteLine("  VersionInfo: {0}", entity.VersionInfo);
      Console.WriteLine();
    }

    private void Dump(VersionSet versions)
    {
      Console.WriteLine("VersionSet: \r\n{0}",
        versions
        .Select(pair => new Pair<Key, VersionInfo>(pair.Key, pair.Value))
        .ToDelimitedString("\r\n")
        .Indent(2, true)
        );
      Console.WriteLine();
    }

    private Domain GetDomain()
    {
      if (existingDomain==null) {
        var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests") {
          UpgradeMode = DomainUpgradeMode.Recreate
        };
        config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
        var domain = Domain.Build(config);
        existingDomain = domain;
      }
      return existingDomain;
    }
  }
}