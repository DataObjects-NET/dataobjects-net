// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.24

using Xtensive.Storage.Attributes;
using Xtensive.Storage.KeyProviders;

namespace Xtensive.Storage.Tests.Storage.Vehicles.Debug
{
  [HierarchyRoot(typeof (Int64Provider), "Id")]
  public class DebugDivision : Entity
  {
    [Field]
    public long Id { get; set; }

    [Field]
    public string Name { get; set; }
  }
}