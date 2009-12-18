// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.16

using System;
using System.Collections.Generic;

namespace Xtensive.Storage.Tests.Storage.ObjectMapping.Model
{
  public abstract class AbstractProductDto
  {
    public string Key { get; set; }

    public string Name { get; set; }
  }

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

  public abstract class PersonDto
  {
    public string Key { get; set; }

    public string Name { get; set; }
  }

  public abstract class AdvancedPersonDto : PersonDto
  {
    public int Age { get; set; }
  }

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
  public class PublisherDto
  {
    public string Key { get; set; }

    public string Trademark { get; set; }

    public HashSet<BookShopDto> Distributors { get; set; }

    public string Country { get; set; }
  }

  [Serializable]
  public class BookShopDto
  {
    public string Key { get; set; }

    public string Url { get; set; }

    public string Name { get; set; }

    public PublisherDto[] Suppliers { get; set; }
  }
}