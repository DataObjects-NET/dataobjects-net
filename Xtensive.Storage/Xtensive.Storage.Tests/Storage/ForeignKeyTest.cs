// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.11

using NUnit.Framework;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Tests.Storage.ForeignKeys
{
  [HierarchyRoot("Id1", "Id2", KeyGenerator = typeof (DualIntKeyGenerator))]
  public class User : Entity
  {
    [Field]
    public int Id1 { get; private set; }

    [Field]
    public int Id2 { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public User Boss { get; set; }

    [Field]
    public Company Company { get; set; }
  }

  [HierarchyRoot("Id3", "Id4", KeyGenerator = typeof(DualIntKeyGenerator))]
  public class Company : Entity
  {
    [Field]
    public int Id3 { get; private set; }

    [Field]
    public int Id4 { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public User Director { get; set; }
  }

  public class DualIntKeyGenerator : KeyGenerator
  {
    private int seed = 1;

    public DualIntKeyGenerator(GeneratorInfo generatorInfo)
      : base(generatorInfo)
    {
    }

    public override Tuple Next()
    {
      return Tuple.Create(seed++, seed++);
    }
  }

  [TestFixture]
  public class ForeignKeyTest : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (User).Assembly, typeof (User).Namespace);
      configuration.ForeignKeyMode = ForeignKeyMode.All;
      return configuration;
    }


    // Insert

    [Test]
    public void InsertSelfReference()
    {
      using (Domain.OpenSession())
      using (Transaction.Open()) {
        var u1 = new User {Name = "U1"};
        u1.Boss = u1;
        Session.Current.Persist();
      }
    }

    [Test]
    public void InsertInTypeSequence()
    {
      using (Domain.OpenSession())
      using (Transaction.Open())
      {
        var u1 = new User { Name = "U1" };
        var u2 = new User { Name = "U2" };
        u1.Boss = u2;
        Session.Current.Persist();
      }
    }

    [Test]
    public void InsertInTypeLoop()
    {
      using (Domain.OpenSession())
      using (Transaction.Open())
      {
        var u1 = new User { Name = "U1" };
        var u2 = new User { Name = "U2" };
        u1.Boss = u2;
        u2.Boss = u1;
        Session.Current.Persist();
      }
    }

    [Test]
    public void InsertMultyTypeSequence()
    {
      using (Domain.OpenSession())
      using (Transaction.Open())
      {
        var u1 = new User { Name = "U1" };
        var c1 = new Company { Name = "C1" };
        u1.Company = c1;
        Session.Current.Persist();
      }
    }

    [Test]
    public void InsertMultyTypeLoop()
    {
      using (Domain.OpenSession())
      using (Transaction.Open())
      {
        var u1 = new User { Name = "U1" };
        var c1 = new Company { Name = "C1" };
        u1.Company = c1;
        c1.Director = u1;
        Session.Current.Persist();
      }
    }

    
    // Update

    [Test]
    public void UpdateSelfReference()
    {
      using (Domain.OpenSession())
      using (Transaction.Open())
      {
        var u1 = new User { Name = "U1" };
        Session.Current.Persist();
        u1.Boss = u1;
        Session.Current.Persist();
      }
    }


    [Test]
    public void UpdateSequence()
    {
      using (Domain.OpenSession())
      using (Transaction.Open())
      {
        var c1 = new Company { Name = "C1" };
        var u1 = new User { Name = "U1" };
        c1.Director = u1;
        Session.Current.Persist();
      }
    }

    [Test]
    public void UpdateLoop()
    {
      using (Domain.OpenSession())
      using (Transaction.Open())
      {
        var c1 = new Company { Name = "C1" };
        var u1 = new User { Name = "U1" };
        c1.Director = u1;
        u1.Company = c1;
        Session.Current.Persist();
      }
    }

    [Test]
    public void UpdateToInserted()
    {
      using (Domain.OpenSession())
      using (Transaction.Open())
      {
        var c1 = new Company { Name = "C1" };
        Session.Current.Persist();
        var u1 = new User { Name = "U1" };
        c1.Director = u1;
        Session.Current.Persist();
      }
    }

    [Test]
    public void UpdateToDeleted()
    {
      using (Domain.OpenSession())
      using (Transaction.Open())
      {
        var c1 = new Company { Name = "C1" };
        var u1 = new User { Name = "U1" };
        Session.Current.Persist();
        c1.Director = u1;
        u1.Company = c1;
        Session.Current.Persist();
        u1.Remove();
        Session.Current.Persist();
      }
    }

    // Delete

    [Test]
    public void DeleteSelfReference()
    {
      using (Domain.OpenSession())
      using (Transaction.Open())
      {
        var u1 = new User { Name = "U1" };
        u1.Boss = u1;
        Session.Current.Persist();
        u1.Remove();
        Session.Current.Persist();
      }
    }

    // Mixed test
  }
}