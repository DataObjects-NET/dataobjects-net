// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.10

using System;

namespace Xtensive.Core.Tests.ObjectMapping.TargetModel
{
  public class PersonDto : ICloneable
  {
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime BirthDate { get; set; }

    public object Clone()
    {
      return new PersonDto {BirthDate = BirthDate, FirstName = FirstName, Id = Id, LastName = LastName};
    }
  }

  public class OrderDto : ICloneable
  {
    public Guid Key { get; set; }

    public PersonDto Customer { get; set; }

    public DateTime ShipDate { get; set; }

    public object Clone()
    {
      return new OrderDto {Customer = (PersonDto) Customer.Clone(), Key = Key, ShipDate = ShipDate};
    }
  }
}