// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.03

using Xtensive.Core;
using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  /// <summary>
  /// BMW passeger car
  /// </summary>
  public sealed class Bmw : Car
  {
    private int passengers;

    /// <summary>
    /// Gets or sets number of passengers in car.
    /// Attention. Property overrides parent's virtual property.
    /// </summary>
    [Field(IsNullable = true)]
    public override int Passengers
    {
      get { return passengers; }
      set
      {
        ArgumentValidator.EnsureArgumentIsInRange(value, 1, 10, "value");
        passengers = value;
      }
    }
  }
}