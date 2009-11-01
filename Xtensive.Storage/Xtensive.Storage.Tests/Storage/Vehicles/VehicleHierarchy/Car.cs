// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.03

using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  /// <summary>
  /// Passenger car.
  /// </summary>
  public abstract class Car : Vehicle
  {
    /// <summary>
    /// Gets or sets number of passengers in car.
    /// Attention. Property is virtual and overrided in inherited classes.
    /// </summary>
    [Field(IsNullable = true)]
    public virtual int Passengers { get; set; }
  }
}