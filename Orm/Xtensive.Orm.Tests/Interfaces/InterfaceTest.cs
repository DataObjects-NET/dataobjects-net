// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.08

using System;
using System.Linq;
using System.Linq.Expressions;
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

  [Serializable]
  [HierarchyRoot]
  public class Person1 : Entity, IPerson
  {
    [Field, Key]
    public int Id { get; private set; }

    public string Name { get; set; }

    public EntitySet<IAnimal> Pets { get; private set; }

    public IAnimal Favorite { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Person2 : Entity, IPerson
  {
    [Field, Key]
    public int Id { get; private set; }

    public string Name { get; set; }

    public EntitySet<IAnimal> Pets { get; private set; }

    public IAnimal Favorite { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Animal1 : Entity, IAnimal
  {
    [Field, Key]
    public int Id { get; private set; }

    public string PetName { get; set; }

    public IPerson Owner { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Animal2 : Entity, IAnimal
  {
    [Field, Key]
    public int Id { get; private set; }

    public string PetName { get; set; }

    public IPerson Owner { get; set; }
  }

  [Serializable]
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
      config.Types.Register(typeof (Person1).Assembly, typeof (Person1).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          IPerson p = new Person1();
          p.Pets.Add(new Animal1());
          p.Pets.Add(new Animal1());
          p.Pets.Add(new Animal2());

          p = new Person2();
          p.Pets.Add(new Animal1());
          p.Pets.Add(new Animal1());
          p.Pets.Add(new Animal2());

          Session.Current.SaveChanges();

          p = session.Query.All<IPerson>().First();
          Assert.AreEqual(3, p.Pets.Count);

          var first = p.Pets.First();
          first.Remove();
          Assert.IsTrue(first.PersistenceState == PersistenceState.Removed);
          Assert.IsFalse(p.Pets.Contains(first));
          Assert.AreEqual(2, p.Pets.Count);

          p.Remove();
          session.Query.All<IAnimal>().Remove();

          new Animal1() { PetName = "A" };
          new Animal1() { PetName = "B" };
          new Animal1() { PetName = "C" };
          new Animal2() { PetName = "D" };
          new Animal2() { PetName = "E" };
          new Animal2() { PetName = "F" };
          new Animal3() { PetName = "G" };
          new Animal3() { PetName = "H" };
          new Animal3() { PetName = "J" };

          var animals = session.Query.All<IAnimal>();
          Assert.AreEqual(9, animals.Count());
          animals.Select(a => new {a.Id, a.PetName}).Where(x => x.Id != 0).ToList();
          var list = session.Query.All<IAnimal>().Where(a => (string)a["PetName"] == "D").ToList();
          Assert.AreEqual(1, list.Count);
          t.Complete();
        }
      }
    }
  }
}