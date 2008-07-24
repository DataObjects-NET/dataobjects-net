// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.03

using Xtensive.Storage.Attributes;
using Xtensive.Storage.Generators;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  /// <summary>
  /// Fleet is a banch of vehicles. 
  /// Usually it groups several vehicles of same type (trucks or passenger cars for example).
  /// It's a base abstract class for <see cref="TruckFleet"/> and <see cref="CarFleet"/> fleets.
  /// </summary>
  [HierarchyRoot(typeof (IncrementalGenerator), "Id")]
  [Index("Name", "Code")]
  public abstract class Fleet : Entity
  {
    [Field]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets division this fleet correspond to.
    /// </summary>
    [Field(IsNullable = true)]
    public Division Division
    {
      get { return GetValue<Division>("Division"); }
      set { SetValue("Division", value); }
    }

    /// <summary>
    /// Gets or sets fleet name.
    /// </summary>
    [Field(Length = 500)]
    public string Name
    {
      get { return GetValue<string>("Name"); }
      set { SetValue("Name", value); }
    }

    /// <summary>
    /// Gets or sets fleet code.
    /// </summary>
    [Field(Length = 500, IsNullable = true)]
    public string Code
    {
      get { return GetValue<string>("Name"); }
      set { SetValue("Name", value); }
    }

    /// <summary>
    /// Gets collection of vehicles of this fleet. Attention! It overrided in inherited classes with "new" instruction.
    /// </summary>
    [Field]
    public EntitySet<Vehicle> Vehicles { get; set; }
  }
}