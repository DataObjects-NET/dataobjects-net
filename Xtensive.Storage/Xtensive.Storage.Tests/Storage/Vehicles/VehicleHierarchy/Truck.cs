// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.03

using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  /// <summary>
  /// Truck.
  /// </summary>
  public abstract class Truck : Vehicle
  {
    /// <summary>
    /// Gets or sets truck weight.
    /// Attention. Property is abstract and overrided in inherited classes.
    /// </summary>
    [Field(IsNullable = true)]
    public abstract double Weight { get; set; }

    /// <summary>
    /// Gets or sets truck's length.
    /// </summary>
    [Field(IsNullable = true)]
    public int Length { get; set; }


    /// <summary>
    /// Gets or sets truck power.
    /// Attention. Property is abstract and override in inherited classes.
    /// </summary>
    [Field(IsNullable = true)]
    public abstract int Power { get; set; }    
  }
}