// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.10

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Core.Tests.ObjectMapping.TargetModel
{
  public class PersonDto : ICloneable
  {
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime BirthDate { get; set; }

    public virtual object Clone()
    {
      return new PersonDto {BirthDate = BirthDate, FirstName = FirstName, Id = Id, LastName = LastName};
    }
  }

  public class OrderDto : ICloneable
  {
    public String Id { get; set; }

    public PersonDto Customer { get; set; }

    public DateTime ShipDate { get; set; }

    public object Clone()
    {
      return new OrderDto {Customer = (PersonDto) Customer.Clone(), Id = Id, ShipDate = ShipDate};
    }
  }

  public class AuthorDto : ICloneable
  {
    public Guid Id { get; set; }

    public string Name { get; set; }

    public BookDto Book { get; set; }

    public object Clone()
    {
      return new AuthorDto {Book = (BookDto) Book.Clone(), Id = Id, Name = Name};
    }
  }

  public class BookDto : ICloneable
  {
    public string ISBN { get; set; }

    public TitleDto Title { get; set; }

    public string TitleText { get; set; }

    public double Price { get; set; }

    public object Clone()
    {
      return new BookDto {ISBN = ISBN, Price = Price, Title = (TitleDto) Title.Clone(), TitleText = TitleText};
    }
  }

  public class TitleDto : ICloneable
  {
    public Guid Id { get; set; }

    public string Text { get; set; }

    public object Clone()
    {
      return new TitleDto {Id = Id, Text = Text};
    }
  }

  public class PetOwnerDto : PersonDto
  {
    public List<AnimalDto> Pets { get; set; }

    public override object Clone()
    {
      var result = new PetOwnerDto
      {
        BirthDate = BirthDate, FirstName = FirstName, Id = Id, LastName = LastName,
        Pets = Pets != null
          ? new List<AnimalDto>(Pets.Select(a => a != null ? (AnimalDto) a.Clone() : null))
          :null
      };
      return result;
    }
  }

  public class AnimalDto : ICloneable
  {
    public Guid Id { get; set; }
    
    public string Name { get; set; }

    public object Clone()
    {
      return new AnimalDto {Id = Id, Name = Name};
    }
  }

  public class CreatureDto
  {
    public Guid Id { get; set; }

    public string Name { get; set; }
  }

  public class InsectDto : CreatureDto
  {
    public int LegPairCount { get; set; }
  }

  public class FlyingInsectDto : InsectDto
  {
    public int WingPairCount { get; set; }
  }

  public class LongBeeDto : FlyingInsectDto
  {
    public int StripCount { get; set; }

    public double Length { get; set; }
  }

  public class MammalDto : CreatureDto
  {
    public bool HasHair { get; set; }

    public string Color { get; set; }
  }

  public class CatDto : MammalDto
  {
    public string Breed { get; set; }
  }

  public class DolphinDto : MammalDto
  {
    public string OceanAreal { get; set; }
  }

  public class IgnorableDto : ICloneable
  {
    public Guid Id { get; set; }

    public string Auxiliary { get; set; }

    public string Ignored { get; set; }

    public IgnorableSubordinateDto IgnoredReference { get; set; }

    public IgnorableSubordinateDto IncludedReference { get; set; }

    public object Clone()
    {
      return new IgnorableDto {
        Auxiliary = Auxiliary, Ignored = Ignored,
        IgnoredReference = IgnoredReference != null ? (IgnorableSubordinateDto) IgnoredReference.Clone() : null,
        IncludedReference = IncludedReference != null
          ? (IgnorableSubordinateDto) IncludedReference.Clone()
          : null,
        Id = Id
      };
    }
  }

  public class IgnorableSubordinateDto : ICloneable
  {
    public Guid Id { get; set; }

    public DateTime Date { get; set; }

    public object Clone()
    {
      return new IgnorableSubordinateDto {Date = Date, Id = Id};
    }
  }

  public class RecursiveCompositionDto
  {
    public Guid Id { get; set; }

    public int Level { get; private set; }

    public SimplestDto AuxReference { get; set; }

    public RecursiveCompositionDto Child { get; private set; }
  }

  public class SimplestDto
  {
    public Guid Id { get; set; }

    public SimplestDto Value { get; set; }
  }

  public class PrimitiveCollectionContainerDto
  {
    public Guid Id { get; set; }

    public List<int> Collection { get; set; }
  }

  public class ComplexCollectionContainerDto
  {
    public Guid Id { get; set; }

    public List<PersonDto> Collection { get; set; }
  }

  [Serializable]
  public class StructureContainerDto
  {
    public Guid Id { get; set; }

    public string AuxString { get; set; }

    public StructureDto Structure { get; set; }

    public CompositeStructure0Dto CompositeStructure { get; set; }
  }

  [Serializable]
  public struct StructureDto
  {
    public int Int { get; set; }

    public string String { get; set; }

    public DateTime DateTime { get; set; }
  }

  [Serializable]
  public struct CompositeStructure0Dto
  {
    public int AuxInt { get; set; }

    public CompositeStructure1Dto Structure { get; set; }
  }

  [Serializable]
  public struct CompositeStructure1Dto
  {
    public int AuxInt { get; set; }

    public CompositeStructure2Dto Structure { get; set; }
  }

  [Serializable]
  public struct CompositeStructure2Dto
  {
    public int AuxInt { get; set; }

    public StructureContainerDto StructureContainer { get; set; }
  }
}