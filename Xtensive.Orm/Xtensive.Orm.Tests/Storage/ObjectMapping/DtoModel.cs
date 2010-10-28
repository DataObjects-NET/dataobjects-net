// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.16

using System;
using System.Collections.Generic;
using Xtensive.Orm.ObjectMapping;

namespace Xtensive.Orm.Tests.Storage.ObjectMapping.Model
{
  [Serializable]
  public class IdentifiableDto
  {
    public string Key {get; set; }
  }

  [Serializable]
  public abstract class AbstractProductDto : IdentifiableDto
  {
    public string Name { get; set; }
  }

  [Serializable]
  public class PersonalProductDto : AbstractProductDto,
    ICloneable
  {
    public EmployeeDto Employee { get; set; }
    public object Clone()
    {
      return new PersonalProductDto {Employee = Employee==null ? null : (EmployeeDto) Employee.Clone(),
        Key = Key, Name = Name};
    }
  }

  [Serializable]
  public abstract class PersonDto : IdentifiableDto
  {
    public string Name { get; set; }
  }

  [Serializable]
  public abstract class AdvancedPersonDto : PersonDto
  {
    public int Age { get; set; }
  }

  [Serializable]
  public class EmployeeDto : AdvancedPersonDto,
    ICloneable
  {
    public string Position { get; set; }

    public object Clone()
    {
      return new EmployeeDto {Age = Age, Key = Key, Name = Name, Position = Position};
    }
  }

  [Serializable]
  public class PublisherDto : IdentifiableDto
  {
    public string Trademark { get; set; }

    public HashSet<BookShopDto> Distributors { get; set; }

    public string Country { get; set; }
  }

  [Serializable]
  public class BookShopDto : IdentifiableDto
  {
    public string Url { get; set; }

    public string Name { get; set; }

    public PublisherDto[] Suppliers { get; set; }
  }

  [Serializable]
  public class SimplePersonDto : PersonDto
  {}

  [Serializable]
  public class ApartmentDto : IdentifiableDto
  {
    public SimplePersonDto Person { get; set; }

    public AddressDto Address { get; set; }

    public ApartmentDescriptionDto Description { get; set; }
  }

  [Serializable]
  public struct AddressDto
  {
    public string Country { get; set; }

    public string City { get; set; }

    public string Street { get; set; }

    public int Building { get; set; }

    public int Office { get; set; }
  }

  [Serializable]
  public struct ApartmentDescriptionDto
  {
    public double RentalFee { get; set; }

    public double Area { get; set; }

    public SimplePersonDto Manager { get; set; }
  }

  [Serializable]
  public class PersonWithVersionDto : PersonDto,
    IHasBinaryVersion
  {
    public byte[] Version { get; set; }
  }

  [Serializable]
  public class CustomPersonDto : PersonDto
  {
    public int Id { get; set; }

    public string AuxString { get; set; }
  }

  [Serializable]
  public class CompositeKeyRootDto
  {
    public string Key { get; set; }

    public CompositeKeyFirstLevel0Dto FirstId { get; set; }

    public CompositeKeySecondLevel0Dto SecondId { get; set; }

    public DateTime? Aux { get; set; }
  }

  [Serializable]
  public class CompositeKeyFirstLevel0Dto
  {
    public string Key { get; set; }

    public Guid FirstId { get; set; }

    public CompositeKeyFirstLevel1Dto SecondId { get; set; }

    public string Aux { get; set; }
  }

  [Serializable]
  public class CompositeKeyFirstLevel1Dto
  {
    public string Key { get; set; }

    public Guid FirstId { get; set; }

    public DateTime SecondId { get; set; }

    public int Aux { get; set; }
  }

  [Serializable]
  public class CompositeKeySecondLevel0Dto
  {
    public string Key { get; set; }

    public int FirstId { get; set; }

    public string SecondId { get; set; }

    public CompositeKeyFirstLevel0Dto Reference { get; set; }

    public int Aux { get; set; }
  }
}