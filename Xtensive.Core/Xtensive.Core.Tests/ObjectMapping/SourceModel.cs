// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.10

using System;
using System.Collections.Generic;

namespace Xtensive.Core.Tests.ObjectMapping.SourceModel
{
  public class Person
  {
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime BirthDate { get; set; }
  }

  public class Order
  {
    public Guid Id { get; set; }

    public Person Customer { get; set; }

    public DateTime ShipDate { get; set; }
  }

  public class Author
  {
    public Guid Id { get; set; }

    public string Name { get; set; }

    public Book Book { get; set; }
  }

  public class Book
  {
    public string ISBN { get; set; }

    public Title Title { get; set; }

    public double Price { get; set; }
  }

  public class Title
  {
    public Guid Id { get; set; }

    public string Text { get; set; }
  }

  public class PetOwner : Person
  {
    public HashSet<Animal> Pets { get; private set; }

    public PetOwner()
      : this(new HashSet<Animal>())
    {}

    public PetOwner(HashSet<Animal> pets)
    {
      Pets = pets;
    }
  }

  public class Animal
  {
    public Guid Id { get; private set; }

    public string Name { get; set; }

    public Animal()
    {
      Id = Guid.NewGuid();
    }
  }

  public class Creature
  {
    public Guid Id { get; private set; }

    public string Name { get; set; }

    public Creature()
    {
      Id = Guid.NewGuid();
    }
  }

  public class Insect : Creature
  {
    public int LegPairCount { get; set; }
  }

  public class FlyingInsect : Insect
  {
    public int WingPairCount { get; set; }
  }

  public class LongBee : FlyingInsect
  {
    public int StripCount { get; set; }

    public double Length { get; set; }
  }

  public class Mammal : Creature
  {
    public bool HasHair { get; set; }

    public string Color { get; set; }
  }

  public class Cat : Mammal
  {
    public string Breed { get; set; }
  }

  public class Dolphin : Mammal
  {
    public string OceanAreal { get; set; }
  }

  public class Ignorable
  {
    public Guid Id { get; set; }

    public string Ignored { get; set; }

    public IgnorableSubordinate IgnoredReference { get; set; }

    public IgnorableSubordinate IncludedReference { get; set; }
  }

  public class IgnorableSubordinate
  {
    public Guid Id { get; set; }

    public DateTime Date { get; set; }
  }

  public class RecursiveComposition
  {
    public Guid Id { get; set; }

    public int Level { get; private set; }

    public RecursiveComposition Child { get; private set; }

    public void Compose(int levelCount)
    {
      var current = this;
      for (var i = 0; i < levelCount; i++) {
        current.Child = new RecursiveComposition {Level = Level + i + 1};
        current = current.Child;
      }
    }

    public RecursiveComposition()
    {
      Id = Guid.NewGuid();
    }
  }
}