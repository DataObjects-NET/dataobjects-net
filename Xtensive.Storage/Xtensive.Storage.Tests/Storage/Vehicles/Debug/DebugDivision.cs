// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.24

using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Vehicles.Debug
{
  [HierarchyRoot(typeof (DefaultGenerator), "Id")]
  public class DebugDivision : Entity
  {
    [Field]
    public long Id { get; set; }

    [Field]
    public string Name { get; set; }
  }
}