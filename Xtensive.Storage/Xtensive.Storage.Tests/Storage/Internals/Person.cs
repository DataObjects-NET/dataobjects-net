// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.27

using Xtensive.Storage.Attributes;
using Xtensive.Storage.Generators;

namespace Xtensive.Storage.Tests.Storage.Internals.Person
{
  [HierarchyRoot(typeof (GuidGenerator), "ID")]
  public class Person : Entity
  {
    [Field]
    public int Age
    {
      get { return GetValue<int>("Age"); }
      set { SetValue("Age", value); }
    }
  }
}