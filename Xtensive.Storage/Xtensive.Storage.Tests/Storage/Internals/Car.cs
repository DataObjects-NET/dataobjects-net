// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.27

using System;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Generators;

namespace Xtensive.Storage.Tests.Storage.Internals.Car
{
  [HierarchyRoot(typeof (GuidGenerator), "ID")]
  public class Car : Entity
  {
    [Field]
    public Guid ID { get; set; }

    [Field]
    public string Name
    {
      get { return GetValue<string>("Name"); }
      set { SetValue("Name", value); }
    }

    public int Price
    {
      get { return Int32.Parse(GetValue<string>("Price")); }
      set { SetValue("Price", value.ToString()); }
    }
  }
}