// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.11

using System;
using System.Linq;
using System.Threading;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Core.Aspects;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Manual.Concurrency.Versions
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
    public int Version { get; private set; }

    [Field]
    public EntitySet<Person> Employees { get; private set; }

    public override string ToString()
    {
      return ToString(false);
    }

    public string ToString(bool withPersons)
    {
      if (withPersons)
        return "Company('{0}', Employees={{1}})".FormatWith(Name, Employees.ToCommaDelimitedString());
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

      using (Session.Open(domain)) {
        // Auto transactions!
        var alex = new Person("Yakunin, Alex");
        var alexVersion = alex.VersionInfo;
        Dump(alex);
        var dmitri = new Person("Maximov, Dmitri");
        var dmitriVersion = dmitri.VersionInfo;
        Dump(dmitri);

        alex.Friends.Add(dmitri);
        // Versions won't change!
        Assert.AreEqual(alexVersion, alex.VersionInfo);
        Assert.AreEqual(dmitriVersion, dmitri.VersionInfo);

        var xtensive = new Company("X-tensive.com");
        var xtensiveVersion = xtensive.VersionInfo;
        Dump(xtensive);
        
        string newName = "Xtensive";
        Console.WriteLine("Changing {0} name to {1}");
        xtensive.Name = newName;
        Dump(xtensive);
        Assert.AreNotEqual(xtensiveVersion, xtensive.VersionInfo);
      }
    }

    private void Dump(Entity entity)
    {
      Console.WriteLine("Entity: {0}", entity);
      Console.WriteLine("          Key: {0}", entity.Key);
      Console.WriteLine("  VersionInfo: {0}", entity.VersionInfo);
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