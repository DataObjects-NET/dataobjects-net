// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.10

using System;

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
    public Guid Key { get; set; }

    public Person Customer { get; set; }

    public DateTime ShipDate { get; set; }
  }
}