// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.27

using Xtensive.Storage.Attributes;
using Xtensive.Storage.Generators;

namespace Xtensive.Storage.Tests.Storage.Internals.Address
{
  [HierarchyRoot(typeof (GuidGenerator), "ID")]
  public class Address : Entity
  {
    public decimal Age
    {
      get { return decimal.Parse(GetValue<string>("Age")); }
      set { SetValue("Age", value.ToString()); }
    }
  }
}