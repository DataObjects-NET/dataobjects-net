// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.08

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Interfaces.InterfaceTest_Model;

namespace Xtensive.Storage.Tests.Interfaces.InterfaceTest_Model
{
  public interface IPerson : IEntity
  {
    string Name { get; set; }

    EntitySet<IAnimal> Pets { get; }

    IAnimal Favorite { get; set; }
  }

  public interface IAnimal : IEntity
  {
    string PetName { get; set; }

    IPerson Owner { get; set; }
  }

  [HierarchyRoot]
  public class Person1 : Entity, IPerson
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public EntitySet<IAnimal> Pets { get; private set; }

    [Field]
    public IAnimal Favorite { get; set; }
  }

  [HierarchyRoot]
  public class Person2 : Entity, IPerson
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public EntitySet<IAnimal> Pets { get; private set; }

    [Field]
    public IAnimal Favorite { get; set; }
  }

  [HierarchyRoot]
  public class Animal1 : Entity, IAnimal
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string PetName { get; set; }

    [Field]
    public IPerson Owner { get; set; }
  }

  [HierarchyRoot]
  public class Animal2 : Entity, IAnimal
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string PetName { get; set; }

    [Field]
    public IPerson Owner { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Interfaces
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
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          

          // Rollback
        }
      }
    }
  }
}