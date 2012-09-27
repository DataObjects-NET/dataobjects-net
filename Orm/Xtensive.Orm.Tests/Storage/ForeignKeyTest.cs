// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.11

using System;
using NUnit.Framework;
using Xtensive.IoC;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests.Storage.ForeignKeys
{
  [Serializable]
  [KeyGenerator(KeyGeneratorKind.Custom, Name = "DualInt")]
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
  [KeyGenerator(KeyGeneratorKind.Custom, Name = "DualInt")]
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
  [KeyGenerator(KeyGeneratorKind.Custom, Name = "DualInt")]
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

  [Service(typeof (KeyGenerator), "DualInt")]
  public class DualIntKeyGenerator : KeyGenerator
  {
    private int seed = 1;

    public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
    {
    }

    public override Tuple GenerateKey(KeyInfo keyInfo, Session session)
    {
      return Tuple.Create(seed++, seed++);
    }
  }

  [TestFixture]
  public class ForeignKeyTest : AutoBuildTest
  {
    protected override Xtensive.Orm.Configuration.DomainConfiguration BuildConfiguration()
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
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction())
      {
        var u1 = new User { Name = "U1" };
        Session.Current.SaveChanges();
      }
    }

    [Test]
    public void InsertSelfReference()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var u1 = new User {Name = "U1"};
        u1.Boss = u1;
        Session.Current.SaveChanges();
      }
    }

    [Test]
    public void InsertInTypeSequence()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction())
      {
        var u1 = new User { Name = "U1" };
        var u2 = new User { Name = "U2" };
        u1.Boss = u2;
        Session.Current.SaveChanges();
      }
    }

    [Test]
    public void InsertInTypeLoop()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction())
      {
        var u1 = new User { Name = "U1" };
        var u2 = new User { Name = "U2" };
        u1.Boss = u2;
        u2.Boss = u1;
        Session.Current.SaveChanges();
      }
    }

    [Test]
    public void InsertMultyTypeSequence()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction())
      {
        var u1 = new User { Name = "U1" };
        var c1 = new Company { Name = "C1" };
        u1.Company = c1;
        Session.Current.SaveChanges();
      }
    }

    [Test]
    public void InsertMultyTypeLoop()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction())
      {
        var u1 = new User { Name = "U1" };
        var c1 = new Company { Name = "C1" };
        u1.Company = c1;
        c1.Director = u1;
        Session.Current.SaveChanges();
      }
    }

    
    // Update

    [Test]
    public void UpdateSelfReference()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction())
      {
        var u1 = new User { Name = "U1" };
        Session.Current.SaveChanges();
        u1.Boss = u1;
        Session.Current.SaveChanges();
      }
    }


    [Test]
    public void UpdateSequence()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction())
      {
        var c1 = new Company { Name = "C1" };
        var u1 = new User { Name = "U1" };
        c1.Director = u1;
        Session.Current.SaveChanges();
      }
    }

    [Test]
    public void UpdateLoop()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction())
      {
        var c1 = new Company { Name = "C1" };
        var u1 = new User { Name = "U1" };
        c1.Director = u1;
        u1.Company = c1;
        Session.Current.SaveChanges();
      }
    }

    [Test]
    public void UpdateToInserted()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction())
      {
        var c1 = new Company { Name = "C1" };
        Session.Current.SaveChanges();
        var u1 = new User { Name = "U1" };
        c1.Director = u1;
        Session.Current.SaveChanges();
      }
    }

    [Test]
    public void UpdateToDeleted()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction())
      {
        var c1 = new Company { Name = "C1" };
        var u1 = new User { Name = "U1" };
        c1.Director = u1;
        u1.Company = c1;
        Session.Current.SaveChanges();
        u1.Remove();
        Session.Current.SaveChanges();
      }
    }

    // Delete

    [Test]
    public void DeleteSelfReference()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction())
      {
        var u1 = new User { Name = "U1" };
        u1.Boss = u1;
        Session.Current.SaveChanges();
        u1.Remove();
        Session.Current.SaveChanges();
      }
    }

    // Intermediate Entity

    [Test]
    public void InsertIntermediateEntity()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction())
      {
        var u1 = new User { Name = "U1" };
        var p1 = new Project { Name = "P1" };
        u1.Projects.Add(p1);
        Session.Current.SaveChanges();
      }
    }

    // Mixed test
  }
}