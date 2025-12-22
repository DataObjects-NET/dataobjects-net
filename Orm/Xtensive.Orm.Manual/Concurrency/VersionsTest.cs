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
using Xtensive.Core;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse;
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
        return $"{Name} {SecondName}";
      }
    }

    public override string ToString()
    {
      return ToString(false);
    }

    public string ToString(bool withFriends)
    {
      if (withFriends)
        return $"Person('{FullName}', Friends={Friends.ToCommaDelimitedString()})";
      return $"Person('{FullName}')";
    }

    public Person(Session session, string fullName)
      : base (session)
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
        return $"Company('{Name}', Employees: {Employees.ToCommaDelimitedString()})";
      return $"Company('{Name}')";
    }

    public Company(Session session, string name)
      : base (session)
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
          alex = new Person(session, "Yakunin, Alex");
          alexVersion = alex.VersionInfo;
          Dump(alex);
          dmitri = new Person(session, "Maximov, Dmitri");
          dmitriVersion = dmitri.VersionInfo;
          Dump(dmitri);

          alex.Friends.Add(dmitri);
          // Automatically provided versions won't change because of change in EntitySet!
          Assert.That(alex.VersionInfo, Is.EqualTo(alexVersion));
          Assert.That(dmitri.VersionInfo, Is.EqualTo(dmitriVersion));

          xtensive = new Company (session, "X-tensive.com");
          xtensiveVersion = xtensive.VersionInfo;
          Dump(xtensive);
          tx.Complete();
        }

        using (var tx = session.OpenTransaction()) {
          string newName = "Xtensive";
          Console.WriteLine($"Changing {xtensive.Name} name to {newName}");
          xtensive.Name = newName; 
          Dump(xtensive);
          Assert.That(xtensive.VersionInfo, Is.Not.EqualTo(xtensiveVersion)); // company version changed
          xtensiveVersion = xtensive.VersionInfo;

          Console.WriteLine("Xtensive.Employees.Add(Alex)");
          xtensive.Employees.Add(alex);
          Dump(xtensive);
          Assert.That(xtensive.VersionInfo, Is.EqualTo(xtensiveVersion)); // company version already updated in current transaction
          Assert.That(alex.VersionInfo, Is.Not.EqualTo(alexVersion));
          xtensiveVersion = xtensive.VersionInfo;
          alexVersion = alex.VersionInfo;
          tx.Complete();
        }

        int xtensiveVersionFieldValue;
        using (var tx = session.OpenTransaction()) {
        Console.WriteLine("Dmitri.Company = Xtensive");
          dmitri.Company = xtensive;
          Dump(xtensive);
          Assert.That(xtensive.VersionInfo, Is.Not.EqualTo(xtensiveVersion));
          Assert.That(dmitri.VersionInfo, Is.Not.EqualTo(dmitriVersion));
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
          Assert.That(newXtensiveVersionInsideTransaction, Is.Not.EqualTo(xtensiveVersion));
          Assert.That(xtensive.Version - 1, Is.EqualTo(xtensiveVersionFieldValue)); // Incremented
          // Alex version is changed
          Assert.That(alex.VersionInfo, Is.Not.EqualTo(alexVersion));

          xtensive.Employees.Remove(dmitri);
          // Xtensive version is NOT changed, since we try to update each version
          // just once per transaction
          Assert.That(xtensive.VersionInfo, Is.EqualTo(newXtensiveVersionInsideTransaction));
          Assert.That(xtensive.Version - 1, Is.EqualTo(xtensiveVersionFieldValue)); // No increment now
          // Dmitri's version is changed
          Assert.That(dmitri.VersionInfo, Is.Not.EqualTo(dmitriVersion));

          Console.WriteLine("Transaction rollback test, inside:");
          Dump(xtensive);
          // tx.Complete(); // Rollback!
        }

        Console.WriteLine("Transaction rollback test, after:");

        using (var tx = session.OpenTransaction()) {
          Dump(xtensive);

          // Let's check if everything is rolled back
          Assert.That(xtensive.VersionInfo, Is.EqualTo(xtensiveVersion));
          Assert.That(xtensive.Version, Is.EqualTo(xtensiveVersionFieldValue));
          Assert.That(xtensive.VersionInfo, Is.EqualTo(xtensiveVersion));
          Assert.That(dmitri.VersionInfo, Is.EqualTo(dmitriVersion));
        }
      }
    }

    [Test]
    public void VersionValidatorTest()
    {
      var domain = GetDomain();

      using (var session = domain.OpenSession())
      using (var scope = session.Activate()) {
        var versions = new VersionSet();

        Person alex;
        Person dmitri;
        using (var tx = session.OpenTransaction()) {
          alex = new Person(session, "Yakunin, Alex");
          dmitri = new Person(session, "Maximov, Dmitri");
          tx.Complete();
        }

        using (VersionCapturer.Attach(session, versions))
        using (var tx = session.OpenTransaction()) {
          // Simulating entity displaying @ web page
          // By default this leads to their addition to VersionSet
          Dump(alex);
          Dump(dmitri);
          tx.Complete();
        }

        // Let's clone VersionSet (actually - serialize & deserialize)
        versions = Cloner.Clone(versions);
        // And dump it
        Dump(versions);

        using (VersionValidator.Attach(session, versions))
        using (var tx = session.OpenTransaction()) {
          alex.Name = "Edited Alex"; // Passes
          alex.Name = "Edited again"; // Passes, because this is not the very first modification
          tx.Complete();
        }

        AssertEx.Throws<VersionConflictException>(() => {
          using (VersionValidator.Attach(session, versions))
          using (var tx = session.OpenTransaction()) {
            alex.Name = "And again"; 
            tx.Complete(); 
          } // Version check fails on Session.Persist() here
        });

        using (var tx = session.OpenTransaction())
          versions.Add(alex, true); // Overwriting versions
        Dump(versions);

        using (VersionValidator.Attach(session, versions))
        using (var tx = session.OpenTransaction()) {
          alex.Name = "Edited again"; // Passes now
          tx.Complete();
        }
      }
    }

    private void Dump(Entity entity)
    {
      Console.WriteLine($"Entity: {entity}");
      Console.WriteLine($"          Key: {entity.Key}");
      Console.WriteLine($"  VersionInfo: {entity.VersionInfo}");
      Console.WriteLine();
    }

    private void Dump(VersionSet versions)
    {
      Console.WriteLine($"VersionSet: \r\n{versions.Select(pair => new Pair<Key, VersionInfo>(pair.Key, pair.Value)).ToDelimitedString("\r\n").Indent(2, true)}");
      Console.WriteLine();
    }

    private Domain GetDomain()
    {
      if (existingDomain==null) {
        var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
        config.UpgradeMode = DomainUpgradeMode.Recreate;
        config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
        var domain = Domain.Build(config);
        existingDomain = domain;
      }
      return existingDomain;
    }
  }
}