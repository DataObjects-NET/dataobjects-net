// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.11

using System;
using NUnit.Framework;
using Xtensive.Core.IoC;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Tests.Storage.ForeignKeys
{
  [Serializable]
  [KeyGenerator(typeof(DualIntKeyGenerator), Name = "DualInt")]
  [HierarchyRoot]
  public class User : Entity
  {
    [Field, Key(0)]
    public int Id1 { get; private set; }

    [Field, Key(1)]
    public int Id2 { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public User Boss { get; set; }

    [Field]
    [Association(OnTargetRemove=OnRemoveAction.Clear)]
    public Company Company { get; set; }

    [Field]
    public EntitySet<Project> Projects { get; private set; }
  }

  [Serializable]
  [KeyGenerator(typeof(DualIntKeyGenerator), Name = "DualInt")]
  [HierarchyRoot]
  public class Company : Entity
  {
    [Field, Key(0)]
    public int Id3 { get; private set; }

    [Field, Key(1)]
    public int Id4 { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public User Director { get; set; }
  }

  [Serializable]
  [KeyGenerator(typeof(DualIntKeyGenerator), Name = "DualInt")]
  [HierarchyRoot]
  public class Project : Entity
  {
    [Field, Key(0)]
    public int Id5 { get; private set; }

    [Field, Key(1)]
    public int Id6 { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field, Association(PairTo = "Projects")]
    public EntitySet<User> Users { get; private set; }
  }

  [Serializable]
  public class ForeignUser : User
  {
    [Field]
    public string Country { get; set; }
  }

  [Service(typeof(DualIntKeyGenerator), "DualInt")]
  public class DualIntKeyGenerator : KeyGenerator<int>
  {
    private int seed = 1;

    public override Tuple Next(bool temporaryKey)
    {
      return Tuple.Create(KeyProviderInfo.KeyTupleDescriptor, seed++, seed++);
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
    public void InsertDescendant()
    {
      using (Session.Open(Domain))
      using (Transaction.Open())
      {
        var u1 = new User { Name = "U1" };
        Session.Current.Persist();
      }
    }

    [Test]
    public void InsertSelfReference()
    {
      using (Session.Open(Domain))
      using (Transaction.Open()) {
        var u1 = new User {Name = "U1"};
        u1.Boss = u1;
        Session.Current.Persist();
      }
    }

    [Test]
    public void InsertInTypeSequence()
    {
      using (Session.Open(Domain))
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
      using (Session.Open(Domain))
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
      using (Session.Open(Domain))
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
      using (Session.Open(Domain))
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
      using (Session.Open(Domain))
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
      using (Session.Open(Domain))
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
      using (Session.Open(Domain))
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
      using (Session.Open(Domain))
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
      using (Session.Open(Domain))
      using (Transaction.Open())
      {
        var c1 = new Company { Name = "C1" };
        var u1 = new User { Name = "U1" };
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
      using (Session.Open(Domain))
      using (Transaction.Open())
      {
        var u1 = new User { Name = "U1" };
        u1.Boss = u1;
        Session.Current.Persist();
        u1.Remove();
        Session.Current.Persist();
      }
    }

    // Intermediate Entity

    [Test]
    public void InsertIntermediateEntity()
    {
      using (Session.Open(Domain))
      using (Transaction.Open())
      {
        var u1 = new User { Name = "U1" };
        var p1 = new Project { Name = "P1" };
        u1.Projects.Add(p1);
        Session.Current.Persist();
      }
    }

    // Mixed test
  }
}