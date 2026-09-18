// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.08

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Interfaces.InterfaceTest_Model;

namespace Xtensive.Orm.Tests.Interfaces.InterfaceTest_Model
{
  public interface IPerson : IEntity
  {
    [Field]
    string Name { get; set; }

    [Field]
    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    EntitySet<IAnimal> Pets { get; }

    [Field]
    IAnimal Favorite { get; set; }
  }

  public interface IAnimal : IEntity
  {
    int Id { get; }

    [Field]
    string PetName { get; set; }

    [Field]
    IPerson Owner { get; set; }
  }

  [HierarchyRoot]
  public class Person1 : Entity, IPerson
  {
    [Field, Key]
    public int Id { get; private set; }

    public string Name { get; set; }

    public EntitySet<IAnimal> Pets { get; private set; }

    public IAnimal Favorite { get; set; }
  }

  [HierarchyRoot]
  public class Person2 : Entity, IPerson
  {
    [Field, Key]
    public int Id { get; private set; }

    public string Name { get; set; }

    public EntitySet<IAnimal> Pets { get; private set; }

    public IAnimal Favorite { get; set; }
  }

  [HierarchyRoot]
  public class Animal1 : Entity, IAnimal
  {
    [Field, Key]
    public int Id { get; private set; }

    public string PetName { get; set; }

    public IPerson Owner { get; set; }
  }

  [HierarchyRoot]
  public class Animal2 : Entity, IAnimal
  {
    [Field, Key]
    public int Id { get; private set; }

    public string PetName { get; set; }

    public IPerson Owner { get; set; }
  }

  [HierarchyRoot]
  public class Animal3 : Entity, IAnimal
  {
    [Field, Key]
    public int Id { get; private set; }

    public string PetName { get; set; }

    public IPerson Owner { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Interfaces
{
  public class InterfaceTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof (Person1).Assembly, typeof (Person1).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {

        var iP = (IPerson) new Person1();
        _ = iP.Pets.Add(new Animal1());
        _ = iP.Pets.Add(new Animal1());
        _ = iP.Pets.Add(new Animal2());

        iP = new Person2();
        _ = iP.Pets.Add(new Animal1());
        _ = iP.Pets.Add(new Animal1());
        _ = iP.Pets.Add(new Animal2());

        session.SaveChanges();

        iP = session.Query.All<IPerson>().First();
        Assert.That(iP.Pets.Count, Is.EqualTo(3));

        var first = iP.Pets.First();
        first.Remove();
        Assert.That(first.PersistenceState == PersistenceState.Removed, Is.True);
        Assert.That(iP.Pets.Contains(first), Is.False);
        Assert.That(iP.Pets.Count, Is.EqualTo(2));

        iP.Remove();
        session.Remove(session.Query.All<IAnimal>());

        _ = new Animal1() { PetName = "A" };
        _ = new Animal1() { PetName = "B" };
        _ = new Animal1() { PetName = "C" };
        _ = new Animal2() { PetName = "D" };
        _ = new Animal2() { PetName = "E" };
        _ = new Animal2() { PetName = "F" };
        _ = new Animal3() { PetName = "G" };
        _ = new Animal3() { PetName = "H" };
        _ = new Animal3() { PetName = "J" };

        var animals = session.Query.All<IAnimal>();
        Assert.That(animals.Count(), Is.EqualTo(9));

        _ = animals.Select(a => new { a.Id, a.PetName }).Where(x => x.Id != 0).ToList();
        var list = session.Query.All<IAnimal>().Where(a => (string) a["PetName"] == "D").ToList();
        Assert.That(list.Count, Is.EqualTo(1));
        t.Complete();
      }
    }
  }
}