// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.03

using System;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Tests.Storage.Internals;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  /// <summary>
  /// Base abstract class for all vehicles.
  /// </summary>
  [HierarchyRoot(typeof (GuidLongGuidKeyProvider), "Id", "Guid", "Long")]
  public abstract class Vehicle : Entity
  {
    [Field]
    public Guid Id { get; set; }

    [Field]
    public long Guid { get; set; }

    [Field]
    public Guid Long { get; set; }


    [Field]
    public Fleet Fleet
    {
      get { return GetValue<Fleet>("Fleet"); }
      set { SetValue("Fleet", value); }
    }

    [Field]
    public string Model { get; set; }
  }
}